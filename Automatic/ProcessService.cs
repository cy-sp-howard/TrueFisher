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
        public IntPtr Handle { get; private set; }
        public IntPtr Address { get; private set; } = IntPtr.Zero;
        public ProcessService(Module module)
        {
            this.module = module;
            SetFirstMemAddr();

        }
        private void SetFirstMemAddr()
        {
            Handle = MemUtil.AttachProcess(0x001F0FFF, false, GameService.GameIntegration.Gw2Instance.Gw2Process.Id);
            Address = GameService.GameIntegration.Gw2Instance.Gw2Process.MainModule.BaseAddress;
        }
    }
}
