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
            ModelComboBox.Items.Add("График");
            ModelComboBox.Items.Add("АЧХ");
            ModelComboBox.Items.Add("ФЧХ");
            ModelComboBox.Items.Add("ХРЗ");
            ModelComboBox.Items.Add("График единицы");
            ModelComboBox.Items.Add("График синус квадрат");
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

            // Построение осей
            DrawAxes(0, 1, 0, 2); // Диапазон OX от 0 до 1, OY от 0 до 2

            if (model == 6) // Если выбрана модель с заданным диапазоном
            {
                List<Polyline> unitGraph = CreateGraphForUnit(0, 1.0 / 6, 1, 1); // Диапазон от 0 до 1/6
                foreach (var part in unitGraph)
                {
                    GraphCanvas.Children.Add(part);
                }
            }
            else if (model == 7)
            {
                DrawSinSquaredGraph(1.0 / 3, 2.0 / 3, 1, 1);
            }
            else
            {
                // Построение остальных графиков
                DrawTransferFunctions(model, FilterTypeComboBox.SelectedIndex);
            }
        }

        private void DrawAxes(double minX, double maxX, double minY, double maxY)
        {
            GraphCanvas.Children.Clear();

            double width = GraphCanvas.Width - 40; // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            // Ось X
            Line xAxis = new Line
            {
                X1 = 20,
                Y1 = height + 20,
                X2 = GraphCanvas.Width - 20,
                Y2 = height + 20,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            // Ось Y
            Line yAxis = new Line
            {
                X1 = 20,
                Y1 = 20,
                X2 = 20,
                Y2 = GraphCanvas.Height - 20,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            GraphCanvas.Children.Add(xAxis);
            GraphCanvas.Children.Add(yAxis);

            // Добавление делений и меток для X
            int divisionsX = 10; // Количество делений
            double stepX = maxX / divisionsX;

            for (int i = 0; i <= divisionsX; i++)
            {
                double x = stepX * i;
                double tickX = x / maxX * width + 20;

                Line xTick = new Line
                {
                    X1 = tickX,
                    Y1 = height + 15,
                    X2 = tickX,
                    Y2 = height + 25,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TextBlock xLabel = new TextBlock
                {
                    Text = x.ToString("0.##"),
                    Margin = new Thickness(tickX - 10, height + 30, 0, 0),
                    FontSize = 10
                };

                GraphCanvas.Children.Add(xTick);
                GraphCanvas.Children.Add(xLabel);
            }

            // Добавление делений и меток для Y
            int divisionsY = 10; // Количество делений для Y
            double stepY = maxY / divisionsY;

            for (int i = 0; i <= divisionsY; i++)
            {
                double y = stepY * i;
                double tickY = height - (y / maxY * height) + 20;

                Line yTick = new Line
                {
                    X1 = 15,
                    Y1 = tickY,
                    X2 = 25,
                    Y2 = tickY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TextBlock yLabel = new TextBlock
                {
                    Text = y.ToString("0.##"),
                    Margin = new Thickness(0, tickY - 10, 0, 0),
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
            List<Polyline> normalizedGraphs = CreateGraph(Brushes.Blue, model, filterType, true, minX, maxX, minY, maxY);
            List<Polyline> denormalizedGraphs = CreateGraph(Brushes.Red, model, filterType, false, minX, maxX, minY, maxY);

            // Добавление графиков на Canvas
            foreach (var graph in normalizedGraphs)
                GraphCanvas.Children.Add(graph);

            foreach (var graph in denormalizedGraphs)
                GraphCanvas.Children.Add(graph);

            // Добавление легенды
            AddLegend();
        }

        private double[] GetGraphBounds(int model, int filterType)
        {
            double minX = 0, maxX = 10; // Диапазон по умолчанию
            double minY = double.MaxValue, maxY = double.MinValue;

            if (model == 6) // Модель с чётко заданным диапазоном
            {
                minX = 0;
                maxX = 1; // Ограничиваем по X
                minY = 0;
                maxY = 1; // Ограничиваем по Y
            }
            else
            {
                // Расчёт для остальных моделей
                for (double x = minX; x <= maxX; x += 0.1)
                {
                    double normalizedY = CalculateFunction(model, filterType, x, true);
                    double denormalizedY = CalculateFunction(model, filterType, x, false);

                    minY = Math.Min(minY, Math.Min(normalizedY, denormalizedY));
                    maxY = Math.Max(maxY, Math.Max(normalizedY, denormalizedY));
                }

                minY = Math.Floor(minY - 1);
                maxY = Math.Ceiling(maxY + 1);
            }

            return new double[] { minX, maxX, minY, maxY };
        }

        private void DrawSinSquaredGraph(double startX, double endX, double pointX, double pointY)
        {
            double maxY = 1; // Максимальное значение Y для sin^2(wt)
            double maxX = 1; // Максимальное значение X
            double w = 3 * Math.PI; // w = 3π

            double width = GraphCanvas.Width - 40; // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            // Рисуем оси
            DrawAxes(0, maxX, 0, 2); // Ось Y до 2, чтобы график был нагляднее

            // Рисуем график sin^2(wt) на указанном диапазоне
            Polyline graph = new Polyline
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };

            for (double x = startX; x <= endX; x += 0.001)
            {
                double y = Math.Pow(Math.Sin(w * x), 2); // sin^2(wt)

                // Переводим координаты в Canvas
                double canvasX = x / maxX * width + 20;
                double canvasY = height - (y / 2 * height) + 20; // Масштабируем Y под максимум 2

                graph.Points.Add(new Point(canvasX, canvasY));
            }

            // Добавляем график на Canvas
            GraphCanvas.Children.Add(graph);

            // Добавляем точку (1, 1)
            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = pointX / maxX * width + 20;
            double pointCanvasY = height - (pointY / 2 * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3); // Центрируем точку
            Canvas.SetTop(point, pointCanvasY - 3);
            GraphCanvas.Children.Add(point);
        }

        private List<Polyline> CreateGraphForUnit(double startX, double endX, double yValue, double pointX)
        {
            List<Polyline> graphParts = new List<Polyline>();
            Polyline line = new Polyline
            {
                Stroke = Brushes.Green, // Цвет линии
                StrokeThickness = 2
            };

            double width = GraphCanvas.Width - 40; // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            // Линия от startX до endX
            for (double x = startX; x <= endX; x += 0.01)
            {
                double canvasX = x / 1 * width + 20; // Пропорция по X
                double canvasY = height - (yValue / 2 * height) + 20; // Пропорция по Y (до 2)

                line.Points.Add(new Point(canvasX, canvasY));
            }

            graphParts.Add(line);

            // Точка на графике (pointX, yValue)
            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = pointX / 1 * width + 20;
            double pointCanvasY = height - (yValue / 2 * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3); // Центруем точку
            Canvas.SetTop(point, pointCanvasY - 3);

            GraphCanvas.Children.Add(point);

            return graphParts;
        }



        private List<Polyline> CreateGraph(Brush color, int model, int filterType, bool normalized, double minX, double maxX, double minY, double maxY)
        {
            List<Polyline> graphParts = new List<Polyline>();
            Polyline currentPart = new Polyline
            {
                Stroke = color,
                StrokeThickness = 2
            };

            double width = GraphCanvas.Width - 40;  // Учитываем отступы
            double height = GraphCanvas.Height - 40;

            // Вычисление асимптот (обнуление знаменателя)
            List<double> asymptotes = FindAsymptotes(model, filterType);

            // Строим график только для положительных значений X и Y
            for (double x = 0; x <= maxX; x += 0.0001) // Начинаем с 0
            {
                double y = CalculateFunction(model, filterType, x, normalized);

                // Проверяем, близка ли точка к асимптоте
                bool nearAsymptote = asymptotes.Any(a => Math.Abs(x - a) < 0.0001);

                if (double.IsInfinity(y) || double.IsNaN(y) || nearAsymptote || y < 0) // Исключаем отрицательные Y
                {
                    if (currentPart.Points.Count > 0)
                    {
                        graphParts.Add(currentPart); // Завершаем текущую часть
                        currentPart = new Polyline
                        {
                            Stroke = color,
                            StrokeThickness = 2
                        };
                    }
                    continue;
                }

                // Переводим координаты в Canvas
                double canvasX = x / maxX * width + 20; // Начинаем с 20 (отступ)
                double canvasY = height - (y / maxY * height) + 20; // Учитываем отступ сверху

                // Ограничиваем точку только областью серого блока
                if (canvasX >= 20 && canvasX <= GraphCanvas.Width - 20 && canvasY >= 20 && canvasY <= GraphCanvas.Height - 20)
                {
                    currentPart.Points.Add(new Point(canvasX, canvasY));
                }
                else
                {
                    if (currentPart.Points.Count > 0)
                    {
                        graphParts.Add(currentPart); // Завершаем текущую часть
                        currentPart = new Polyline
                        {
                            Stroke = color,
                            StrokeThickness = 2
                        };
                    }
                }
            }

            // Добавляем последнюю часть графика
            if (currentPart.Points.Count > 0)
            {
                graphParts.Add(currentPart);
            }

            return graphParts;
        }




        private List<double> FindAsymptotes(int model, int filterType)
        {
            List<double> asymptotes = new List<double>();

            if (model == 2)
            {
                double o = 2.201216298;
                double a1 = 0.8379138549;
                double a2 = 0.3071740531;
                double c = 4.472957086;
                double b = 1.0947347065;

                // Первая часть знаменателя: (p - a1) = 0
                asymptotes.Add(a1);

                // Вторая часть знаменателя: (p^2 - 2a2p + a2^2 + b^2) = 0
                double discriminant = 4 * a2 * a2 - 4 * (a2 * a2 + b * b);
                if (discriminant >= 0)
                {
                    double root1 = (2 * a2 + Math.Sqrt(discriminant)) / 2;
                    double root2 = (2 * a2 - Math.Sqrt(discriminant)) / 2;
                    asymptotes.Add(root1);
                    asymptotes.Add(root2);
                }
            }

            // Возвращаем отсортированный список асимптот
            return asymptotes.Distinct().OrderBy(a => a).ToList();
        }

        private double CalculateFunction(int model, int filterType, double p, bool normalized)
        {
            //с-0325-31
            double o = 2.201216298;
            double a1 = 0.8379138549;
            double a2 = 0.3071740531;
            double c = 4.472957086;
            double b = 1.0947347065;


            //c0350-33
            /*
            double o = 2.07648729;
            double a1 = 0.5031863086;
            double a2 = 0.1945487548;
            double c = 8.764948695;
            double b = 0.9694279502;
            */

            /* Даня и Жека 93-20-33
            double o = 2.07648729;
            double a1 = 0.9779905926;
            double a2 = 0.3276465410;
            double c = 3.098877330;
            double b = 1.1468952283;
            */
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
            else if (model == 3)
                return Math.Abs((p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b)));
            else if (model == 4)
            {

            }
            else if (model == 5)
                return 20 * Math.Log10(1 / Math.Abs((p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b))));

            return 0;
        }

        private void AddLegend()
        {
            TextBlock normalizedLegend = new TextBlock
            {
                Text = "Нормированная функция (синяя)",
                Foreground = Brushes.Blue,
                Margin = new Thickness(10, 0, 0, 0)
            };

            TextBlock denormalizedLegend = new TextBlock
            {
                Text = "Денормированная функция (красная)",
                Foreground = Brushes.Red,
                Margin = new Thickness(10, 20, 0, 0)
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
