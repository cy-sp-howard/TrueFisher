using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Utils
{
    internal static class MapUtil
    {
        public static  double GetDistance(double x1, double y1, double x2, double y2)
        {
            double offsetX = x2 - x1;
            double offsetY = y2 - y1;
            return Math.Sqrt(Math.Pow(offsetX, 2) + Math.Pow(offsetY, 2));
        }
        //用來取得是否要轉彎
        public static double GetAngle(Vector2 pos_1,Vector2 pos_2)
        {
          
                // 計算兩個向量與 X 軸正向的夾角
                double angle1 = Math.Atan2(pos_1.Y, pos_1.X);
                double angle2 = Math.Atan2(pos_2.Y, pos_2.X);

                double angleDiff = Math.Abs(angle1 - angle2) * 180 / Math.PI;

                return angleDiff;
         
        }
        //將地圖座標 轉成現在螢幕座標
        public static Vector2 MapPosToScreenPos(Vector3 pos)
        {
            return new Vector2();
        }
    }
}
