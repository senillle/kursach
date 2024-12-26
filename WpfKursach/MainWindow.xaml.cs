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
        private GraphRenderer _graphRenderer;
        private GraphCalculate __graphCalculate;
        private GraphLayout _graphLayout;
        public MainWindow()
        {
            InitializeComponent();
            _graphRenderer = new GraphRenderer();
            __graphCalculate = new GraphCalculate();
            _graphLayout = new GraphLayout();
            PopulateComboBoxes();
        }

        private void PopulateComboBoxes()
        {
            //список моделей
            ModelComboBox.Items.Add("Золотарева-Кауэра");
            ModelComboBox.Items.Add("Чебышева");
            ModelComboBox.Items.Add("График");
            ModelComboBox.Items.Add("АЧХ");
            ModelComboBox.Items.Add("ФЧХ");
            ModelComboBox.Items.Add("ХРЗ");
            ModelComboBox.Items.Add("График единицы");
            ModelComboBox.Items.Add("График синус квадрат");
            ModelComboBox.SelectedIndex = 0;

            //типы фильтров
            FilterTypeComboBox.Items.Add("ФНЧ 1");
            FilterTypeComboBox.Items.Add("ПФ 6");
            FilterTypeComboBox.Items.Add("ФНЧ 9");
            FilterTypeComboBox.SelectedIndex = 0;
        }

        private void BuildGraphButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, выбраны ли модель и тип фильтра
            if (ModelComboBox.SelectedIndex < 0 || FilterTypeComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите модель и тип фильтра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Очистка Canvas перед построением графика
            GraphCanvas.Children.Clear();

            // Получение выбранной модели
            int model = ModelComboBox.SelectedIndex;

            // Получение границ осей из пользовательского ввода
            double minX = double.TryParse(MinXInput.Text, out var parsedMinX) ? parsedMinX : 0;
            double maxX = double.TryParse(MaxXInput.Text, out var parsedMaxX) ? parsedMaxX : 10;
            double minY = double.TryParse(MinYInput.Text, out var parsedMinY) ? parsedMinY : 0;
            double maxY = double.TryParse(MaxYInput.Text, out var parsedMaxY) ? parsedMaxY : 2;

            // Построение осей с указанными границами
            _graphLayout.DrawAxes(GraphCanvas, minX, maxX, minY, maxY);

            // Определение, какой график строить, в зависимости от модели
            if (model == 6)
            {
                // Построение графика для модели "График единицы"
                List<Polyline> unitGraph = _graphRenderer.CreateGraphForUnit(GraphCanvas, 0, 1.0 / 6, 1, 1, minX, maxX, minY, maxY);
                foreach (var part in unitGraph)
                {
                    GraphCanvas.Children.Add(part);
                }
            }
            else if (model == 7)
            {
                // Построение графика для модели "График синус квадрат"
                _graphRenderer.DrawSinSquaredGraph(GraphCanvas, 1.0 / 3, 2.0 / 3, 1, 1, minX, maxX, minY, maxY);
            }
            else
            {
                // Построение графика для остальных моделей
                _graphRenderer.DrawTransferFunctions(GraphCanvas, model, FilterTypeComboBox.SelectedIndex, minX, maxX, minY, maxY);
            }
        }

        private double MinX = 0;
        private double MaxX = 10;
        private double MinY = 0;
        private double MaxY = 2;
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем значения из текстовых полей
                double minX = double.Parse(MinXInput.Text);
                double maxX = double.Parse(MaxXInput.Text);
                double minY = double.Parse(MinYInput.Text);
                double maxY = double.Parse(MaxYInput.Text);

                // Проверяем корректность значений
                if (minX >= maxX || minY >= maxY)
                {
                    MessageBox.Show("Минимальные значения должны быть меньше максимальных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Перерисовываем графики с новыми границами
                GraphCanvas.Children.Clear();
                _graphLayout.DrawAxes(GraphCanvas, minX, maxX, minY, maxY);
                _graphRenderer.DrawTransferFunctions(GraphCanvas, ModelComboBox.SelectedIndex, FilterTypeComboBox.SelectedIndex, minX, maxX, minY, maxY);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите числовые значения для границ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}