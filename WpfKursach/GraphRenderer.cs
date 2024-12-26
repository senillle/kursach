using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfKursach
{
    internal class GraphRenderer
    {
        private GraphLayout _graphLayout = new GraphLayout();
        private GraphCalculate __graphCalculate = new GraphCalculate();

        /// <summary>
        /// График функции для синус квадрат
        /// </summary>
        public void DrawSinSquaredGraph(Canvas canvas, double startX, double endX, double pointX, double pointY, double minX, double maxX, double minY, double maxY)
        {
            double w = 3 * Math.PI;

            double width = canvas.Width - 40;
            double height = canvas.Height - 40;

            _graphLayout.DrawAxes(canvas, minX, maxX, minY, maxY);

            Polyline graph = new Polyline
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };

            for (double x = startX; x <= endX; x += 0.001)
            {
                double y = Math.Pow(Math.Sin(w * x), 2);

                double canvasX = (x - minX) / (maxX - minX) * width + 20;
                double canvasY = height - ((y - minY) / (maxY - minY) * height) + 20;

                graph.Points.Add(new Point(canvasX, canvasY));
            }

            canvas.Children.Add(graph);

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = (pointX - minX) / (maxX - minX) * width + 20;
            double pointCanvasY = height - ((pointY - minY) / (maxY - minY) * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3);
            Canvas.SetTop(point, pointCanvasY - 3);
            canvas.Children.Add(point);
        }

        /// <summary>
        /// График функции для единицы
        /// </summary>
        public List<Polyline> CreateGraphForUnit(Canvas canvas, double startX, double endX, double yValue, double pointX, double minX, double maxX, double minY, double maxY)
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
                double canvasX = (x - minX) / (maxX - minX) * width + 20;
                double canvasY = height - ((yValue - minY) / (maxY - minY) * height) + 20;

                line.Points.Add(new Point(canvasX, canvasY));
            }

            graphParts.Add(line);

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double pointCanvasX = (pointX - minX) / (maxX - minX) * width + 20;
            double pointCanvasY = height - ((yValue - minY) / (maxY - minY) * height) + 20;

            Canvas.SetLeft(point, pointCanvasX - 3);
            Canvas.SetTop(point, pointCanvasY - 3);
            canvas.Children.Add(point);

            return graphParts;
        }

        /// <summary>
        /// Графики функций характеристик
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

            for (double x = minX; x <= maxX; x += 0.0001)
            {
                double y = __graphCalculate.CalculateFunction(model, filterType, x, normalized);

                bool nearAsymptote = asymptotes.Any(a => Math.Abs(x - a) < 0.0001);

                if (double.IsInfinity(y) || double.IsNaN(y) || nearAsymptote || y < minY)
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

                double canvasX = (x - minX) / (maxX - minX) * width + 20;
                double canvasY = height - ((y - minY) / (maxY - minY) * height) + 20;

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
        /// График передаточной функции
        /// </summary>
        public void DrawTransferFunctions(Canvas canvas, int model, int filterType, double minX, double maxX, double minY, double maxY)
        {
            List<Polyline> normalizedGraphs = CreateGraph(canvas, Brushes.Blue, model, filterType, true, minX, maxX, minY, maxY);
            List<Polyline> denormalizedGraphs = CreateGraph(canvas, Brushes.Red, model, filterType, false, minX, maxX, minY, maxY);

            foreach (var graph in normalizedGraphs)
                canvas.Children.Add(graph);

            foreach (var graph in denormalizedGraphs)
                canvas.Children.Add(graph);

            _graphLayout.AddLegend(canvas);
        }
    }
}
