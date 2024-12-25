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
        private GraphRenderer _graphRenders;
        private GraphCalculate __graphCalculate;
        private GraphLayout _graphicLayout;
        public MainWindow()
        {
            InitializeComponent();
            _graphRenders = new GraphRenderer();
            __graphCalculate = new GraphCalculate();
            _graphicLayout = new GraphLayout();
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
            if (ModelComboBox.SelectedIndex < 0 || FilterTypeComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите модель и тип фильтра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GraphCanvas.Children.Clear();
            int model = ModelComboBox.SelectedIndex;
            _graphicLayout.DrawAxes(GraphCanvas, 0, 1, 0, 2); //диапазон OX от 0 до 1, OY от 0 до 2

            if (model == 6)
            {
                List<Polyline> unitGraph = _graphRenders.CreateGraphForUnit(GraphCanvas, 0, 1.0 / 6, 1, 1); //диапазон от 0 до 1/6
                foreach (var part in unitGraph)
                {
                    GraphCanvas.Children.Add(part);
                }
            }
            else if (model == 7)
            {
                _graphRenders.DrawSinSquaredGraph(GraphCanvas, 1.0 / 3, 2.0 / 3, 1, 1);
            }
            else
            {
                _graphRenders.DrawTransferFunctions(GraphCanvas, model, FilterTypeComboBox.SelectedIndex);
            }
        }
    }
}