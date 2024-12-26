using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKursach
{
    internal class GraphCalculate
    {
        /// <summary>
        /// нахождение асимптот, чтобы график не слеплялся
        /// </summary>
        public List<double> FindAsymptotes(int model, int filterType)
        {
            List<double> asymptotes = new List<double>();

            if (model == 2)
            {
                double o = 2.201216298;
                double a1 = 0.8379138549;
                double a2 = 0.3071740531;
                double c = 4.472957086;
                double b = 1.0947347065;

                asymptotes.Add(a1);

                double discriminant = 4 * a2 * a2 - 4 * (a2 * a2 + b * b);
                if (discriminant >= 0)
                {
                    double root1 = (2 * a2 + Math.Sqrt(discriminant)) / 2;
                    double root2 = (2 * a2 - Math.Sqrt(discriminant)) / 2;
                    asymptotes.Add(root1);
                    asymptotes.Add(root2);
                }
            }

            return asymptotes.Distinct().OrderBy(a => a).ToList();
        }

        /// <summary>
        /// нахождение границ графика, чтобы он за пределы серой области не выходил
        /// </summary>
        public double[] GetGraphBounds(int model, int filterType)
        {
            double minX = 0, maxX = 10;
            double minY = double.MaxValue, maxY = double.MinValue;

            if (model == 6)
            {
                minX = 0;
                maxX = 1; //ограничиваем по X
                minY = 0;
                maxY = 1; //ограничиваем по Y
            }
            else
            {
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

        /// <summary>
        /// расчёт фильтров
        /// </summary>
        public double CalculateFunction(int model, int filterType, double p, bool normalized)
        {
            //с0325-31
            double o = 2.201216298;
            double a1 = 0.8379138549;
            double a2 = 0.3071740531;
            double c = 4.472957086;
            double b = 1.0947347065;
            double fd = p;
            double fdMax = 4.2 * 3;

            //c0350-33
            /*
            double o = 2.07648729;
            double a1 = 0.5031863086;
            double a2 = 0.1945487548;
            double c = 8.764948695;
            double b = 0.9694279502;
            */

            //с0320 - 33
            /*с0320-33
            double o = 2.07648729;
            double a1 = 0.9779905926;
            double a2 = 0.3276465410;
            double c = 3.098877330;
            double b = 1.1468952283;
            */

            if (model == 0) //Золотарёва-Кауэра
            {
                switch (filterType)
                {
                    case 0: //1ФНЧ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    case 1: //6ПФ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    case 2: //9ФНЧ
                        return normalized ? 1 / (1 + Math.Abs(p)) : 1 / (1 + 2 * Math.Abs(p));

                    default:
                        throw new ArgumentOutOfRangeException(nameof(filterType), "Недопустимый тип фильтра.");
                }
            }
            else if (model == 1) //Чебышева
            {
                return normalized ? 1 / Math.Sqrt(1 + p * p) : 1 / Math.Sqrt(1 + 4 * p * p);
            }
            else if (model == 2)
                return (p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b));
            else if (model == 3)
                return Math.Abs((p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b)));
            else if (model == 4 && fdMax >= fd)
            {
                double aF = c * (2 * fd * a2 + a1 * fd * fd - a1 * a2 * a2 - a1 * b * b);
                double bF = c * fd * (b * b - fd * fd - a2 * a2 + 2 * a1 * a2);
                Console.WriteLine(aF);
                Console.WriteLine(bF);
                return -1 * Math.Atan(bF / aF);
            }
            else if (model == 5)
                return 20 * Math.Log10(1 / Math.Abs((p * p + o * o) / (c * (p - a1) * (p * p - 2 * a2 * p + a2 * a2 + b * b))));

            return 0;
        }
    }
}
