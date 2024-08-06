using BhModule.TrueFisher.Utils;
using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.TrueFisher.Automatic
{
    public class ProcessService
    {
        private Module module;
        public int ID { get => GameService.GameIntegration.Gw2Instance.Gw2Process.Id; }
        public IntPtr Handle { get => MemUtil.AttachProcess(0x001F0FFF, false, GameService.GameIntegration.Gw2Instance.Gw2Process.Id); }
        public IntPtr Address { get => GameService.GameIntegration.Gw2Instance.Gw2Process.MainModule.BaseAddress; }
        public ProcessService(Module module)
        {
            this.module = module;

        }

    }
}
