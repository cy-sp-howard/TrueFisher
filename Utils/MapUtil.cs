using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    internal class MapUtil
    {
        public static  double GetDistance(double x1, double y1, double x2, double y2)
        {
            double offsetX = x2 - x1;
            double offsetY = y2 - y1;
            return Math.Sqrt(Math.Pow(offsetX, 2) + Math.Pow(offsetY, 2));
        }
    }
}
