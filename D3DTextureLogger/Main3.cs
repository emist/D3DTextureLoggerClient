using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using EasyHook;
using D3DTextureLoggerClient;
using SlimDX;
using SlimDX.Direct3D9;

namespace D3DTextureLogger
{

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_ResetDelegate(IntPtr device, ref D3DPRESENT_PARAMETERS presentParameters);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9_DrawIndexedPrimitives(IntPtr device,
    SlimDX.Direct3D9.PrimitiveType primitiveType,
    int baseVertexIndex,
    int minimumVertexIndex,
    int vertexCount,
    int startIndex,
    int primitiveCount);

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DPRESENT_PARAMETERS
    {
        public int BackBufferWidth;
        public int BackBufferHeight;
        public Format BackBufferFormat;
        public int BackBufferCount;

        public MultisampleType MultiSampleType;
        public int MultiSampleQuality;

        public SwapEffect SwapEffect;
        public IntPtr DeviceWindowHandle;
        public bool Windowed;
        public bool EnableAutoDepthStencil;
        public Format AutoDepthStencilFormat;
        public PresentFlags Flags;

        /* FullScreen_RefreshRateInHz must be zero for Windowed mode */
        public int FullScreen_RefreshRateInHz;
        public PresentInterval PresentationInterval;
    }

    public class Main : EasyHook.IEntryPoint
    {
        //Texture RedTexture = null;
        static byte[] red = 
                    {
                        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00
                    };

        D3DLoggerInterface Interface;
        LocalHook DrawIndexedPrimitivesLocalHook;
        LocalHook ResetLocalHook;

        static IntPtr DrawIndexedPrimitivesAddress = IntPtr.Zero;
        static IntPtr ResetAddress = IntPtr.Zero;

        Stack<String> Queue = new Stack<String>();

        public Main(RemoteHooking.IContext InContext, String InChannelName, IntPtr[] addresses)
        {
            // connect to host...
            Interface = RemoteHooking.IpcConnectClient<D3DLoggerInterface>(InChannelName);
            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName, IntPtr[] addresses)
        {
            // install hook...
            try
            {
                DrawIndexedPrimitivesAddress = addresses[2];
                ResetAddress = addresses[4];

                //ResetLocalHook = LocalHook.Create(ResetAddress, new Direct3D9Device_ResetDelegate(ResetHook), this);
                DrawIndexedPrimitivesLocalHook = LocalHook.Create(DrawIndexedPrimitivesAddress, new Direct3D9_DrawIndexedPrimitives(DrawIndexedPrimitivesHook), this);

                DrawIndexedPrimitivesLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);
                //ResetLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);
                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            RemoteHooking.WakeUpProcess();
            // wait for host process termination...
            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (Queue.Count > 0)
                    {
                        String[] Package = null;
                        lock (Queue)
                        {
                            Package = Queue.ToArray();
                            Queue.Clear();
                        }
                        Interface.Message(RemoteHooking.GetCurrentProcessId(), Package);
                    }
                    else
                        Interface.Ping();
                }
            }
            catch (Exception e)
            {
                Interface.ReportException(e);
                return;
            }
        }

        int DrawIndexedPrimitivesHook(IntPtr devicePtr, PrimitiveType primitiveType,
                                        int baseVertexIndex, int minimumVertexIndex,
                                        int numVertices, int startIndex, int primCount)
        {
            int hRet = 0;
            using (Device device = Device.FromPointer(devicePtr))
            {
                try
                {
                    
                    
                    Texture RedTexture = Texture.FromMemory(device, red, Usage.DoNotClip, Pool.Managed);
                    
                    device.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    device.SetTexture(0, RedTexture);
                
                    hRet = device.DrawIndexedPrimitives(primitiveType, baseVertexIndex, minimumVertexIndex,
                                                    numVertices, startIndex, primCount).Code;
                }
                catch (Exception e)
                {
                    Interface.ReportException(e);
                }
                return hRet;
            }
        }

        void Cleanup()
        {
        }

        int ResetHook(IntPtr devicePtr, ref D3DPRESENT_PARAMETERS presentParameters)
        {
            using (Device device = Device.FromPointer(devicePtr))
            {
                PresentParameters pp = new PresentParameters()
                {
                    AutoDepthStencilFormat = (Format)presentParameters.AutoDepthStencilFormat,
                    BackBufferCount = presentParameters.BackBufferCount,
                    BackBufferFormat = (Format)presentParameters.BackBufferFormat,
                    BackBufferHeight = presentParameters.BackBufferHeight,
                    BackBufferWidth = presentParameters.BackBufferWidth,
                    DeviceWindowHandle = presentParameters.DeviceWindowHandle,
                    EnableAutoDepthStencil = presentParameters.EnableAutoDepthStencil,
                    FullScreenRefreshRateInHertz = presentParameters.FullScreen_RefreshRateInHz,
                    Multisample = (MultisampleType)presentParameters.MultiSampleType,
                    MultisampleQuality = presentParameters.MultiSampleQuality,
                    PresentationInterval = (PresentInterval)presentParameters.PresentationInterval,
                    PresentFlags = (PresentFlags)presentParameters.Flags,
                    SwapEffect = (SwapEffect)presentParameters.SwapEffect,
                    Windowed = presentParameters.Windowed
                };
                Cleanup();
                return device.Reset(pp).Code;
            }
        }
    }
}