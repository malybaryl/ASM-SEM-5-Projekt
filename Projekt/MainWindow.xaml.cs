using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Projekt
{
    public partial class MainWindow : Window
    {
        // Importowanie funkcji z DLL ASM (użyjemy poprawnej ścieżki i architektury)
        [DllImport(@"C:\Users\kacpe\Desktop\home\Programing\studia\ASM-SEM-5\Projekt\x64\Debug\ModuleAsm.dll")]
        public static extern void DeuteranopiaAsm(IntPtr originalImage, IntPtr processedImage, int pixelCount, int threadCount);
        
        //static extern int MyProc1(int a, int b);

        private Bitmap _originalImage;
        private Bitmap _processedImage;

        public MainWindow()
        {
   

            InitializeComponent();
            // Pobranie liczby wątków procesora i ustawienie wartości początkowej slidera
            int processorThreads = Environment.ProcessorCount;
            threadSlider.Value = processorThreads; // Ustawiamy wartość slidera na liczbę wątków procesora
            threadCount.Text = $"Wybrane wątki: {processorThreads}"; // Wyświetlamy wartość początkową wątku
        }

        // Zamykanie aplikacji przy wybraniu opcji Exit
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Przeciąganie okna
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Wyśrodkowanie okna przy starcie
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            // Rozmiary ekranu
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            // Centrowanie okna
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
            // Zainicjuj liczbę wątków po wczytaniu okna
            int processorThreads = Environment.ProcessorCount;
            threadSlider.Value = processorThreads; // Ustaw wartość slidera
            threadCount.Text = $"Ilość wątków: {processorThreads}"; // Zaktualizuj wyświetlanie
        }

        // Obsługa zmiany wartości slidera
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (threadCount != null)
            {
                int threadValue = (int)threadSlider.Value;
                threadCount.Text = $"Ilość wątków: {threadValue}";
            }
        }

        // Wybór obrazu
        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg)|*.jpg",
                Title = "Wybierz obraz"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                imagePathTextBox.Text = openFileDialog.FileName;
                _originalImage = new Bitmap(openFileDialog.FileName);
                MessageBox.Show("Obraz został załadowany.");
            }
        }

        // Przetwarzanie obrazu na deuteranopię
        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_originalImage == null)
            {
                MessageBox.Show("Najpierw wybierz obraz.");
                return;
            }

            // Pobranie liczby wątków z suwaka
            int threadCount = (int)threadSlider.Value;

            _processedImage = new Bitmap(_originalImage.Width, _originalImage.Height);

            if (asmRadioButton.IsChecked == true) // Radiobutton ASM
            {
                IntPtr originalPtr = _originalImage.GetHbitmap(); // Pobierz wskaźnik do oryginalnego obrazu
                IntPtr processedPtr = _processedImage.GetHbitmap(); // Pobierz wskaźnik do przetworzonego obrazu
                int pixelCount = _originalImage.Width * _originalImage.Height;

                // Wywołanie funkcji ASM
                DeuteranopiaAsm(originalPtr, processedPtr, pixelCount, threadCount);
                MessageBox.Show("Obraz przetworzony w ASM.");
            }
            else if (cSharpRadioButton.IsChecked == true)
            {
                // Symulacja deuteranopii w C#
                _processedImage = SimulateDeuteranopia(_originalImage, threadCount);
                MessageBox.Show("Obraz przetworzony w C#.");
            }
        }

        // Zapis przetworzonego obrazu
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

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
                }

        // Funkcja symulacji deuteranopii
        public static Bitmap SimulateDeuteranopia(Bitmap original, int threads)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap simulatedImage = new Bitmap(width, height, original.PixelFormat);

            // Blokowanie obszaru w pamięci
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            BitmapData originalData = original.LockBits(rect, ImageLockMode.ReadOnly, original.PixelFormat);
            BitmapData simulatedData = simulatedImage.LockBits(rect, ImageLockMode.WriteOnly, simulatedImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(original.PixelFormat) / 8;
            int stride = originalData.Stride;
            IntPtr originalScan0 = originalData.Scan0;
            IntPtr simulatedScan0 = simulatedData.Scan0;

            byte[] originalPixels = new byte[stride * height];
            byte[] simulatedPixels = new byte[stride * height];

            // Kopiowanie danych oryginalnego obrazu do tablicy bajtów
            System.Runtime.InteropServices.Marshal.Copy(originalScan0, originalPixels, 0, originalPixels.Length);

            // Parallel.For dla wielowątkowego przetwarzania obrazu
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

                        // Pobieranie wartości R, G, B z oryginalnego obrazu
                        byte originalB = originalPixels[pixelIndex];
                        byte originalG = originalPixels[pixelIndex + 1];
                        byte originalR = originalPixels[pixelIndex + 2];

                        // Symulowanie deuteranopii poprzez zmniejszenie zielonego kanału i skorygowanie RGB
                        int newR = (int)(originalR * 0.625 + originalG * 0.375);
                        int newG = (int)(originalG * 0.7); // Zielony stłumiony
                        int newB = (int)(originalB * 0.8);

                        newR = Clamp(newR, 0, 255);
                        newG = Clamp(newG, 0, 255);
                        newB = Clamp(newB, 0, 255);

                        // Ustawianie wartości R, G, B w przetworzonym obrazie
                        simulatedPixels[pixelIndex] = (byte)newB;
                        simulatedPixels[pixelIndex + 1] = (byte)newG;
                        simulatedPixels[pixelIndex + 2] = (byte)newR;
                    }
                }
            });

            // Kopiowanie zmodyfikowanych danych pikseli z powrotem do przetworzonego obrazu
            System.Runtime.InteropServices.Marshal.Copy(simulatedPixels, 0, simulatedScan0, simulatedPixels.Length);

            // Odblokowanie pamięci
            original.UnlockBits(originalData);
            simulatedImage.UnlockBits(simulatedData);

            return simulatedImage;

      
        }
    }
}
