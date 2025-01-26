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
using ColorBlindnessCS;

namespace Projekt
{
    public partial class MainWindow : Window
    {
        [DllImport(@"C:\Users\kacpe\Desktop\home\Programing\studia\ASM-SEM-5\Projekt\x64\Debug\ModuleAsm.dll")]
        public static extern void DeuteranopiaAsm(IntPtr originalImage, IntPtr processedImage, int pixelCount, int stride, int blindnessType);

        private Bitmap _originalImage;
        private Bitmap _processedImage;

        private int threadValue;

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
            int processorThreads = Environment.ProcessorCount;          // Get the number of logical processors
            threadSlider.Value = processorThreads;                      // Set slider value to processor count
            threadCount.Text = $"Wybrane wątki: {processorThreads}";    // Display the number of threads
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
                threadValue = (int)threadSlider.Value;                  // Get the slider's value as an integer
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
                imagePathTextBox.Text = openFileDialog.FileName;            // Display selected image path
                _originalImage = new Bitmap(openFileDialog.FileName);       // Load image into _originalImage
                MessageBox.Show("Obraz został załadowany.");                // Show success message                                                            
                imageControl.Source = BitmapToImageSource(_originalImage);  // Showing in GUI processed image
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
                MessageBox.Show("Najpierw wybierz obraz.");                             // Warn if no image is loaded
                return;
            }

            int threadCount = (int)threadSlider.Value;                                  // Get the thread count from the slide
            _processedImage = new Bitmap(_originalImage.Width, _originalImage.Height);  // Create a blank bitmap for processed image

            // Getting chosen type of color blindness from ComboBox
            string selectedColorBlindness = (colorBlindnessComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Check if the ASM processing option is selected
            if (asmRadioButton.IsChecked == true)
            {
                stopwatch.Restart(); // Start stopwatch for ASM
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


                stopwatch.Stop();   // Stop stopwatch for ASM
                TimeSpan elapsed = stopwatch.Elapsed; // Save time
                string preciseTime = $"{elapsed.TotalMilliseconds:F3}";
                asmTimeText.Text = $"{preciseTime} ms";   // Update time w GUI
                MessageBox.Show("Obraz przetworzony w ASM.");   // Success message for ASM
            }

            // Check if the C# processing option is selected
            else if (cSharpRadioButton.IsChecked == true)
            {
                stopwatch.Restart(); // Start stopwatch for C#
                // Calling the appropriate method in C# depending on the selected color blindness mode
                if (selectedColorBlindness == "Deuteranopia")
                {
                    ProcessColorBlindnessCS(0, threadCount);
                }
                else if (selectedColorBlindness == "Protanopia")
                {
                    ProcessColorBlindnessCS(1, threadCount);
                }
                else if (selectedColorBlindness == "Tritanopia")
                {
                    ProcessColorBlindnessCS(2, threadCount);
                }
                stopwatch.Stop();   // Stop stopwatch for C#
                TimeSpan elapsed = stopwatch.Elapsed; // Save time
                string preciseTime = $"{elapsed.TotalMilliseconds:F3}";
                cSharpTimeText.Text = $"{preciseTime} ms";   // Update time w GUI
                MessageBox.Show("Obraz przetworzony w C#.");
            }

            // Showing in GUI processed image
            imageControl.Source = BitmapToImageSource(_processedImage);
        }

        /*
         * Processes the image for a specific type of color blindness using an ASM (Assembly) function.
         * Applies color correction for the entire image in parallel using multiple partitions.
         * The blindnessType parameter specifies the type of color blindness to simulate (e.g., Deuteranopia).
         */
        private void ProcessColorBlindnessAsm(int blindnessType)
        {
            // Check if images are loaded; if not, display an error message and exit the method.
            if (_originalImage == null || _processedImage == null)
            {
                MessageBox.Show("Error: Image not loaded.");
                return;
            }

            // Define the area of the image to process.
            Rectangle rect = new Rectangle(0, 0, _originalImage.Width, _originalImage.Height);

            // Lock the bits of the original and processed images for direct memory access.
            BitmapData originalData = _originalImage.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData processedData = _processedImage.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Calculate the total number of pixels and retrieve the stride (row byte width).
            int pixelCount = _originalImage.Width * _originalImage.Height;
            int stride = originalData.Stride;

            IntPtr originalPtr = originalData.Scan0; // Pointer to the start of the original image data.
            IntPtr processedPtr = processedData.Scan0; // Pointer to the start of the processed image data.

            try
            {
                // Use parallel processing to divide the image into partitions and process them concurrently.
                Parallel.For(0, threadValue, partition =>
                {
                    // Determine the range of rows to process for this partition.
                    int rowsPerPartition = _originalImage.Height / threadValue;
                    int startRow = partition * rowsPerPartition;
                    int endRow = (partition == threadValue - 1) ? _originalImage.Height : startRow + rowsPerPartition;

                    // Calculate the number of pixels in this partition.
                    int partitionPixelCount = (endRow - startRow) * _originalImage.Width;

                    // Compute pointers for this partition's data in the original and processed images.
                    IntPtr originalPartitionPtr = IntPtr.Add(originalPtr, startRow * stride);
                    IntPtr processedPartitionPtr = IntPtr.Add(processedPtr, startRow * stride);

                    // Call the assembly function to process the partition for the specified color blindness type.
                    DeuteranopiaAsm(originalPartitionPtr, processedPartitionPtr, partitionPixelCount, stride, blindnessType);
                });
            }
            catch (Exception ex)
            {
                // Display an error message if the ASM function fails.
                MessageBox.Show($"Error calling the ASM function: {ex.Message}");
            }
            finally
            {
                // Unlock the bits for both images to release resources.
                _originalImage.UnlockBits(originalData);
                _processedImage.UnlockBits(processedData);
            }
        }

        /*
         * Processes the image for a specific type of color blindness using a C# class.
         * This method serves as a wrapper around a managed C# implementation for color blindness simulation.
         * The blindnessType parameter specifies the type of color blindness to simulate (e.g., Deuteranopia).
         * The threadCount parameter determines the number of threads to use for processing.
         */
        private void ProcessColorBlindnessCS(int blindnessType, int threadCount)
        {
            // Check if images are loaded; if not, display an error message and exit the method.
            if (_originalImage == null || _processedImage == null)
            {
                MessageBox.Show("Error: Image not loaded.");
                return;
            }

            // Define the area of the image to process.
            Rectangle rect = new Rectangle(0, 0, _originalImage.Width, _originalImage.Height);

            // Lock the bits of the original and processed images for direct memory access.
            BitmapData originalData = _originalImage.LockBits(rect, ImageLockMode.ReadOnly, _originalImage.PixelFormat);
            BitmapData processedData = _processedImage.LockBits(rect, ImageLockMode.WriteOnly, _originalImage.PixelFormat);

            // Calculate the total number of pixels and retrieve the stride (row byte width).
            int pixelCount = _originalImage.Width * _originalImage.Height;
            int stride = originalData.Stride;

            IntPtr originalPtr = originalData.Scan0; // Pointer to the start of the original image data.
            IntPtr processedPtr = processedData.Scan0; // Pointer to the start of the processed image data.

            // Call the managed C# class to perform the color blindness simulation.
            ColorBlindnessCSClass.Execute(originalPtr, processedPtr, pixelCount, stride, blindnessType, threadCount);

            // Unlock the bits for both images to release resources.
            _originalImage.UnlockBits(originalData);
            _processedImage.UnlockBits(processedData);
        }

        /*
         * Converts a System.Drawing.Bitmap into a WPF-compatible ImageSource.
         * This method allows integration between WinForms-based image processing and WPF UI elements.
         */
        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            // Use a memory stream to temporarily store the bitmap data in BMP format.
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp); // Save the bitmap to the memory stream.
                memory.Position = 0; // Reset the stream position to the beginning.

                BitmapImage bitmapImage = new BitmapImage(); // Create a new BitmapImage.
                bitmapImage.BeginInit(); // Begin initialization of the BitmapImage.
                bitmapImage.StreamSource = memory; // Set the stream source for the BitmapImage.
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Cache the image data.
                bitmapImage.EndInit(); // Complete initialization.

                return bitmapImage; // Return the WPF-compatible ImageSource.
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
            // Number of debug iterations
            const int debugIterations = 5;
            long totalCSharpTime = 0;
            long totalAsmTime = 0;

            // Getting chosen type of color blindness from ComboBox
            string selectedColorBlindness = (colorBlindnessComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            int threadCount = (int)threadSlider.Value;                  // Get the thread count from the slide
            int debugThread = 1;
            for (int j = 1; j <= 7; j++)
            {
                if (j == 1)
                    debugThread = j;
                threadCount = debugThread;
                for (int i = 0; i < debugIterations; i++)
                {
                    stopwatch.Restart();
                    if (selectedColorBlindness == "Deuteranopia")
                    {
                        ProcessColorBlindnessCS(0, threadCount);
                    }
                    else if (selectedColorBlindness == "Protanopia")
                    {
                        ProcessColorBlindnessCS(1, threadCount);
                    }
                    else if (selectedColorBlindness == "Tritanopia")
                    {
                        ProcessColorBlindnessCS(2, threadCount);
                    }
                    stopwatch.Stop();
                    TimeSpan elapsed = stopwatch.Elapsed; // Save time
                    string preciseTime = $"{elapsed.TotalMilliseconds:F3}";
                    Console.WriteLine(debugThread + " c#: " + preciseTime);
                    totalCSharpTime += stopwatch.ElapsedMilliseconds;
                }
                cSharpTime = totalCSharpTime / debugIterations;

                // Debuging ASM method
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
                    TimeSpan elapsed = stopwatch.Elapsed; // Save time
                    string preciseTime = $"{elapsed.TotalMilliseconds:F3}";
                    Console.WriteLine(debugThread + " asm: " + preciseTime);
                    totalAsmTime += stopwatch.ElapsedMilliseconds;
                }
                asmTime = totalAsmTime / debugIterations;
                debugThread = debugThread * 2;
            }


            // Update UI
            cSharpTimeText.Text = $"{cSharpTime} ms";
            asmTimeText.Text = $"{asmTime} ms";
            MessageBox.Show($"Debugowanie zakończone.\nCzas C#: {cSharpTime} ms\nCzas ASM: {asmTime} ms");
            // Showing in GUI processed image
            imageControl.Source = BitmapToImageSource(_processedImage);
        }







    }
}