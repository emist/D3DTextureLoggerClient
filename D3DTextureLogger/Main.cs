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
using System.Windows.Forms;
using KBHook;
using System.IO;

namespace D3DTextureLogger
{
    /// <summary>
    /// The IDirect3DDevice9.EndScene function definition
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_EndSceneDelegate(IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_BeginSceneDelegate(IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_SetStreamSourceDelegate(IntPtr device, int stream, IntPtr vBuffer,
                                                         int offsetInBytes, int stride);


    /// <summary>
    /// The IDirect3DDevice9.Reset function definition
    /// </summary>
    /// <param name="device"></param>
    /// <param name="presentParameters"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_ResetDelegate(IntPtr device, ref PresentParameters presentParameters);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9Device_SetLightDelegate(IntPtr devicePtr, int index, ref SlimDX.Direct3D9.Light light);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    delegate int Direct3D9_DrawIndexedPrimitives(IntPtr device,
    SlimDX.Direct3D9.PrimitiveType primitiveType,
    int baseVertexIndex,
    int minimumVertexIndex,
    int vertexCount,
    int startIndex,
    int primitiveCount);

    public class Main : EasyHook.IEntryPoint
    {
        /*
        static SlimDX.Color4 MenuLineColor;
        static SlimDX.Color4 FontColor;
        static SlimDX.Direct3D9.Texture RedTexture = null;
        static SlimDX.Direct3D9.Texture OrangeTexture = null;
        static SlimDX.Direct3D9.Font MenuFont = null;
        static SlimDX.Direct3D9.Line MenuLine = null;
         */

        VertexBuffer vb = null;
        static SlimDX.Direct3D9.Texture RedTexture = null;

        static PrimitiveList prims = new PrimitiveList();
        static List<StoredDIP> lastDraw = new List<StoredDIP>();
        Dictionary<int, int> vmap = new Dictionary<int, int>();
        
        static byte[] red = 
                    {
                        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00
                    };
        
        static byte[] orange = 
                    {
                        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA5, 0xFF, 0x00
                    };




        //const int STRIDE = 0;
        const int NUMVERTS = 1;
        const int PRIMCOUNT = 2;
        const int STARTINDEX = 3;
        const int LOGVALUES = 4;
        int g_uiStride = 0;
        List<vertex> vertices = new List<vertex>();

        PixelShader chamPixelShader; 

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        struct ModelRecLogger_t
        {
            public string type;
            public int value;
            public bool isLogging;
        };

        static ModelRecLogger_t[] model =
            new ModelRecLogger_t[]{
                new ModelRecLogger_t() { type = "Stride :", value = 0, isLogging = false },
                new ModelRecLogger_t() { type = "NumVert :", value = 0, isLogging = false },
                new ModelRecLogger_t() { type = "PrimCount :", value = 0, isLogging = false },
                new ModelRecLogger_t() { type = "StartIndex :", value = 0, isLogging = false },
                new ModelRecLogger_t() { type = "Log All Value :", value = 0, isLogging = false },
            };

        static int menuIndex = 0;
        static int incrementBy = 1;
        public struct vertex
        {
            public float x, y, z;
        };


        D3DLoggerInterface Interface;
        //static String outputdir = "C:\\Users\\emist\\Output\\";
        //SlimDX.Direct3D9.Device device = null;

        LocalHook EndSceneLocalHook;
        LocalHook DrawIndexedPrimitivesLocalHook;
        //LocalHook BeginSceneLocalHook;
        LocalHook ResetLocalHook;
        LocalHook SetStreamSourceLocalHook;


        static int stride = 0;

        static IntPtr EndSceneAddress = IntPtr.Zero;
        static IntPtr SetStreamSourceAddress = IntPtr.Zero;
        static IntPtr BeginSceneAddress = IntPtr.Zero;
        static IntPtr DrawIndexedPrimitivesAddress = IntPtr.Zero;
        static IntPtr ResetAddress = IntPtr.Zero;

        Stack<String> Queue = new Stack<String>();

        public Main(RemoteHooking.IContext InContext, String InChannelName, IntPtr[] addresses)
        {
            // connect to host...
            Interface = RemoteHooking.IpcConnectClient<D3DLoggerInterface>(InChannelName);
            Interface.Ping();
        }

        public bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (GetAsyncKeyState(vKey) & 0x8000);
        }

        public int FindSelected()
        {
            if (prims == null)
            {
                Console.WriteLine("Prims is null");
                return -1;
            }

            if (prims.Count < 1)
            {
                Console.WriteLine("Prims is empty");
                return -1;
            }

            foreach (Primitive prim in prims)
            {
                if (prim.Selected)
                    return prims.IndexOf(prim);
            }
            return -1;
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName, IntPtr[] addresses)
        {
            // install hook...

            try
            {
                EndSceneAddress = addresses[0];
                SetStreamSourceAddress = addresses[1];
                DrawIndexedPrimitivesAddress = addresses[2];
                BeginSceneAddress = addresses[3];
                ResetAddress = addresses[4];

                EndSceneLocalHook = LocalHook.Create(EndSceneAddress, new Direct3D9Device_EndSceneDelegate(EndSceneHook), this);
                ResetLocalHook = LocalHook.Create(ResetAddress, new Direct3D9Device_ResetDelegate(ResetHook), this);
                DrawIndexedPrimitivesLocalHook = LocalHook.Create(DrawIndexedPrimitivesAddress, new Direct3D9_DrawIndexedPrimitives(DrawIndexedPrimitivesHook), this);

                SetStreamSourceLocalHook = LocalHook.Create(SetStreamSourceAddress, new Direct3D9Device_SetStreamSourceDelegate(SetStreamSourceHook), this);
                SetStreamSourceLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);

                //BeginSceneLocalHook = LocalHook.Create(BeginSceneAddress, new Direct3D9Device_BeginSceneDelegate(BeginSceneHook), this);
                //BeginSceneLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);


                vertices.Add(new vertex());
                DrawIndexedPrimitivesLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);
                EndSceneLocalHook.ThreadACL.SetExclusiveACL(new Int32[1]);
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

        public void Screenshot(Device device, string filename, int primCount, int numVertices)
        {
            Surface surf;
            using (Surface backbuffer = device.GetBackBuffer(0, 0))
            {
                using (SwapChain sc = device.GetSwapChain(0))
                {
                    surf = Surface.CreateOffscreenPlain(device, sc.PresentParameters.BackBufferWidth, sc.PresentParameters.BackBufferHeight, sc.PresentParameters.BackBufferFormat, Pool.SystemMemory);
                }
                try
                {
                    device.GetRenderTargetData(backbuffer, surf);
                    if(!Directory.Exists(filename + "Output"))
                        Directory.CreateDirectory(filename + "Output");

                    if (!Directory.Exists(filename + "Output\\"+ Interface.exe))
                        Directory.CreateDirectory(filename + "Output\\" + Interface.exe);
                    
                    Surface.ToFile(surf, filename + "Output\\" + Interface.exe + "\\" + primCount + "x" + numVertices + ".bmp", ImageFileFormat.Bmp);
                    surf.Dispose();
                }
                catch (Exception e)
                {
                    Interface.ReportException(e);
                }
            }
        }

        public vertex GetVertex(UInt16 index, ref BinaryReader VertexData)
        {
            vertex v = new vertex();
            if (VertexData != null)
            {
                if (VertexData.BaseStream == null)
                {
                    Queue.Push("VertexData is NULL (GetVertex)");
                    return v;
                }
                VertexData.BaseStream.Seek(index * stride, SeekOrigin.Begin);
                //Queue.Push("Stride = ");
                //Queue.Push(stride.ToString());
                v.x = VertexData.ReadSingle();
                v.y = VertexData.ReadSingle();
                v.z = VertexData.ReadSingle();
            }
            return v;
        }

        public bool VertContains(vertex v)
        {
            foreach (vertex vert in this.vertices)
            {
                if (vert.x == v.x)
                    if (vert.y == v.y)
                        if (vert.z == v.z)
                            return true;
            }
            return false;
        }

        bool VertexEquals(vertex a, vertex b)
        {
            if (a.x == b.x)
                if (a.y == b.y)
                    if (a.z == b.z)
                        return true;
            return false;
        }

        int GetIndex(vertex v)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (VertexEquals(vertices[i], v))
                    return i;
            }
            return -1;
        }

        public void RipTriangleStrip(int baseVertexIndex, int startIndex, int primCount, ref IndexBuffer ib)
        {

            Queue.Push("Strip");
            if (ib == null)
            {
                Queue.Push("IB IS NULL");
                return;
            }

            if (vb == null)
            {
                Queue.Push("VB IS NULL");
                return;
            }

            BinaryReader IndexData = new BinaryReader(ib.Lock(0, 0, LockFlags.ReadOnly));
            BinaryReader VertexData = new BinaryReader(vb.Lock(0, 0, LockFlags.ReadOnly));
            vertex v;

            if (IndexData == null)
            {
                Queue.Push("Index Data is Null");
                return;
            }

            if (VertexData == null)
            {
                Queue.Push("VertexData is NULL");
                return;
            }

            IndexData.BaseStream.Seek(startIndex*sizeof(UInt16), SeekOrigin.Begin);
            int f = 0;
            string verts = "";
            for (int i = 0; i < primCount; i++)
            {

                UInt16 index = IndexData.ReadUInt16();
                UInt16 bVertex = (UInt16)baseVertexIndex;
                v = GetVertex((UInt16)(index+bVertex), ref VertexData);
                if (!VertContains(v))
                {
                    vertices.Add(v);
                    verts += "v " + v.x + " " + v.y + " " + v.z + "\r\n";
                }
                vmap[index + bVertex] = GetIndex(v);
            }

            System.IO.File.WriteAllText("C:\\Users\\emist\\models.txt", verts);

            IndexData.BaseStream.Seek(startIndex*sizeof(UInt16), SeekOrigin.Begin);
            string faces = "";
            
            //build initial face
            //faces += "f " + vmap[IndexData.ReadUInt16() + (UInt16)baseVertexIndex] + " " + vmap[IndexData.ReadUInt16() + (UInt16)baseVertexIndex] +
            //" " + vmap[IndexData.ReadUInt16() + (UInt16)baseVertexIndex] + "\r\n";
            
            //IndexData.BaseStream.Seek(startIndex+1, SeekOrigin.Begin);

            for (int i = 0; i < primCount; i++)
            {
                IndexData.BaseStream.Seek((startIndex + i)*sizeof(UInt16), SeekOrigin.Begin);
                UInt16 bVertex = (UInt16)baseVertexIndex;
               
                UInt16 I = IndexData.ReadUInt16();
                UInt16 I1 = IndexData.ReadUInt16();
                UInt16 I2 = IndexData.ReadUInt16();
                faces+="f " + vmap[I+bVertex] + " " + vmap[I1 + bVertex] + " " 
                            + vmap[I2 + bVertex] + "\r\n";
                f++;
            }
            System.IO.File.AppendAllText("C:\\Users\\emist\\models.txt", faces);
            vb.Unlock();
            ib.Unlock();
            vertices.Clear();
            vertices.Add(new vertex());
            vmap.Clear();
        }

        public void RipTriangleList(int baseVertexIndex, int startIndex, int primCount, ref IndexBuffer ib)
        {
            Queue.Push("List");
            if (ib == null)
            {
                Queue.Push("IB IS NULL");
                return;
            }

            if (vb == null)
            {
                Queue.Push("VB IS NULL");
                return;
            }

            BinaryReader IndexData = new BinaryReader(ib.Lock(0,0, LockFlags.ReadOnly));
            BinaryReader VertexData = new BinaryReader(vb.Lock(0, 0, LockFlags.ReadOnly));
            vertex v;

            if (IndexData == null)
            {
                Queue.Push("Index Data is Null");
                return;
            }

            if (VertexData == null)
            {
                Queue.Push("VertexData is NULL");
                return;
            }
            
            IndexData.BaseStream.Seek(startIndex * sizeof(UInt16), SeekOrigin.Begin);
            int f = 0;
            string verts = "";
            for (int i = 0; i < primCount * 3; i++)
            {
                
                UInt16 index = IndexData.ReadUInt16();
                UInt16 bVertex = (UInt16)baseVertexIndex;
                v = GetVertex(index, ref VertexData);
                if (!VertContains(v))
                {
                    vertices.Add(v);
                    verts += "v " + v.x + " " + v.y + " " + v.z + "\r\n";
                }
                vmap[index + bVertex] = GetIndex(v);   
            }

            System.IO.File.WriteAllText("C:\\Users\\emist\\models.txt", verts);

            IndexData.BaseStream.Seek(startIndex*sizeof(UInt16), SeekOrigin.Begin);
            string faces = "";
            for(int i = 0; i < primCount*3; i++)
            {
                UInt16 index = IndexData.ReadUInt16();
                UInt16 bVertex = (UInt16)baseVertexIndex;
                if(f % 3 == 0)
                {
                    faces += "\r\nf ";
                }

                faces+=vmap[index+bVertex] + " ";
                f++;
            }
            System.IO.File.AppendAllText("C:\\Users\\emist\\models.txt", faces);
            vb.Unlock();
            ib.Unlock();
            vertices.Clear();
            vertices.Add(new vertex());
            vmap.Clear();            
        }

        public void RipModel(SlimDX.Direct3D9.Device device, SlimDX.Direct3D9.PrimitiveType primitiveType,
                                        int baseVertexIndex, int startIndex, int primCount)
        {
            IndexBuffer ib = device.Indices;
            if (primitiveType == PrimitiveType.TriangleList)
                RipTriangleList(baseVertexIndex, startIndex, primCount, ref ib);
            if (primitiveType == PrimitiveType.TriangleStrip)
                RipTriangleStrip(baseVertexIndex, startIndex, primCount, ref ib);
        }

        int DrawIndexedPrimitivesHook(IntPtr devicePtr, SlimDX.Direct3D9.PrimitiveType primitiveType,
                                        int baseVertexIndex, int minimumVertexIndex,
                                        int numVertices, int startIndex, int primCount)
        {
            using (SlimDX.Direct3D9.Device device = SlimDX.Direct3D9.Device.FromPointer(devicePtr))
            {
                Primitive prim = new Primitive(primCount, numVertices);
                int hRet = 0;
                try
                {
                    //if new primitive being rendered, add it to our list
                    if (!prims.Contains(prim))
                    {
                        prims.Add(prim);
                    }

                    Primitive selectedPrim = prims.GetSelectedPrimitive();
                   
                    if (selectedPrim != null)
                    {
                        if (selectedPrim.Equals(prim))
                        {
                       
                            if (RedTexture == null)
                                RedTexture = SlimDX.Direct3D9.Texture.FromMemory(device, red);

                            if (Interface.chamed == true)
                            {
                                selectedPrim.Chamed = true;
                                Interface.Togglecham();
                            }
                            
                            device.SetRenderState(SlimDX.Direct3D9.RenderState.FillMode, SlimDX.Direct3D9.FillMode.Solid);
                            device.SetTexture(0, RedTexture);


                            if (selectedPrim.Chamed)
                            {
                                //device.Clear(ClearFlags.ZBuffer, Color.Red, 1.0f, 0);
                                device.SetRenderState(SlimDX.Direct3D9.RenderState.ZEnable, false);
                            }

                            if (Interface.rip)
                            {
                                RipModel(device, primitiveType, baseVertexIndex, startIndex, primCount);
                                Interface.ToggleRip();
                            }

                            hRet = device.DrawIndexedPrimitives(primitiveType, baseVertexIndex, minimumVertexIndex,
                                                             numVertices, startIndex, primCount).Code;
                            device.SetRenderState(SlimDX.Direct3D9.RenderState.ZEnable, true);
                        }
                    }
                    //if not to display, don't render
                    if(prims.IndexOf(prim) != -1)
                        if (prims[prims.IndexOf(prim)].Displayed == false)
                            return 0;
                }
                catch (Exception e)
                {
                    Interface.ReportException(e);
                    return hRet;
                }

                if (hRet == 0)
                {
                    if (prims[prims.IndexOf(prim)].Chamed == false)
                    {
                        hRet = device.DrawIndexedPrimitives(primitiveType, baseVertexIndex, minimumVertexIndex,
                                                            numVertices, startIndex, primCount).Code;
                    }
                    else
                    {
                        PixelShader previous = device.PixelShader;
                        if(chamPixelShader == null)
                            chamPixelShader = new PixelShader(device, ShaderBytecode.Compile("float4 PShader(float4 position : SV_POSITION) : SV_Target\n" +
                                                                    "{\nreturn float4(1.0f, 1.0f, 0.0f, 1.0f);\n}", "PShader", "ps_3_0", ShaderFlags.None));
                        
                        device.PixelShader = chamPixelShader;
                        
                        device.SetRenderState(SlimDX.Direct3D9.RenderState.ZEnable, false);
                        hRet = device.DrawIndexedPrimitives(primitiveType, baseVertexIndex, minimumVertexIndex,
                                                            numVertices, startIndex, primCount).Code;
                        device.SetRenderState(RenderState.ZEnable, true);
                        device.PixelShader = previous;
                    }
                }
                return hRet;
            }
        }

        int SetStreamSourceHook(IntPtr devicePtr, int stream, IntPtr vBuffer,
                                int offsetInBytes, int Stride)
        {
            using (SlimDX.Direct3D9.Device device = SlimDX.Direct3D9.Device.FromPointer(devicePtr))
            {
                try
                {
                    if (stream == 0)
                    {
                        stride = Stride;
                        vb = VertexBuffer.FromPointer(vBuffer);
                    }
                }
                catch (Exception e)
                {
                    Interface.ReportException(e);
                }
                return device.SetStreamSource(stream, VertexBuffer.FromPointer(vBuffer), offsetInBytes, Stride).Code;
            }
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        static byte[] ReadFullStream(System.IO.Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }


        void Cleanup()
        {


            if (RedTexture != null)
            {
                RedTexture.Dispose();
                RedTexture = null;
            }
            /*
            if (OrangeTexture != null)
            {
                OrangeTexture.Dispose();
                OrangeTexture = null;
            }

            if (MenuFont != null)
            {
                MenuFont.Dispose();
                MenuFont = null;
            }

            if (MenuLine != null)
            {
                MenuLine.Dispose();
                MenuLine.Dispose();
            }
            */
        }

        /// <summary>
        /// Reset the _renderTarget so that we are sure it will have the correct presentation parameters (required to support working across changes to windowed/fullscreen or resolution changes)
        /// </summary>
        /// <param name="devicePtr"></param>
        /// <param name="presentParameters"></param>
        /// <returns></returns>
        int ResetHook(IntPtr devicePtr, ref SlimDX.Direct3D9.PresentParameters presentParameters)
        {
            using (Device device = Device.FromPointer(devicePtr))
            {
                Cleanup();
                return device.Reset(presentParameters).Code;
            }
        }

        int EndSceneHook(IntPtr devicePtr)
        {
            if (Interface.clearprims)
            {
                prims.Clear();
                Interface.ToggleClearPrims();
            }

            if (Interface.clearChams)
            {
                foreach (Primitive prim in prims)
                {
                    prim.Chamed = false;
                }
                Interface.ToggleClearChams();
            }

            Interface.UpdateTotalPrims(prims.Count);

            using (Device device = SlimDX.Direct3D9.Device.FromPointer(devicePtr))
            {
                /*
                    try
                    {

                        byte[] red = 
                    {
                        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00
                    };

                        byte[] orange = 
                    {
                        0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA5, 0xFF, 0x00
                    };

                        //Main This = (Main)HookRuntimeInfo.Callback;

                        SlimDX.Color4 MenuLineColor;
                        SlimDX.Color4 FontColor;
                        SlimDX.Direct3D9.Texture RedTexture = null;
                        SlimDX.Direct3D9.Texture OrangeTexture = null;
                        SlimDX.Direct3D9.Font MenuFont = null;
                        SlimDX.Direct3D9.Line MenuLine = null;


                        MenuLineColor = new SlimDX.Color4(Color.Red);
                        FontColor = new SlimDX.Color4(Color.Purple);


                        RedTexture = SlimDX.Direct3D9.Texture.FromMemory(device, red);
                        OrangeTexture = SlimDX.Direct3D9.Texture.FromMemory(device, orange);

                        
                        MenuFont = new SlimDX.Direct3D9.Font(device, 15, 0, SlimDX.Direct3D9.FontWeight.Bold, 1, false,
                                                                 SlimDX.Direct3D9.CharacterSet.Default, SlimDX.Direct3D9.Precision.Default,
                                                                 SlimDX.Direct3D9.FontQuality.Antialiased,
                                                                 SlimDX.Direct3D9.PitchAndFamily.DontCare,
                                                                 "Verdana");

                        MenuLine = new SlimDX.Direct3D9.Line(device);


                        if (MenuFont != null && MenuLine != null && FontColor != null)
                        {
                            Rectangle fontPos = new Rectangle();
                            fontPos.X = 10;
                            fontPos.Y = 10;
                            fontPos.Width = 10 + 120;
                            fontPos.Height = 15 + 16;

                            string text = "Helllo world";

                            if (MenuFont != null)
                            {
                                MenuFont.DrawString(null, text, fontPos, SlimDX.Direct3D9.DrawTextFormat.NoClip, FontColor);
                            }
                            //ModelRecLoggerMenu(FontColor, MenuFont, MenuLine, MenuLineColor);
                            
                          
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Interface.ReportException(e);
                        return device.EndScene().Code;
                    }
                    */
                
                if (Interface.display == false)
                {
                    int selected = prims.IndexOf(prims.GetSelectedPrimitive());
                    if (selected != -1)
                    {
                        if (prims[selected].Displayed == true)
                            prims[selected].Displayed = false;
                        else
                            prims[selected].Displayed = true;

                        Interface.ToggleDisplayPrim();
                    }
                }

                try
                {
                    if (Interface.keys.Count > 0)
                    {
                        if (prims.Count > 0)
                        {
                            string primcount = "", vertcount = "";
                            int selected = prims.IndexOf(prims.GetSelectedPrimitive());
                            if (Interface.keys[0].Equals(Keys.Up))
                            {
                                if (selected != -1)
                                {
                                    prims[selected].Selected = false;
                                    if (selected != 0)
                                    {
                                        prims[selected - 1].Selected = true;
                                        primcount = Convert.ToString(prims[selected - 1].PrimCount);
                                        vertcount = Convert.ToString(prims[selected - 1].NumVertices);
                                    }
                                    else
                                    {
                                        primcount = Convert.ToString(prims[prims.Count - 1].PrimCount);
                                        vertcount = Convert.ToString(prims[prims.Count - 1].NumVertices);
                                        prims[prims.Count - 1].Selected = true;
                                    }
                                }
                                else
                                {
                                    primcount = Convert.ToString(prims[0].PrimCount);
                                    vertcount = Convert.ToString(prims[0].NumVertices);
                                    prims[0].Selected = true;
                                }
                            }
                            else if (Interface.keys[0].Equals(Keys.Down))
                            {
                                if (selected != -1)
                                {
                                    prims[selected].Selected = false;
                                    if (selected != prims.Count - 1)
                                    {
                                        primcount = Convert.ToString(prims[selected + 1].PrimCount);
                                        vertcount = Convert.ToString(prims[selected + 1].NumVertices);
                                        prims[selected + 1].Selected = true;
                                    }
                                    else
                                    {
                                        primcount = Convert.ToString(prims[0].PrimCount);
                                        vertcount = Convert.ToString(prims[0].NumVertices);
                                        prims[0].Selected = true;
                                    }
                                }
                                else
                                {
                                    primcount = Convert.ToString(prims[0].PrimCount);
                                    vertcount = Convert.ToString(prims[0].NumVertices);
                                    prims[0].Selected = true;
                                }

                            }
                            Interface.UpdatePrimandVertCount(primcount, vertcount);
                            Interface.ClearQueue();
                        }
                    }
                    if (Interface.saveprim)
                    {
                        Primitive prim = prims.GetSelectedPrimitive();
                        Screenshot(device, Interface.OutPutDir, prim.PrimCount, prim.NumVertices);
                        Interface.clearsaveprim();
                    }
                }
                catch (Exception e)
                {
                    Interface.ReportException(e);
                }
                return device.EndScene().Code;
            }
        }

        void DrawRectangle(float x, float y, float w, int h, Line MenuLine, SlimDX.Direct3D9.Font MenuFont, Color4 MenuLineColor)
        {
            try
            {
                SlimDX.Vector2[] vLine1 = new SlimDX.Vector2[2];
                SlimDX.Vector2[] vLine2 = new SlimDX.Vector2[2];
                SlimDX.Vector2[] vLine3 = new SlimDX.Vector2[2];
                SlimDX.Vector2[] vLine4 = new SlimDX.Vector2[2];

                vLine1[0] = new Vector2();
                vLine1[1] = new Vector2();
                vLine2[0] = new Vector2();
                vLine2[1] = new Vector2();
                vLine3[0] = new Vector2();
                vLine3[1] = new Vector2();
                vLine4[0] = new Vector2();
                vLine4[1] = new Vector2();

                vLine1[0].X = x;
                vLine1[0].Y = y;
                vLine1[1].X = x;
                vLine1[1].Y = y + h;

                vLine2[0].X = x + w;
                vLine2[0].Y = y;
                vLine2[1].X = x + w;
                vLine2[1].Y = y + h;

                vLine3[0].X = x;
                vLine3[0].Y = y;
                vLine3[1].X = x + w;
                vLine3[1].Y = y;

                vLine4[0].X = x;
                vLine4[0].Y = y + h;
                vLine4[1].X = x + w;
                vLine4[1].Y = y + h;

                if (MenuLine != null)
                {
                    MenuLine.Width = 2;
                    MenuLine.Antialias = false;
                    MenuLine.GLLines = false;
                    MenuLine.Begin();

                    MenuLine.Draw(vLine1, MenuLineColor);
                    MenuLine.Draw(vLine2, MenuLineColor);
                    MenuLine.Draw(vLine3, MenuLineColor);
                    MenuLine.Draw(vLine4, MenuLineColor);

                    MenuLine.End();
                }
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);
                return;
            }

        }

        public void DrawString(int x, int y, Color4 color, SlimDX.Direct3D9.DrawTextFormat format, string text, SlimDX.Direct3D9.Font MenuFont)
        {
            Rectangle fontPos = new Rectangle();
            fontPos.X = x;
            fontPos.Y = y;
            fontPos.Width = x + 120;
            fontPos.Height = y + 16;

            if (MenuFont != null)
            {
                MenuFont.DrawString(null, text, fontPos, format, color);

            }

        }


        void ModelRecLoggerMenu(Color4 FontColor, SlimDX.Direct3D9.Font MenuFont, Line MenuLine, Color4 MenuLineColor)
        {
            try
            {
                DrawString(10, 10, FontColor, SlimDX.Direct3D9.DrawTextFormat.NoClip, "( Stride Logger )........Last Edited By : PheonX", MenuFont);
            }
            catch (Exception e)
            {
                Interface.ReportException(e);
            }

            for (int i = 0; i < 5; i++)
            {
                if (i == menuIndex)
                {
                    try
                    {
                        DrawRectangle(8, 23 + (i * 15), 140, 18, MenuLine, MenuFont, MenuLineColor);
                    }
                    catch (Exception e)
                    {
                        Interface.ReportException(e);
                    }
                }

                DrawString(10, 25 + (i * 15), model[i].isLogging == true ? new Color4(Color.Green) : new Color4(Color.Red), SlimDX.Direct3D9.DrawTextFormat.NoClip, model[i].type, MenuFont);


                /*
                if (i != LOGVALUES)
                {
                    DrawString(100, 25 + (i * 15), model[i].isLogging == true ? new Color4(Color.Green) : new Color4(Color.Red), SlimDX.Direct3D9.DrawTextFormat.NoClip, model[i].value.ToString(), MenuFont);
                }
                 */

            }


            if (IsKeyPushedDown(Keys.Up)) menuIndex--;
            if (IsKeyPushedDown(Keys.Down)) menuIndex++;
            if (IsKeyPushedDown(Keys.Left)) model[menuIndex].value -= incrementBy;
            if (IsKeyPushedDown(Keys.Right)) model[menuIndex].value += incrementBy;

            if (IsKeyPushedDown(Keys.Delete)) model[menuIndex].isLogging = !model[menuIndex].isLogging;

            if (IsKeyPushedDown(Keys.Insert)) model[menuIndex].value = 0;
            if (IsKeyPushedDown(Keys.Next))
            {
                incrementBy *= 10;
                if (incrementBy > 1000)
                {
                    incrementBy = 1;
                }
            }

            if (IsKeyPushedDown(Keys.End))
            {
                for (int i = 0; i < 4; i++)
                {
                    model[i].isLogging = false;
                }
            }


            if (model[LOGVALUES].isLogging == true)
            {
                model[LOGVALUES].isLogging = false;
                //g_uiTimer = GetTickCount() + 2000;

                /*
                Log("Stride : %i | NumVerts : %i | PrimCount : %i | StartIndex : %i",
                model[STRIDE].isLogging == true ? model[STRIDE].value : -1,
                model[NUMVERTS].isLogging == true ? model[NUMVERTS].value : -1,
                model[PRIMCOUNT].isLogging == true ? model[PRIMCOUNT].value : -1,
                model[STARTINDEX].isLogging == true ? model[STARTINDEX].value : -1);
            
                 */
            }



            if (menuIndex > 4) menuIndex = 0;
            if (menuIndex < 0) menuIndex = 4;

            if (model[menuIndex].value < 0) model[menuIndex].value = 0;

        }


    }


}
