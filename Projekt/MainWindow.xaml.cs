using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Projekt
{
    public partial class MainWindow : Window
    {
        // Importing the ASM function from the external DLL
        [DllImport(@"C:\Users\kacpe\Desktop\home\Programing\studia\ASM-SEM-5\Projekt\x64\Debug\ModuleAsm.dll")]
        public static extern void DeuteranopiaAsm(IntPtr originalImage, IntPtr processedImage, int pixelCount, int stride);

        private Bitmap _originalImage;    // To hold the original image
        private Bitmap _processedImage;   // To hold the processed image

        /*
         * Main constructor for the MainWindow class.
         * Initializes components and sets the default value of threads to the number of processor cores.
         */
        public MainWindow()
        {
            InitializeComponent();
            int processorThreads = Environment.ProcessorCount;   // Get the number of logical processors
            threadSlider.Value = processorThreads;               // Set slider value to processor count
            threadCount.Text = $"Wybrane wątki: {processorThreads}";   // Display the number of threads
        }

        /*
         * Event handler for exiting the application.
         * Shuts down the app when the "Exit" button is clicked.
         */
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();  // Closes the application
        }

        /*
         * Event handler for moving the window by clicking and dragging the mouse.
         * Allows the window to be dragged when the left mouse button is pressed.
         */
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();   // Moves the window
            }
        }

        /*
         * Event handler for when the window is loaded.
         * Centers the window on the screen.
         */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;  // Disable automatic centering
            var screenWidth = SystemParameters.PrimaryScreenWidth;     // Get screen width
            var screenHeight = SystemParameters.PrimaryScreenHeight;   // Get screen height
            this.Left = (screenWidth - this.Width) / 2;                // Center horizontally
            this.Top = (screenHeight - this.Height) / 2;               // Center vertically
        }

        /*
         * Event handler for the slider value change.
         * Updates the displayed thread count when the slider is moved.
         */
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (threadCount != null)
            {
                int threadValue = (int)threadSlider.Value;             // Get the slider's value as an integer
                threadCount.Text = $"Ilość wątków: {threadValue}";      // Display the number of threads
            }
        }

        /*
         * Event handler for selecting an image file.
         * Opens a file dialog and loads the selected image into memory.
         */
        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",     // Filter to show only image files
                Title = "Wybierz obraz"                                // Set the dialog title
            };

            if (openFileDialog.ShowDialog() == true)
            {
                imagePathTextBox.Text = openFileDialog.FileName;        // Display selected image path
                _originalImage = new Bitmap(openFileDialog.FileName);   // Load image into _originalImage
                MessageBox.Show("Obraz został załadowany.");            // Show success message
            }
        }

        /*
         * Event handler for processing the image.
         * Processes the selected image using either the ASM or C# method.
         */
        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Najpierw wybierz obraz.");             // Warn if no image is loaded
                return;
            }

            int threadCount = (int)threadSlider.Value;                  // Get the thread count from the slider

            _processedImage = new Bitmap(_originalImage.Width, _originalImage.Height); // Create a blank bitmap for processed image

            // Check if the ASM processing option is selected
            if (asmRadioButton.IsChecked == true)
            {
                // Ensure images are loaded before processing
                if (_originalImage == null || _processedImage == null)
                {
                    MessageBox.Show("Błąd: Obraz nie został załadowany.");  // Error if images are null
                    return;
                }

                // Lock bits of both the original and processed image to access pixel data directly
                Rectangle rect = new Rectangle(0, 0, _originalImage.Width, _originalImage.Height);
                BitmapData originalData = _originalImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData processedData = _processedImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int pixelCount = _originalImage.Width * _originalImage.Height;  // Total number of pixels
                int stride = originalData.Stride;                               // Image stride
                int rowsPerPartition = _originalImage.Height / Environment.ProcessorCount;  // Divide image into chunks based on CPU cores

                IntPtr originalPtr = originalData.Scan0;                        // Pointer to original image data
                IntPtr processedPtr = processedData.Scan0;                      // Pointer to processed image data

                // Check if pointers are valid
                if (originalPtr == IntPtr.Zero || processedPtr == IntPtr.Zero)
                {
                    MessageBox.Show("Błąd: Nie udało się zablokować obrazów.");   // Error message if locking bits failed
                    _originalImage.UnlockBits(originalData);                     // Unlock bits
                    _processedImage.UnlockBits(processedData);                   // Unlock bits
                    return;
                }

                Console.WriteLine($"Original Image Pointer: {originalPtr}");     // Debug info
                Console.WriteLine($"Processed Image Pointer: {processedPtr}");   // Debug info
                Console.WriteLine($"Pixel Count: {pixelCount}");                 // Debug info
                Console.WriteLine($"Stride: {stride}");                          // Debug info

                try
                {
                    // Use Parallel.For to process each partition in a separate thread
                    Parallel.For(0, Environment.ProcessorCount, partition =>
                    {
                        // Calculate the range of rows for this partition
                        int startRow = partition * rowsPerPartition;
                        int endRow = (partition == Environment.ProcessorCount - 1) ? _originalImage.Height : startRow + rowsPerPartition;

                        int partitionPixelCount = (endRow - startRow) * _originalImage.Width;

                        // Pointers for this partition
                        IntPtr originalPartitionPtr = IntPtr.Add(originalPtr, startRow * stride);
                        IntPtr processedPartitionPtr = IntPtr.Add(processedPtr, startRow * stride);

                        // Call the ASM function to process the partition
                        DeuteranopiaAsm(originalPartitionPtr, processedPartitionPtr, partitionPixelCount, stride);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas wywoływania funkcji ASM: {ex.Message}"); // Handle any ASM errors
                    _originalImage.UnlockBits(originalData);                                // Unlock image bits
                    _processedImage.UnlockBits(processedData);                              // Unlock image bits
                    return;
                }

                // Unlock image bits after processing
                _originalImage.UnlockBits(originalData);
                _processedImage.UnlockBits(processedData);

                MessageBox.Show("Obraz przetworzony w ASM.");   // Success message for ASM
            }

            // Check if the C# processing option is selected
            else if (cSharpRadioButton.IsChecked == true)
            {
                _processedImage = SimulateDeuteranopia(_originalImage, threadCount);   // Process image in C#
                MessageBox.Show("Obraz przetworzony w C#.");   // Success message for C#
            }
        }

        /*
         * Event handler for saving the processed image.
         * Opens a dialog to save the processed image to a file.
         */
        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_processedImage == null)
            {
                MessageBox.Show("Najpierw przetwórz obraz.");   // Error if no processed image to save
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image files (*.jpg)|*.jpg;",         // Filter to save as .jpg
                Title = "Zapisz obraz"                         // Dialog title
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _processedImage.Save(saveFileDialog.FileName);  // Save the processed image
                MessageBox.Show("Obraz zapisany.");             // Show success message
            }
        }

        /*
         * Simulates deuteranopia (color blindness) on the provided image using multiple threads.
         * Uses parallelism to process the image more efficiently.
         */
        public static Bitmap SimulateDeuteranopia(Bitmap original, int threads)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap simulatedImage = new Bitmap(width, height, original.PixelFormat);   // Create an empty bitmap

            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);     // Define the region to lock
            BitmapData originalData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
            BitmapData simulatedData = simulatedImage.LockBits(rect, ImageLockMode.WriteOnly, simulatedImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(original.PixelFormat) / 8;    // Calculate bytes per pixel
            int stride = originalData.Stride;                                          // Get stride of the image
            IntPtr originalScan0 = originalData.Scan0;                                 // Pointer to original image data
            IntPtr simulatedScan0 = simulatedData.Scan0;                               // Pointer to simulated image data

            byte[] originalPixels = new byte[stride * height];                         // Byte array to hold original pixel data
            byte[] simulatedPixels = new byte[stride * height];                        // Byte array to hold simulated pixel data

            Marshal.Copy(originalScan0, originalPixels, 0, originalPixels.Length);     // Copy original pixel data to array

            // Parallelize the simulation using multiple threads
            Parallel.For(0, threads, threadIndex =>
            {
                int partitionSize = height / threads;                                  // Divide the image height into partitions
                int start = threadIndex * partitionSize;                               // Start index for each thread
                int end = (threadIndex == threads - 1) ? height : start + partitionSize; // Last thread handles any remaining pixels

                // Process the pixels within the assigned partition
                for (int y = start; y < end; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelIndex = y * stride + x * bytesPerPixel;               // Calculate pixel index

                        byte originalB = originalPixels[pixelIndex];                   // Blue component
                        byte originalG = originalPixels[pixelIndex + 1];               // Green component
                        byte originalR = originalPixels[pixelIndex + 2];               // Red component

                        // Apply color transformation for deuteranopia
                        int newR = (int)(originalR * 0.625 + originalG * 0.375);
                        int newG = (int)(originalG * 0.7);
                        int newB = (int)(originalB * 0.8);

                        // Clamp values to ensure they're within valid RGB range
                        newR = Clamp(newR, 0, 255);
                        newG = Clamp(newG, 0, 255);
                        newB = Clamp(newB, 0, 255);

                        // Store the transformed values in the simulated pixel array
                        simulatedPixels[pixelIndex] = (byte)newB;
                        simulatedPixels[pixelIndex + 1] = (byte)newG;
                        simulatedPixels[pixelIndex + 2] = (byte)newR;
                    }
                }
            });

            Marshal.Copy(simulatedPixels, 0, simulatedScan0, simulatedPixels.Length);  // Copy the processed pixel data back

            original.UnlockBits(originalData);  // Unlock the original image bits
            simulatedImage.UnlockBits(simulatedData);  // Unlock the simulated image bits

            return simulatedImage;  // Return the processed (simulated) image
        }

        /*
         * Clamps an integer value between the provided minimum and maximum values.
         */
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;  // Return the minimum if the value is too low
            if (value > max) return max;  // Return the maximum if the value is too high
            return value;                 // Return the original value if within range
        }
    }
}
