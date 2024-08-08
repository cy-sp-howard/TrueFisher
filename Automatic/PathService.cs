using Blish_HUD;
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
        public Blish_HUD.Gw2Mumble.PlayerCharacter Character { get => GameService.Gw2Mumble.PlayerCharacter; }
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
    }
}
