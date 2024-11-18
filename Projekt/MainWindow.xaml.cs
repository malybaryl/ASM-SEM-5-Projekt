using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace Projekt
{
    public partial class MainWindow : Window
    {
        // Importing the ASM function from the external DLL
        [DllImport(@"C:\Users\kacpe\Desktop\home\Programing\studia\ASM-SEM-5\Projekt\x64\Debug\ModuleAsm.dll")]
        public static extern void DeuteranopiaAsm(IntPtr originalImage, IntPtr processedImage, int pixelCount, int stride, int blindnessType);

        private Bitmap _originalImage;    // To hold the original image
        private Bitmap _processedImage;   // To hold the processed image

        // Zmienne do przechowywania czasu przetwarzania
        private Stopwatch stopwatch = new Stopwatch();
        private long cSharpTime = 0;
        private long asmTime = 0;


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
                imageControl.Source = BitmapToImageSource(_originalImage); // Showing in GUI processed image
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

            int threadCount = (int)threadSlider.Value;                  // Get the thread count from the slide
            _processedImage = new Bitmap(_originalImage.Width, _originalImage.Height); // Create a blank bitmap for processed image

            // Getting chosen type of color blindness from ComboBox
            string selectedColorBlindness = (colorBlindnessComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Check if the ASM processing option is selected
            if (asmRadioButton.IsChecked == true)
            {
                stopwatch.Restart(); // Start pomiaru czasu dla ASM
                                     // Ensure images are loaded before processing
                if (selectedColorBlindness == "Deuteranopia")
                {
                    ProcessColorBlindnessAsm(0);
                }
                else if (selectedColorBlindness == "Protanopia")
                {
                    ProcessColorBlindnessAsm(1);
                }
                else if (selectedColorBlindness == "Tritanopia")
                {
                    ProcessColorBlindnessAsm(2);
                }
                

                stopwatch.Stop();   // Stop pomiaru czasu dla ASM
                asmTime = stopwatch.ElapsedMilliseconds;   // Zapisz czas


                asmTimeText.Text = $"{asmTime} ms";   // Zaktualizuj czas w GUI
                MessageBox.Show("Obraz przetworzony w ASM.");   // Success message for ASM
            }

            // Check if the C# processing option is selected
            else if (cSharpRadioButton.IsChecked == true)
            {
                stopwatch.Restart(); // Start pomiaru czasu dla C#
                // Calling the appropriate method in C# depending on the selected color blindness mode
                if (selectedColorBlindness == "Deuteranopia")
                {
                    _processedImage = SimulateDeuteranopia(_originalImage, threadCount);
                }
                else if (selectedColorBlindness == "Protanopia")
                {
                    _processedImage = SimulateProtanopia(_originalImage, threadCount);
                }
                else if (selectedColorBlindness == "Tritanopia")
                {
                    _processedImage = SimulateTritanopia(_originalImage, threadCount);
                }
                stopwatch.Stop();   // Stop pomiaru czasu dla C#
                cSharpTime = stopwatch.ElapsedMilliseconds; // Zapisz czas

                cSharpTimeText.Text = $"{cSharpTime} ms";   // Zaktualizuj czas w GUI
                MessageBox.Show("Obraz przetworzony w C#.");
            }

            // Showing in GUI processed image
            imageControl.Source = BitmapToImageSource(_processedImage);
        }

        private void ProcessColorBlindnessAsm(int blindnessType)
        {
            if (_originalImage == null || _processedImage == null)
            {
                MessageBox.Show("Błąd: Obraz nie został załadowany.");
                return;
            }

            Rectangle rect = new Rectangle(0, 0, _originalImage.Width, _originalImage.Height);
            BitmapData originalData = _originalImage.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData processedData = _processedImage.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int pixelCount = _originalImage.Width * _originalImage.Height;
            int stride = originalData.Stride;

            IntPtr originalPtr = originalData.Scan0;
            IntPtr processedPtr = processedData.Scan0;

            try
            {
                Parallel.For(0, Environment.ProcessorCount, partition =>
                {
                    int rowsPerPartition = _originalImage.Height / Environment.ProcessorCount;
                    int startRow = partition * rowsPerPartition;
                    int endRow = (partition == Environment.ProcessorCount - 1) ? _originalImage.Height : startRow + rowsPerPartition;

                    int partitionPixelCount = (endRow - startRow) * _originalImage.Width;
                    IntPtr originalPartitionPtr = IntPtr.Add(originalPtr, startRow * stride);
                    IntPtr processedPartitionPtr = IntPtr.Add(processedPtr, startRow * stride);

                    // Pass blindnessType to the ASM function
                    DeuteranopiaAsm(originalPartitionPtr, processedPartitionPtr, partitionPixelCount, stride, blindnessType);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas wywoływania funkcji ASM: {ex.Message}");
            }
            finally
            {
                _originalImage.UnlockBits(originalData);
                _processedImage.UnlockBits(processedData);
            }
        }


        // Konwersja Bitmapy na ImageSource dla wyświetlenia w kontrolce WPF
        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
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

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Najpierw wybierz obraz.");
                return;
            }
            // Liczba powtórzeń debugowania
            const int debugIterations = 5;
            long totalCSharpTime = 0;
            long totalAsmTime = 0;

            // Getting chosen type of color blindness from ComboBox
            string selectedColorBlindness = (colorBlindnessComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            int threadCount = (int)threadSlider.Value;                  // Get the thread count from the slide

            // Debugowanie metody C#
            for (int i = 0; i < debugIterations; i++)
            {
                stopwatch.Restart();
                if (selectedColorBlindness == "Deuteranopia")
                {
                    _processedImage = SimulateDeuteranopia(_originalImage, threadCount);
                }
                else if (selectedColorBlindness == "Protanopia")
                {
                    _processedImage = SimulateProtanopia(_originalImage, threadCount);
                }
                else if (selectedColorBlindness == "Tritanopia")
                {
                    _processedImage = SimulateTritanopia(_originalImage, threadCount);
                }
                stopwatch.Stop();
                totalCSharpTime += stopwatch.ElapsedMilliseconds;
            }
            cSharpTime = totalCSharpTime / debugIterations;

            // Debugowanie metody ASM
            for (int i = 0; i < debugIterations; i++)
            {
                stopwatch.Restart();
                if (selectedColorBlindness == "Deuteranopia")
                {
                    ProcessColorBlindnessAsm(0);
                }
                else if (selectedColorBlindness == "Protanopia")
                {
                    ProcessColorBlindnessAsm(1);
                }
                else if (selectedColorBlindness == "Tritanopia")
                {
                    ProcessColorBlindnessAsm(2);
                }
                stopwatch.Stop();
                totalAsmTime += stopwatch.ElapsedMilliseconds;
            }
            asmTime = totalAsmTime / debugIterations;

            // Aktualizacja UI po debugowaniu
            cSharpTimeText.Text = $"{cSharpTime} ms";
            asmTimeText.Text = $"{asmTime} ms";
            MessageBox.Show($"Debugowanie zakończone.\nCzas C#: {cSharpTime} ms\nCzas ASM: {asmTime} ms");
            // Showing in GUI processed image
            imageControl.Source = BitmapToImageSource(_processedImage);
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

            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(original.PixelFormat) / 8;    // Calculate bytes per pixel
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

        // Simulates Protanopy
public static Bitmap SimulateProtanopia(Bitmap original, int threads)
{
    int width = original.Width;
    int height = original.Height;
    Bitmap simulatedImage = new Bitmap(width, height, original.PixelFormat);

    Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
    BitmapData originalData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
    BitmapData simulatedData = simulatedImage.LockBits(rect, ImageLockMode.WriteOnly, simulatedImage.PixelFormat);

    int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(original.PixelFormat) / 8;
    int stride = originalData.Stride;
    IntPtr originalScan0 = originalData.Scan0;
    IntPtr simulatedScan0 = simulatedData.Scan0;

    byte[] originalPixels = new byte[stride * height];
    byte[] simulatedPixels = new byte[stride * height];

    Marshal.Copy(originalScan0, originalPixels, 0, originalPixels.Length);

    Parallel.For(0, threads, threadIndex =>
    {
        int partitionSize = height / threads;
        int start = threadIndex * partitionSize;
        int end = (threadIndex == threads - 1) ? height : start + partitionSize;

        for (int y = start; y < end; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = y * stride + x * bytesPerPixel;

                byte originalB = originalPixels[pixelIndex];
                byte originalG = originalPixels[pixelIndex + 1];
                byte originalR = originalPixels[pixelIndex + 2];

                int newR = (int)(originalR * 0.567 + originalG * 0.433);
                int newG = (int)(originalG * 0.558);
                int newB = (int)(originalB * 0.0);

                newR = Clamp(newR, 0, 255);
                newG = Clamp(newG, 0, 255);
                newB = Clamp(newB, 0, 255);

                simulatedPixels[pixelIndex] = (byte)newB;
                simulatedPixels[pixelIndex + 1] = (byte)newG;
                simulatedPixels[pixelIndex + 2] = (byte)newR;
            }
        }
    });

    Marshal.Copy(simulatedPixels, 0, simulatedScan0, simulatedPixels.Length);
    original.UnlockBits(originalData);
    simulatedImage.UnlockBits(simulatedData);

    return simulatedImage;
}

// Symulacja tritanopii
public static Bitmap SimulateTritanopia(Bitmap original, int threads)
{
    int width = original.Width;
    int height = original.Height;
    Bitmap simulatedImage = new Bitmap(width, height, original.PixelFormat);

    Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
    BitmapData originalData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
    BitmapData simulatedData = simulatedImage.LockBits(rect, ImageLockMode.WriteOnly, simulatedImage.PixelFormat);

    int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(original.PixelFormat) / 8;
    int stride = originalData.Stride;
    IntPtr originalScan0 = originalData.Scan0;
    IntPtr simulatedScan0 = simulatedData.Scan0;

    byte[] originalPixels = new byte[stride * height];
    byte[] simulatedPixels = new byte[stride * height];

    Marshal.Copy(originalScan0, originalPixels, 0, originalPixels.Length);

    Parallel.For(0, threads, threadIndex =>
    {
        int partitionSize = height / threads;
        int start = threadIndex * partitionSize;
        int end = (threadIndex == threads - 1) ? height : start + partitionSize;

        for (int y = start; y < end; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = y * stride + x * bytesPerPixel;

                byte originalB = originalPixels[pixelIndex];
                byte originalG = originalPixels[pixelIndex + 1];
                byte originalR = originalPixels[pixelIndex + 2];

                int newR = (int)(originalR * 0.95);
                int newG = (int)(originalG * 0.433);
                int newB = (int)(originalB * 0.567);

                newR = Clamp(newR, 0, 255);
                newG = Clamp(newG, 0, 255);
                newB = Clamp(newB, 0, 255);

                simulatedPixels[pixelIndex] = (byte)newB;
                simulatedPixels[pixelIndex + 1] = (byte)newG;
                simulatedPixels[pixelIndex + 2] = (byte)newR;
            }
        }
    });

    Marshal.Copy(simulatedPixels, 0, simulatedScan0, simulatedPixels.Length);
    original.UnlockBits(originalData);
    simulatedImage.UnlockBits(simulatedData);

    return simulatedImage;
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
