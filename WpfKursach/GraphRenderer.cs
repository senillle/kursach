using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfKursach
{
    internal class GraphRenderer
    {
        private GraphLayout _graphicLayout = new GraphLayout();
        private GraphCalculate __graphCalculate = new GraphCalculate();

        /// <summary>
        /// график функции для синус квадрат
        /// </summary>
        public void DrawSinSquaredGraph(Canvas canvas, double startX, double endX, double pointX, double pointY)
        {
            double maxY = 1; //максимальное значение по оси Oy
            double maxX = 1; //максимальное значение по оси Ox
            double w = 3 * Math.PI;

            double width = canvas.Width - 40;
            double height = canvas.Height - 40;

            _graphicLayout.DrawAxes(canvas, 0, maxX, 0, 2);

            Polyline graph = new Polyline
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };

            for (double x = startX; x <= endX; x += 0.001)
            {
                double y = Math.Pow(Math.Sin(w * x), 2);

                double canvasX = x / maxX * width + 20;
                double canvasY = height - (y / 2 * height) + 20;

                graph.Points.Add(new Point(canvasX, canvasY));
            }

            canvas.Children.Add(graph);

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = pointX / maxX * width + 20;
            double pointCanvasY = height - (pointY / 2 * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3);
            Canvas.SetTop(point, pointCanvasY - 3);
            canvas.Children.Add(point);
        }
        /// <summary>
        /// график функции для единицы
        /// </summary>
        public List<Polyline> CreateGraphForUnit(Canvas canvas, double startX, double endX, double yValue, double pointX)
        {
            List<Polyline> graphParts = new List<Polyline>();
            Polyline line = new Polyline
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };

            double width = canvas.Width - 40;
            double height = canvas.Height - 40;

            for (double x = startX; x <= endX; x += 0.01)
            {
                double canvasX = x / 1 * width + 20; 
                double canvasY = height - (yValue / 2 * height) + 20;

                line.Points.Add(new Point(canvasX, canvasY));
            }

            graphParts.Add(line);

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = pointX / 1 * width + 20;
            double pointCanvasY = height - (yValue / 2 * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3); 
            Canvas.SetTop(point, pointCanvasY - 3);

            canvas.Children.Add(point);

            return graphParts;
        }
        /// <summary>
        /// графики функций характеристик, нашего графика, Чебышева и Кауэра
        /// </summary>
        public List<Polyline> CreateGraph(Canvas canvas, Brush color, int model, int filterType, bool normalized, double minX, double maxX, double minY, double maxY)
        {
            List<Polyline> graphParts = new List<Polyline>();
            Polyline currentPart = new Polyline
            {
                Stroke = color,
                StrokeThickness = 2
            };

            double width = canvas.Width - 40;
            double height = canvas.Height - 40;

            List<double> asymptotes = __graphCalculate.FindAsymptotes(model, filterType);

            for (double x = 0; x <= maxX; x += 0.0001)
            {
                double y = __graphCalculate.CalculateFunction(model, filterType, x, normalized);

                bool nearAsymptote = asymptotes.Any(a => Math.Abs(x - a) < 0.0001);

                if (double.IsInfinity(y) || double.IsNaN(y) || nearAsymptote || y < 0) 
                {
                    if (currentPart.Points.Count > 0)
                    {
                        graphParts.Add(currentPart);
                        currentPart = new Polyline
                        {
                            Stroke = color,
                            StrokeThickness = 2
                        };
                    }
                    continue;
                }

                double canvasX = x / maxX * width + 20; 
                double canvasY = height - (y / maxY * height) + 20;

                if (canvasX >= 20 && canvasX <= canvas.Width - 20 && canvasY >= 20 && canvasY <= canvas.Height - 20)
                {
                    currentPart.Points.Add(new Point(canvasX, canvasY));
                }
                else
                {
                    if (currentPart.Points.Count > 0)
                    {
                        graphParts.Add(currentPart);
                        currentPart = new Polyline
                        {
                            Stroke = color,
                            StrokeThickness = 2
                        };
                    }
                }
            }

            if (currentPart.Points.Count > 0)
            {
                graphParts.Add(currentPart);
            }

            return graphParts;
        }
        /// <summary>
        /// график передаточной функции
        /// </summary>
        public void DrawTransferFunctions(Canvas canvas, int model, int filterType)
        {
            double[] bounds = __graphCalculate.GetGraphBounds(model, filterType);
            double minX = bounds[0], maxX = bounds[1], minY = bounds[2], maxY = bounds[3];

            List<Polyline> normalizedGraphs = CreateGraph(canvas, Brushes.Blue, model, filterType, true, minX, maxX, minY, maxY);
            List<Polyline> denormalizedGraphs = CreateGraph(canvas, Brushes.Red, model, filterType, false, minX, maxX, minY, maxY);

            foreach (var graph in normalizedGraphs)
                canvas.Children.Add(graph);

            foreach (var graph in denormalizedGraphs)
                canvas.Children.Add(graph);

            _graphicLayout.AddLegend(canvas);
        }
    }
}
