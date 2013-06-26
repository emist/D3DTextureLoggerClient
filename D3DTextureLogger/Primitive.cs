using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;

namespace D3DTextureLogger
{
    public class Primitive
    {
        int primCount, numVertices;
        bool selected = false;
        bool displayed = true;
        bool chamed = false;
        int[] indexBuffer;
        float[] vertexBuffer;

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[30 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public int DumpToObjFile(string filename, ref MemoryStream ibuffer, ref MemoryStream vbuffer, int startIndex, int minIndex, int baseVertexIndex)
        {
            filename = @"C:\Users\emist\Desktop\t.obj";
            byte[] i = new byte[32*3*primCount];
            byte[] v;

            ibuffer.Read(i, startIndex, 32*3*primCount);
            v = ReadFully(vbuffer);
            float[] vertexBuff = new float[v.Length / 4];
            Buffer.BlockCopy(v, 0, vertexBuff, 0, v.Length);
            Int32[] indexBuff = new Int32[i.Length / 4];
            Buffer.BlockCopy(i, 0, indexBuff, 0, i.Length);

            foreach (float f in vertexBuff)
            {
                File.WriteAllText("PrimTest.txt", Convert.ToString(f) + "f,");
                File.WriteAllText("PrimTest.txt", " ");
            }

            return 0;
        }

        public Primitive(int primCount, int numVertices)
        {
            this.primCount = primCount;
            this.numVertices = numVertices;
        }

        public Primitive(int primCount, int numVertices, bool selected)
        {
            this.primCount = primCount;
            this.numVertices = numVertices;
            this.selected = selected;
        }

        public bool Equals(Primitive prim)
        {
            if (prim == null)
                return false;
            if (prim.NumVertices == this.numVertices && prim.primCount == this.primCount)
                return true;
            return false;
        }

        public int PrimCount
        {
            get
            {
                return primCount;
            }
            set
            {
                primCount = value;
            }
        }

        public int NumVertices
        {
            get
            {
                return numVertices;
            }
            set
            {
                numVertices = value;
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        public bool Chamed
        {
            get
            {
                return chamed;
            }
            set
            {
                chamed = value;
            }
        }

        public bool Displayed
        {
            get
            {
                return displayed;
            }
            set
            {
                displayed = value;
            }
        }


    }
}
