using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;

namespace BhModule.TrueFisher.Utils
{
    internal static class MapUtil
    {
        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            double offsetX = x2 - x1;
            double offsetY = y2 - y1;
            return Math.Sqrt(Math.Pow(offsetX, 2) + Math.Pow(offsetY, 2));
        }
        public static float GetPlayerDistance(Vector3 pos)
        {
            // pos-> in
            // return-> in
            return Vector3.Distance(GameService.Gw2Mumble.PlayerCharacter.Position / 0.0254f, pos);
        }
        //用來取得是否要轉彎
        public static double GetAngle(Vector2 pos_1, Vector2 pos_2)
        {

            // 計算兩個向量與 X 軸正向的夾角
            double angle1 = Math.Atan2(pos_1.Y, pos_1.X);
            double angle2 = Math.Atan2(pos_2.Y, pos_2.X);

            double angleDiff = Math.Abs(angle1 - angle2) * 180 / Math.PI;

            return angleDiff;

        }
        public static Vector2 MapPosToScreenPos(Vector3 pos)
        {
            var modelMatrix = Matrix.CreateTranslation(pos * 0.0254f); // in to m
            var transformMatrix = Matrix.Multiply(Matrix.Multiply(modelMatrix, GameService.Gw2Mumble.PlayerCamera.View),
                                                       GameService.Gw2Mumble.PlayerCamera.Projection);

            var screenPosition = Vector4.Transform(new Vector3(0, 0, 0), transformMatrix);
            screenPosition /= screenPosition.W;
            int x = (int)((screenPosition.X + 1) * 0.5f * GameService.Graphics.WindowWidth);
            int y = (int)((screenPosition.Y - 1) * 0.5f * -1 * GameService.Graphics.WindowHeight);

            return new Vector2(x, y);
        }
    }
}
