using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework;

namespace BhModule.TrueFisher.Automatic
{
    public class PathService
    {
        private TrueFisherModule module;
        public VirtualKeyShort MoveForward { get => ControlService.GetGameBindButton(SettingMem.MoveForward); }
        public VirtualKeyShort MoveBackward { get => ControlService.GetGameBindButton(SettingMem.MoveBackward); }
        public VirtualKeyShort TurnLeft { get => ControlService.GetGameBindButton(SettingMem.TurnLeft); }
        public VirtualKeyShort TurnRight { get => ControlService.GetGameBindButton(SettingMem.TurnRight); }


        public Blish_HUD.Gw2Mumble.PlayerCharacter Player { get => GameService.Gw2Mumble.PlayerCharacter; }
        public PathService(TrueFisherModule module)
        {
            this.module = module;
        }
        public void Update()
        {

        }
        public void Unload()
        {
        }
        public void Move(Vector2 pos)
        {
            double distance = MapUtil.GetDistance(pos.X, pos.Y, Player.Position.X, Player.Position.Y);
            Keyboard.Stroke(MoveForward);

            double angle = MapUtil.GetAngle(new Vector2(pos.X - Player.Position.X, pos.Y - Player.Position.Y), new Vector2(Player.Forward.X, Player.Forward.Y));
            if (angle > 5) Keyboard.Stroke(TurnLeft);
            else if (angle < -5) Keyboard.Stroke(TurnRight);
        }


    }
}