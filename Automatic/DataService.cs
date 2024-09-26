using AsyncWindowsClipboard.Clipboard.Native;
using BhModule.TrueFisher.Utils;
using Blish_HUD;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Models;
using Blish_HUD.Gw2Mumble;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TmfLib.Pathable;

namespace BhModule.TrueFisher.Automatic
{
    public class DataService
    {

        static public Process Process { get => GameService.GameIntegration.Gw2Instance.Gw2Process; }

        static public IntPtr Handle { get => Process == null ? IntPtr.Zero : Process.Handle; }
        static public IntPtr Address { get => Process == null ? IntPtr.Zero : Process.MainModule.BaseAddress; }

        static public Mem<T> Read<T>(MemTrail trail)
        {
            return MemUtil.ReadMem(DataService.Handle, trail.StartAddress, Marshal.SizeOf<T>(), trail.Offset).Parse<T>();
        }
        static public int Write(MemTrail trail, byte[] val)
        {
            return MemUtil.WriteMem(DataService.Handle, trail.StartAddress, val, trail.Offset);
        }
        static public DLLInject AgentDLL = new DLLInject();

        private TrueFisherModule module;
        public DataService(TrueFisherModule module)
        {
            this.module = module;
            if (GameService.GameIntegration.Gw2Instance.Gw2IsRunning) AgentDLL.InjectDLL();
            GameService.GameIntegration.Gw2Instance.Gw2Started += delegate { AgentDLL.InjectDLL(); };
        }
        public void Update(GameTime gameTime)
        {
        }
        public void Unload()
        {
            AgentDLL.EjectDLL();
        }

    }

    public class DLLInject
    {
        readonly string dllName = "TruefisherAgent.dll";

        public event EventHandler<ChangeEventArgs<bool>> Ready;
        public IntPtr BaseAddress { get => baseAddress; }
        public IntPtr AddressData { get => IntPtr.Add(baseAddress, 0xC1A10); }
        IntPtr baseAddress;
        System.Timers.Timer checkReadyTimer;
        Process process
        {
            get => DataService.Process == null ? null : Process.GetProcessById(DataService.Process.Id);
        }

        IntPtr initDLLFile()
        {
            byte[] dllBytes = Resource.AgentDLL;

            string dllFullPath = Path.GetFullPath(dllName);
            byte[] dllFullPath_bytes = Encoding.ASCII.GetBytes(dllFullPath);
            try
            {
                File.WriteAllBytes(dllFullPath, dllBytes);
            }
            catch { }


            IntPtr dllPathPtr = MemUtil.VirtualAllocEx(process.Handle, IntPtr.Zero, dllFullPath_bytes.Length, 0x1000, 0x04);
            int bytesWritten;
            MemUtil.WriteProcessMemory(process.Handle, dllPathPtr, dllFullPath_bytes, dllFullPath_bytes.Length, out bytesWritten);
            return dllPathPtr;
        }
        public void InjectDLL()
        {
            if (process == null || baseAddress != IntPtr.Zero) return;
            checkInjected(); // if false，check it, because probably default value
            if (baseAddress != IntPtr.Zero) return;
            IntPtr dllPathPtr = initDLLFile();
            IntPtr handle = process.Handle;
            IntPtr loadFunc = MemUtil.GetProcAddress(MemUtil.GetModuleHandle("kernel32"), "LoadLibraryA");

            IntPtr thread = MemUtil.CreateRemoteThread(handle, IntPtr.Zero, 0, loadFunc, dllPathPtr, 0, IntPtr.Zero);
            MemUtil.WaitForSingleObject(thread, 0xFFFFFFFF);
            checkInjected();
            MemUtil.VirtualFreeEx(process.Handle, dllPathPtr, 0, 0x8000);


        }
        public void EjectDLL()
        {
            if (process == null) return;
            if (baseAddress == IntPtr.Zero) return;
            IntPtr handle = process.Handle;
            IntPtr freeFunc = MemUtil.GetProcAddress(MemUtil.GetModuleHandle("kernel32"), "FreeLibrary");
            IntPtr thread = MemUtil.CreateRemoteThread(handle, IntPtr.Zero, 0, freeFunc, baseAddress, 0, IntPtr.Zero);
            MemUtil.WaitForSingleObject(thread, 0xFFFFFFFF);

        }
        void checkInjected()
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == dllName)
                {
                    if (baseAddress == IntPtr.Zero) checkReady();

                    baseAddress = module.BaseAddress;
                    return;
                }
            }
        }
        void checkReady()
        {
            checkReadyTimer = new System.Timers.Timer(3000);
            checkReadyTimer.Elapsed += onReady;

            checkReadyTimer.AutoReset = true;
            checkReadyTimer.Enabled = true;
        }
        void onReady(object sender, EventArgs e)
        {
            bool isReady = DataService.Read<bool>(SettingMem.DLLReady).value;
            if (!isReady) return;
            Ready?.Invoke(this, new(true, false));
            checkReadyTimer.Stop();
        }
    }
    public class modelPos
    {
        public float x;
        public float y;
        public float z;
        public float distance;
        public IntPtr characterPosAddr;
        public IntPtr modelBase;
        public IntPtr modelParent;
    }
    public static class FishMem
    {
        public static MemTrail BaseMemAddr => new(0x18);
        public static MemTrail State => BaseMemAddr.AddOffset(0x40);
        public static MemTrail Fishing => BaseMemAddr.AddOffset(0x178);
        public static MemTrail Progression => BaseMemAddr.AddOffset(0x58);
        public static MemTrail FisPos => BaseMemAddr.AddOffset(0x5C);
        public static MemTrail YellowBarWidth => BaseMemAddr.AddOffset(0x64);
        public static MemTrail UserPos => BaseMemAddr.AddOffset(0x60);
        public static MemTrail InRange => BaseMemAddr.AddOffset(0x68);
        public static MemTrail HolesStart => new(0x68, [0x8]);
        public static MemTrail HolesEnd => new(0x68, [0x10]);
        public static MemTrail Scanned => new(0x38);
    }

    public static class SettingMem
    {
        public static MemTrail DLLReady => new(0);
        public static MemTrail Language => new(0x10, [0]);

        public static MemTrail KeyBind(int index, int bindIndex = 0) => new(bindIndex == 0 ? 0x20 : 0x28, [index * 8, 0]);
        public static readonly int Skill_1 = 0x15;
        public static readonly int Skill_2 = 0x16;
        public static readonly int Skill_3 = 0x17;
        public static readonly int Interact = 0x6D;
        public static readonly int Anchor = 0x1E;
        public static readonly int TurnLeft = 2;
        public static readonly int TurnRight = 3;
        public static readonly int MoveForward = 0;
        public static readonly int MoveBackward = 1;
    }

    public class MemTrail
    {
        public IntPtr StartAddress { get => IntPtr.Add(BaseAddress, FirstOffset); }
        public IntPtr BaseAddress { get => _baseAddress == null? DataService.AgentDLL.AddressData: _baseAddress; }
        public IReadOnlyList<int> Offset { get => _offset.AsReadOnly(); }
        public int FirstOffset { get; private set; }
        public IntPtr _baseAddress;
        private List<int> _offset = new();
        public MemTrail(int firstOffset, int[] offset)
        {
            this.FirstOffset = firstOffset;
            _offset.AddRange(offset);
        }
        public MemTrail(int firstOffset)
        {
            this.FirstOffset = firstOffset;
        }
        public MemTrail AddOffset(int val)
        {
            return AddOffset([val]);
        }
        public MemTrail AddOffset(int[] vals)
        {
            List<int> n_offset = _offset.ToList();
            n_offset.AddRange(vals);
            return new(FirstOffset, n_offset.ToArray()) { _baseAddress = _baseAddress };
        }
    }
}
