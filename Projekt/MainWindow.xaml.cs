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
        // Importing the ASM function (updated with correct path)
        [DllImport(@"C:\Users\kacpe\Desktop\home\Programing\studia\ASM-SEM-5\Projekt\x64\Debug\ModuleAsm.dll")]
        public static extern void DeuteranopiaAsm(IntPtr originalImage, IntPtr processedImage, int pixelCount, int stride, int threadCount);

        private Bitmap _originalImage;
        private Bitmap _processedImage;

        public MainWindow()
        {
            InitializeComponent();
            int processorThreads = Environment.ProcessorCount;
            threadSlider.Value = processorThreads;
            threadCount.Text = $"Wybrane wątki: {processorThreads}";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (threadCount != null)
            {
                int threadValue = (int)threadSlider.Value;
                threadCount.Text = $"Ilość wątków: {threadValue}";
            }
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
                Title = "Wybierz obraz"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                imagePathTextBox.Text = openFileDialog.FileName;
                _originalImage = new Bitmap(openFileDialog.FileName);
                MessageBox.Show("Obraz został załadowany.");
            }
        }

        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Najpierw wybierz obraz.");
                return;
            }

            int threadCount = (int)threadSlider.Value;

            _processedImage = new Bitmap(_originalImage.Width, _originalImage.Height);

            if (asmRadioButton.IsChecked == true)
            {
    
                Rectangle rect = new Rectangle(0, 0, _originalImage.Width, _originalImage.Height);
                BitmapData originalData = _originalImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData processedData = _processedImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                
                int pixelCount = _originalImage.Width * _originalImage.Height;
                int stride = originalData.Stride;


                IntPtr originalPtr = originalData.Scan0;
                IntPtr processedPtr = processedData.Scan0;

                Console.WriteLine($"Original Image Pointer: {originalPtr}");
                Console.WriteLine($"Processed Image Pointer: {processedPtr}");
                Console.WriteLine($"Pixel Count: {pixelCount}");
                Console.WriteLine($"Stride: {stride}");

                DeuteranopiaAsm(originalPtr, processedPtr, pixelCount, stride, threadCount);

                _originalImage.UnlockBits(originalData);
                _processedImage.UnlockBits(processedData);

                MessageBox.Show("Obraz przetworzony w ASM.");
            }
            else if (cSharpRadioButton.IsChecked == true)
            {
                _processedImage = SimulateDeuteranopia(_originalImage, threadCount);
                MessageBox.Show("Obraz przetworzony w C#.");
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_processedImage == null)
            {
                MessageBox.Show("Najpierw przetwórz obraz.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
                Title = "Zapisz obraz"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _processedImage.Save(saveFileDialog.FileName);
                MessageBox.Show("Obraz zapisany.");
            }
        }

        public static Bitmap SimulateDeuteranopia(Bitmap original, int threads)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap simulatedImage = new Bitmap(width, height, original.PixelFormat);

            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            BitmapData originalData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
            BitmapData simulatedData = simulatedImage.LockBits(rect, ImageLockMode.WriteOnly, simulatedImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(original.PixelFormat) / 8;
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

                        int newR = (int)(originalR * 0.625 + originalG * 0.375);
                        int newG = (int)(originalG * 0.7);
                        int newB = (int)(originalB * 0.8);

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

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
