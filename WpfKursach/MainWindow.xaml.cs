using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfKursach
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateComboBoxes();
        }

        private void PopulateComboBoxes()
        {
            // Заполнение списка моделей
            ModelComboBox.Items.Add("Золотарева-Кауэра");
            ModelComboBox.Items.Add("Чебышева");
            ModelComboBox.Items.Add("По приколу");
            ModelComboBox.SelectedIndex = 0;

            // Заполнение списка типов фильтров
            FilterTypeComboBox.Items.Add("ФНЧ 1");
            FilterTypeComboBox.Items.Add("ПФ 6");
            FilterTypeComboBox.Items.Add("ФНЧ 9");
            FilterTypeComboBox.SelectedIndex = 0;
        }

        private void BuildGraphButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModelComboBox.SelectedIndex < 0 || FilterTypeComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите модель и тип фильтра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Очистка предыдущего графика
            GraphCanvas.Children.Clear();

            // Определение модели и типа фильтра
            int model = ModelComboBox.SelectedIndex;
            int filterType = FilterTypeComboBox.SelectedIndex;

            // Получение границ графика
            double[] bounds = GetGraphBounds(model, filterType);
            double minX = bounds[0], maxX = bounds[1], minY = bounds[2], maxY = bounds[3];

            // Построение осей
            DrawAxes(minX, maxX, minY, maxY);

            // Построение графиков
            DrawTransferFunctions(model, filterType);
        }


        private void DrawAxes(double minX, double maxX, double minY, double maxY)
        {
            GraphCanvas.Children.Clear();

            double width = GraphCanvas.Width - 40; // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            // Центры для осей
            double centerX = (0 - minX) / (maxX - minX) * width + 20;
            double centerY = height - (0 - minY) / (maxY - minY) * height + 20;

            // Ось X
            Line xAxis = new Line
            {
                X1 = 20,
                Y1 = centerY,
                X2 = GraphCanvas.Width - 20,
                Y2 = centerY,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            // Ось Y
            Line yAxis = new Line
            {
                X1 = centerX,
                Y1 = 20,
                X2 = centerX,
                Y2 = GraphCanvas.Height - 20,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            GraphCanvas.Children.Add(xAxis);
            GraphCanvas.Children.Add(yAxis);

            // Добавление делений и меток для X
            for (double i = Math.Floor(minX); i <= Math.Ceiling(maxX); i++)
            {
                double tickX = (i - minX) / (maxX - minX) * width + 20;
                Line xTick = new Line
                {
                    X1 = tickX,
                    Y1 = centerY - 5,
                    X2 = tickX,
                    Y2 = centerY + 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TextBlock xLabel = new TextBlock
                {
                    Text = i.ToString("0.##"), // Точное значение с двумя знаками после запятой
                    Margin = new Thickness(tickX - 10, centerY + 10, 0, 0),
                    FontSize = 10
                };

                GraphCanvas.Children.Add(xTick);
                GraphCanvas.Children.Add(xLabel);
            }

            // Добавление делений и меток для Y
            for (double i = Math.Floor(minY); i <= Math.Ceiling(maxY); i++)
            {
                double tickY = height - ((i - minY) / (maxY - minY) * height) + 20;

                // Пропускаем деление, если оно близко к нулю
                if (Math.Abs(i) < 1e-10) i = 0;

                Line yTick = new Line
                {
                    X1 = centerX - 5,
                    Y1 = tickY,
                    X2 = centerX + 5,
                    Y2 = tickY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TextBlock yLabel = new TextBlock
                {
                    Text = i.ToString("0.##"), // Точное значение с двумя знаками после запятой
                    Margin = new Thickness(centerX - 30, tickY - 10, 0, 0),
                    FontSize = 10
                };

                GraphCanvas.Children.Add(yTick);
                GraphCanvas.Children.Add(yLabel);
            }
        }


        private void DrawTransferFunctions(int model, int filterType)
        {
            ITransferFunction transferFunction = new TransferFunction();

            // Определение функций
            string normalized = transferFunction.GetNormalizedTransferFunction(model, filterType);
            string denormalized = transferFunction.GetDenormalizedTransferFunction(model, filterType);

            // Получение границ для масштабирования
            double[] bounds = GetGraphBounds(model, filterType);
            double minX = bounds[0], maxX = bounds[1], minY = bounds[2], maxY = bounds[3];

            // Построение графиков
            Polyline normalizedGraph = CreateGraph(Brushes.Blue, model, filterType, true, minX, maxX, minY, maxY);
            Polyline denormalizedGraph = CreateGraph(Brushes.Red, model, filterType, false, minX, maxX, minY, maxY);

            // Добавление легенды
            AddLegend();

            // Добавление графиков на Canvas
            GraphCanvas.Children.Add(normalizedGraph);
            GraphCanvas.Children.Add(denormalizedGraph);
        }


        private double[] GetGraphBounds(int model, int filterType)
        {
            double minX = -10, maxX = 10;
            double minY = double.MaxValue, maxY = double.MinValue;

            for (double x = minX; x <= maxX; x += 0.1)
            {
                double normalizedY = CalculateFunction(model, filterType, x, true);
                double denormalizedY = CalculateFunction(model, filterType, x, false);

                minY = Math.Min(minY, Math.Min(normalizedY, denormalizedY));
                maxY = Math.Max(maxY, Math.Max(normalizedY, denormalizedY));
            }

            // Добавляем небольшое поле для масштабирования
            minY = Math.Floor(minY - 1);
            maxY = Math.Ceiling(maxY + 1);

            return new double[] { minX, maxX, minY, maxY };
        }

        private Polyline CreateGraph(Brush color, int model, int filterType, bool normalized, double minX, double maxX, double minY, double maxY)
        {
            Polyline graph = new Polyline
            {
                Stroke = color,
                StrokeThickness = 2
            };

            double width = GraphCanvas.Width - 40;  // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            Point? previousPoint = null; // Предыдущая точка для проверки разрыва

            for (double x = minX; x <= maxX; x += 0.1)
            {
                double y = CalculateFunction(model, filterType, x, normalized);

                // Исключение точек с большими значениями
                if (Math.Abs(y) > Math.Abs(maxY * 10)) // Если значение выходит далеко за пределы оси
                {
                    previousPoint = null; // Сбрасываем предыдущую точку, чтобы не соединять
                    continue;
                }

                // Переводим координаты в Canvas
                double canvasX = (x - minX) / (maxX - minX) * width + 20;
                double canvasY = height - ((y - minY) / (maxY - minY) * height) + 20;

                Point currentPoint = new Point(canvasX, canvasY);

                // Добавляем текущую точку только если предыдущая существовала
                if (previousPoint.HasValue)
                {
                    graph.Points.Add(currentPoint);
                }

                previousPoint = currentPoint; // Обновляем предыдущую точку
            }

            return graph;
        }



        private double CalculateFunction(int model, int filterType, double p, bool normalized)
        {
            double o = 2.201216298;
            double a1 = 0.8379138549;
            double a2 = 0.3071740531;
            double c = 4.472957086;
            double b = 1.0947347065;
            if (model == 0) // Золотарёва-Кауэра
            {
                switch (filterType)
                {
                    case 0: // 1ФНЧ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    case 1: // 6ПФ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    case 2: // 9ФНЧ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    default:
                        throw new ArgumentOutOfRangeException(nameof(filterType), "Недопустимый тип фильтра.");
                }
            }
            else if (model == 1) // Чебышева
            {
                return normalized ? 1 / Math.Sqrt(1 + p * p) : 1 / Math.Sqrt(1 + 4 * p * p);
            } 
            else if (model == 2)
                return (p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b));

            return 0;
        }

        private void AddLegend()
        {
            TextBlock normalizedLegend = new TextBlock
            {
                Text = "Нормированная функция (синяя)",
                Foreground = Brushes.Blue,
                Margin = new Thickness(10, 550, 0, 0)
            };

            TextBlock denormalizedLegend = new TextBlock
            {
                Text = "Денормированная функция (красная)",
                Foreground = Brushes.Red,
                Margin = new Thickness(10, 570, 0, 0)
            };

            GraphCanvas.Children.Add(normalizedLegend);
            GraphCanvas.Children.Add(denormalizedLegend);
        }
    }

    public interface ITransferFunction
    {
        string GetNormalizedTransferFunction(int model, int filterType);
        string GetDenormalizedTransferFunction(int model, int filterType);
    }

    public class TransferFunction : ITransferFunction
    {
        public string GetNormalizedTransferFunction(int model, int filterType)
        {
            return model == 0 ? $"K{filterType}Н(p) = 1 / (1 + p)" : $"K{filterType}Н(p) = 1 / sqrt(1 + p^2)";
        }

        public string GetDenormalizedTransferFunction(int model, int filterType)
        {
            return model == 0 ? $"K{filterType}Д(p) = 1 / (1 + 2p)" : $"K{filterType}Д(p) = 1 / sqrt(1 + 4p^2)";
        }
    }
}