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


        private List<Mem<IntPtr>> characters { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> playerCharacters { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> avAgents { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> gadgetAvAgents { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> notGadgetAvAgents { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> wpGadgetAvAgents { get; set; } = new List<Mem<IntPtr>>();
        private List<Mem<IntPtr>> in5mgadgetAvAgents { get; set; } = new List<Mem<IntPtr>>();


        private List<modelPos> models { get; set; } = new List<modelPos>();
        private List<modelPos> in5m { get; set; }
        private int waiting = 0;
        public DataService(TrueFisherModule module)
        {
            this.module = module;
            if (GameService.GameIntegration.Gw2Instance.Gw2IsRunning) AgentDLL.InjectDLL();
            GameService.GameIntegration.Gw2Instance.Gw2Started += delegate { AgentDLL.InjectDLL(); };
        }
        public Vector3 ScanAvAgent()
        {
            Write(FishMem.ScanHoleSwitch, [0]);
            Thread.Sleep(50);
            Vector3 selfPos = new Vector3(WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.X),
WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Y),
WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Z));
            IntPtr currentHole = Read<IntPtr>(FishMem.HolesStart).value;
            IntPtr holeEnd = Read<IntPtr>(FishMem.HolesEnd).value;
            IntPtr result = IntPtr.Zero;
            float resultDistance = float.PositiveInfinity;
            Vector3 resultPos = new Vector3(0, 0, 0);
            while (currentHole.ToInt64() < holeEnd.ToInt64())
            {
                byte[] posBytes = MemUtil.ReadMem(DataService.Handle, currentHole, 12, [0xe8]).value;
                float x = BitConverter.ToSingle(posBytes, 0);
                float y = BitConverter.ToSingle(posBytes, 4);
                float z = BitConverter.ToSingle(posBytes, 8);
                Vector3 pos = new Vector3(x, y, z);
                float distance = Vector3.Distance(selfPos, pos);
                if (resultDistance > distance)
                {
                    result = currentHole;
                    resultPos = pos;
                    resultDistance = distance;
                }
                currentHole = IntPtr.Add(currentHole, 0x8);
            }
            return resultPos;

        }
        void GetCharacters()
        {
            waiting += 1;
            var root = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(Address, 0x26F1E40), 8, new List<int>() { 0x38 }).Parse<IntPtr>().value;
            var base1 = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(root, 0x98), 8).Parse<IntPtr>(); //6719af得rax
            var firtAddr = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(base1.value, 0x60), 8).Parse<IntPtr>();
            var lastAddrOffset = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(base1.value, 0x6C), 8).Parse<int>().value * 8;
            //213949F8CF0
            var lastAddr = IntPtr.Add(firtAddr.value, lastAddrOffset);
            var index = 0;
            var currentAddrInt = firtAddr.value.ToInt64();
            var lastAddrInt = lastAddr.ToInt64();
            characters.Clear();
            playerCharacters.Clear();

            while (currentAddrInt < lastAddrInt)
            {
                var addr = IntPtr.Add(firtAddr.value, 8 * index);
                var target = MemUtil.ReadMem(DataService.Handle, addr, 8).Parse<IntPtr>();


                if (target.value != IntPtr.Zero)
                {
                    float x = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(target.value, 0x480), 4).Parse<float>().value;
                    if (x != 0 && Math.Abs(x) > 0.01)
                    {
                        characters.Add(target);
                    }
                    // 有哪些type
                    int type = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(target.value, 0x8 + 0x98), 4).Parse<int>().value;
                    long _type = type & 0xF0000000;
                    if (_type == 0x30000000)
                    {
                        playerCharacters.Add(target);
                    }
                }
                currentAddrInt = addr.ToInt64();
                index += 1;
            }


            waiting -= 1;
        }
        void GetModels()
        {

            waiting += 1;
            models.Clear();
            // Gw2-64.exe+10415C8 - lea rdx,[Gw2-64.exe+275B348]
            IntPtr firstModelParent = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(Address, 0x275B348 + 0x8 + 0x8), 0x8).Parse<IntPtr>().value;
            IntPtr currentModelParent = firstModelParent;
            int currentLoop = 0;
            while (currentModelParent != IntPtr.Zero)
            {
                currentLoop += 1;
                IntPtr validPtr = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(currentModelParent, 0x1b0), 0x8).Parse<IntPtr>().value;
                if (validPtr != IntPtr.Zero)
                {
                    IntPtr modelBasePtr = IntPtr.Add(validPtr, 0x8);
                    IntPtr modelBase = MemUtil.ReadMem(DataService.Handle, modelBasePtr, 0x8).Parse<IntPtr>().value;
                    float x = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(modelBase, 0x104), 0x4).Parse<float>().value;
                    float y = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(modelBase, 0x108), 0x4).Parse<float>().value;
                    float z = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(modelBase, 0x10C), 0x4).Parse<float>().value;
                    float distance = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(modelBase, 0xb4), 0x4).Parse<float>().value;

                    IntPtr characterPosAddr = IntPtr.Add(validPtr, 0x28);
                    if (distance > 0)
                    {
                        var data = new modelPos() { x = x, y = y, z = z, distance = distance, characterPosAddr = characterPosAddr, modelBase = modelBase, modelParent = currentModelParent };

                        models.Add(data);

                    }
                }

                currentModelParent = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(currentModelParent, 0x30 + 0x8), 8).Parse<IntPtr>().value;

            }
            in5m = models.FindAll(item => item.distance < 5);
            waiting -= 1;
        }
        void GetAvAgents()
        {
            waiting += 1;


            avAgents.Clear();
            gadgetAvAgents.Clear();
            notGadgetAvAgents.Clear();
            wpGadgetAvAgents.Clear();
            in5mgadgetAvAgents.Clear();
            IntPtr firstAgentAddressPtr = IntPtr.Add(Address, 0x24DAE90 + 0x68 + 0x8);
            IntPtr agentMaxCountPtr = IntPtr.Add(Address, 0x24DAE90 + 0x68 + 0x14);
            int max = MemUtil.ReadMem(DataService.Handle, agentMaxCountPtr, 0x4).Parse<int>().value;
            IntPtr firstAgentAddress = MemUtil.ReadMem(DataService.Handle, firstAgentAddressPtr, 0x8).Parse<IntPtr>().value;
            for (int i = 0; i < max; i++)
            {
                IntPtr CurrentAgentAddress = IntPtr.Add(firstAgentAddress, 0x8 * i);
                var agent = MemUtil.ReadMem(DataService.Handle, CurrentAgentAddress, 0x8).Parse<IntPtr>();
                if (agent.value != IntPtr.Zero)
                {
                    avAgents.Add(agent);
                    int type = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(agent.value, 0x8 + 0xb8), 0x8, new List<int> { 0x38, 0x8 }).Parse<int>().value;

                    var pos = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(agent.value, 0xe8), 12);
                    float x = BitConverter.ToSingle(pos.value, 0);
                    float y = BitConverter.ToSingle(pos.value, 4);
                    float z = BitConverter.ToSingle(pos.value, 8);
                    Vector3 _pos = new Vector3(x, y, z);
                    Vector3 selfPos = new Vector3(WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.X),
                   WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Y),
                   WorldUtil.WorldToGameCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Z));
                    float distance = Vector3.Distance(_pos, selfPos);
                    if (distance < 5000)
                    {
                        in5mgadgetAvAgents.Add(agent);
                    }
                    if (type == 0xA)
                    {
                        int gadgetType = MemUtil.ReadMem(DataService.Handle, IntPtr.Add(agent.value, 0x8 + 0xb8), 0x4, new List<int> { 0x200 }).Parse<int>().value;
                        if (gadgetType == 19)
                        {
                            wpGadgetAvAgents.Add(agent);
                        }
                        gadgetAvAgents.Add(agent);
                    }
                    else
                    {
                        notGadgetAvAgents.Add(agent);
                    }

                }
            }

            waiting -= 1;
        }
        public void Update(GameTime gameTime)
        {
            if (waiting > 0) return;
            var kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.OemCloseBrackets))
            {
                //GetCharacters();
                //GetModels();
                //GetAvAgents();
                //ScanAvAgent();
            }
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
            byte[] dllBytes = Resource.Agent_debug;
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
        public static MemTrail BaseMemAddr => new(0x18) { BaseAddress = DataService.AgentDLL.AddressData };
        public static MemTrail State => BaseMemAddr.AddOffset(0x40);
        public static MemTrail Fishing => BaseMemAddr.AddOffset(0x178);
        public static MemTrail Progression => BaseMemAddr.AddOffset(0x58);
        public static MemTrail FisPos => BaseMemAddr.AddOffset(0x5C);
        public static MemTrail YellowBarWidth => BaseMemAddr.AddOffset(0x64);
        public static MemTrail UserPos => BaseMemAddr.AddOffset(0x60);
        public static MemTrail InRange => BaseMemAddr.AddOffset(0x68);
        public static MemTrail HolesStart => new(0x68, [0x8]) { BaseAddress = DataService.AgentDLL.AddressData };
        public static MemTrail HolesEnd => new(0x68, [0x10]) { BaseAddress = DataService.AgentDLL.AddressData };
        public static MemTrail ScanHoleSwitch => new(0x38) { BaseAddress = DataService.AgentDLL.AddressData };
    }

    public static class SettingMem
    {
        public static MemTrail DLLReady => new(0) { BaseAddress = DataService.AgentDLL.AddressData };
        public static MemTrail Language => new(0x10, [0]) { BaseAddress = DataService.AgentDLL.AddressData };

        public static MemTrail KeyBind(int index, int bindIndex = 0) => new(bindIndex == 0 ? 0x20 : 0x28, [index * 8, 0]) { BaseAddress = DataService.AgentDLL.AddressData };
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
        public IntPtr BaseAddress = DataService.Address;
        public IReadOnlyList<int> Offset { get => _offset.AsReadOnly(); }
        public int FirstOffset { get; private set; }
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
            return new(FirstOffset, n_offset.ToArray()) { BaseAddress = BaseAddress };
        }
    }
}
