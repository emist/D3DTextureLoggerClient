using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using EasyHook;
using D3DHookingLibrary;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace D3DTextureLoggerClient
{
    public class D3DLoggerInterface : MarshalByRefObject
    {
        public List<Keys> keys;
        public string primCount, vertCount;
        public bool saveprim = false, automatic = false;
        public bool clearprims = false;
        public bool display = true;
        public bool chamed = false;
        public int TotalPrims = 0;
        public string OutPutDir;

        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("D3DLogger has been installed in target {0}.\r\n", InClientPID);
        }

        public void ToggleClearPrims()
        {
            clearprims = false;
        }

        public void ToggleDisplayPrim()
        {
            display = true;
        }

        public void UpdateTotalPrims(int prims)
        {
            TotalPrims = prims;
        }

        public void clearsaveprim()
        {
            this.saveprim = false;
        }

        public void clearautomatic()
        {
            this.automatic = false;
        }

        public void Togglecham()
        {
            this.chamed = false;
        }

        public void Message(Int32 InClientPID, String[] strings)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                Console.WriteLine(strings[i]);
            }
        }

        public void ClearQueue()
        {
            keys.Clear();
        }

        public void GetKeyPressed(ref Keys key)
        {
            key = Keys.Up;
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported" +
                              " an error:\r\n" + InInfo.ToString());
        }

        public void UpdatePrimandVertCount(string primcount, string vertcount)
        {
            this.primCount = primcount;
            this.vertCount = vertcount;
        }

        public void Ping()
        {
        }
    }

    public class Program
    {
        public static D3DLoggerInterface _interface = new D3DLoggerInterface();
        static String ChannelName = null;
        static D3DHookingLibrary.D3DFuncLookup d3d9Util;
        static D3DHookingLibrary.D3DFuncLookup d3d9xUtil;
        public static string exeName = "";

        static IntPtr[] addresses = new IntPtr[100];

        public static void hook(object parameter)
        {
            string exeName = (string)parameter;
            Int32 TargetPID = 0;
            addresses[0] = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.EndScene);
            addresses[1] = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.SetStreamSource);
            addresses[2] = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.DrawIndexedPrimitive);
            addresses[3] = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.BeginScene);
            addresses[4] = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.Reset);

            if (addresses[0] == null || addresses[1] == null)
                return;

            TargetPID = Convert.ToInt32(d3d9Util.LookUpObject.FindProcessId(exeName));

            RemoteHooking.IpcCreateServer<D3DLoggerInterface>(ref ChannelName, WellKnownObjectMode.SingleCall, _interface);

            Hooking.hook(TargetPID, "Whatever descriptor", "D3DTextureLoggerClient.exe", "D3DTextureLogger.dll",
                         "D3DTextureLogger.dll", ChannelName, addresses);

        }

        static void Main(string[] args)
        {
            _interface.keys = new List<Keys>();

            if ((args.Length != 3))
            {
                Console.WriteLine();
                Console.WriteLine("Usage: D3DTextureLoggerClient.exe %EXENAME% %D3DMODULE% %D3DXMODULE");
                Console.WriteLine();
                return;
            }

            exeName = args[0];

            try
            {
                try
                {
                    Console.WriteLine(args[0]);
                    d3d9Util = new D3DHookingLibrary.D3DFuncLookup(args[0], args[1]);
                }
                catch (ApplicationException)
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

                Application.Run(new Form1());
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
            }
        }
    }
}
