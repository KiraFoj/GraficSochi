using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ГрафикСочи3
{
    public partial class MainWindow : Window
    {
        private bool isRunning = false;
        private Random random = new Random();
        private List<double> weatherData = new List<double>();
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            // Запускаем задачу для генерации случайных данных и обновления графика
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => GenerateWeatherData(cancellationTokenSource.Token));

            // Запускаем таймер для обновления TextBox средних значений
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += UpdateAverageTextBox;
            timer.Interval = TimeSpan.FromSeconds(5); // каждые 5 секунд
            timer.Start();
        }

        private async void GenerateWeatherData(CancellationToken cancellationToken)
        {
            isRunning = true;
            weatherData.Add(15);
            while (isRunning && !cancellationToken.IsCancellationRequested)
            {
                // Генерация случайных данных для графика
                double newWeatherValue = weatherData.Last() + random.Next(-5, 6);
                    if (newWeatherValue < 0 ) { newWeatherValue = 0; }
                    if (newWeatherValue > 30 ) { newWeatherValue = 30; }
                weatherData.Add(newWeatherValue);

                // Обновление графика
                UpdateGraph();

                await Task.Delay(1000); // пауза в 1 секунду
            }
            isRunning = false;
        }

        private void UpdateGraph()
        {
            // Очистка Canvas
            weatherCanvas.Dispatcher.Invoke(() => weatherCanvas.Children.Clear());

            // Рисование графика на Canvas
            double canvasWidth = 0;
            double canvasHeight = 0;

            weatherCanvas.Dispatcher.Invoke(() =>
            {
                canvasWidth = weatherCanvas.ActualWidth;
                canvasHeight = weatherCanvas.ActualHeight;

                // Ось X
                var xAxis = new Line
                {
                    X1 = 0,
                    Y1 = canvasHeight,
                    X2 = canvasWidth,
                    Y2 = canvasHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                weatherCanvas.Children.Add(xAxis);

                // Ось Y
                var yAxis = new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = canvasHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                weatherCanvas.Children.Add(yAxis);

                // Метки по оси X
                //for (int i = 0; i < 10; i++)
                //{
                //    double xPos = (canvasWidth / 10) * i;
                //    var label = new TextBlock
                //    {
                //        Text = i.ToString(),
                //        Margin = new Thickness(xPos - 10, canvasHeight, 0, 0)
                //    };
                //    weatherCanvas.Children.Add(label);
                //}

                // Метки по оси Y
                for (int i = 0; i <= 30; i += 10)
                {
                    double yPos = canvasHeight - (canvasHeight / 30) * i;
                    var label = new TextBlock
                    {
                        Text = i.ToString(),
                        Margin = new Thickness(0, yPos - 5, 0, 0)
                    };
                    weatherCanvas.Children.Add(label);
                }
            });

            if (weatherData.Count > 1)
            {
                for (int i = 1; i < weatherData.Count; i++)
                {
                    double x1 = (canvasWidth / (weatherData.Count - 1)) * (i - 1);
                    double y1 = canvasHeight - (canvasHeight * weatherData[i - 1] / 30);

                    double x2 = (canvasWidth / (weatherData.Count - 1)) * i;
                    double y2 = canvasHeight - (canvasHeight * weatherData[i] / 30);

                    // Создание линии и добавление ее на Canvas
                    weatherCanvas.Dispatcher.Invoke(() =>
                    {
                        var line = new Line
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 2
                        };

                        weatherCanvas.Children.Add(line);
                    });
                }
            }
        }

        private void UpdateAverageTextBox(object sender, EventArgs e)
        {
            // Расчет среднего значения и вывод в TextBox
            double average = weatherData.Count > 0 ? weatherData.Average() : 0;
            averageTextBox.Text = $"Среднее значение: {average:F2}";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => GenerateWeatherData(cancellationTokenSource.Token));
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            cancellationTokenSource?.Cancel();
        }
    }
}
