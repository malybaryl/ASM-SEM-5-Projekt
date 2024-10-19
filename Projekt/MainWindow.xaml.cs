using System;
using System.Windows;
using System.Windows.Input;

namespace Projekt
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Pobranie liczby wątków procesora i ustawienie wartości początkowej slidera
            int processorThreads = Environment.ProcessorCount;
            threadSlider.Value = processorThreads; // Ustawiamy wartość slidera na liczbę wątków procesora
            threadCount.Text = $"Wybrane wątki: {processorThreads}"; // Wyświetlamy wartość początkową wątku
        }

        // Zamykanie aplikacji przy wybraniu opcji Exit
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
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
                threadCount.Text = $"Wybrane wątki: {threadValue}";
            }
        }
    }
}
