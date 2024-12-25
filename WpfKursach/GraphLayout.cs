using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace WpfKursach
{
    internal class GraphLayout
    {
        /// <summary>
        /// рисуем оси
        /// </summary>
        public void DrawAxes(Canvas canvas, double minX, double maxX, double minY, double maxY)
        {
            canvas.Children.Clear();

            double width = canvas.Width - 40;
            double height = canvas.Height - 40;

            Line xAxis = new Line
            {
                X1 = 20,
                Y1 = height + 20,
                X2 = canvas.Width - 20,
                Y2 = height + 20,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Line yAxis = new Line
            {
                X1 = 20,
                Y1 = 20,
                X2 = 20,
                Y2 = canvas.Height - 20,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            int divisionsX = 10; //количество делений для X
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

                canvas.Children.Add(xTick);
                canvas.Children.Add(xLabel);
            }

            int divisionsY = 10; //количество делений для Y
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

                canvas.Children.Add(yTick);
                canvas.Children.Add(yLabel);
            }
        }

        /// <summary>
        /// легенда графика
        /// </summary>
        public void AddLegend(Canvas canvas)
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

            canvas.Children.Add(normalizedLegend);
            canvas.Children.Add(denormalizedLegend);
        }
    }
}
