using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Automatic
{
    public class PathService
    {
        private TrueFisherModule module;
        public VirtualKeyShort MoveForward { get => GetGameBindButton(SettingMem.MoveForward); }
        public VirtualKeyShort MoveBackward { get => GetGameBindButton(SettingMem.MoveBackward); }
        public VirtualKeyShort TurnLeft { get => GetGameBindButton(SettingMem.TurnLeft); }
        public VirtualKeyShort TurnRight { get => GetGameBindButton(SettingMem.TurnRight); }

        static public Dictionary<VirtualKeyShort, VirtualKeyShort> GameKeyMap = new() {
            {VirtualKeyShort.ACCEPT,VirtualKeyShort.RIGHT },
            {VirtualKeyShort.NONCONVERT,VirtualKeyShort.LEFT },
        };
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
        static public VirtualKeyShort GameKeyToVirtualKey(VirtualKeyShort key)
        {
            if (GameKeyMap.ContainsKey(key))
            {
                return GameKeyMap[key];
            }
            return key;
        }
        static public VirtualKeyShort GetGameBindButton(MemTrail offset)
        {
            Mem<short> result = GameProcess.Read<short>(offset);
            if (result.value == 0)
            {
                int[] secondKeyOffsetAry = offset.Offset.ToArray();
                secondKeyOffsetAry[secondKeyOffsetAry.Length - 1] = secondKeyOffsetAry[secondKeyOffsetAry.Length - 1] + SettingMem.SecondKeyOffset;
                MemTrail secondKeyTrail = new(offset.FirstOffset, secondKeyOffsetAry);
                return GameKeyToVirtualKey((VirtualKeyShort)GameProcess.Read<short>(secondKeyTrail).value);
            }
            return GameKeyToVirtualKey((VirtualKeyShort)result.value);
        }
    }
}
