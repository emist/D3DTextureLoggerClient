using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace D3DTextureLogger
{
    class StoredDIP
    {
        public IntPtr devicePtr;
        public SlimDX.Direct3D9.PrimitiveType primitiveType;
        public int baseVertexIndex, minimumVertexIndex, numVertices, startIndex, primCount;

        public StoredDIP(IntPtr devicePtr, SlimDX.Direct3D9.PrimitiveType primitiveType,
                                        int baseVertexIndex, int minimumVertexIndex,
                                        int numVertices, int startIndex, int primCount)
        {
            this.devicePtr = devicePtr;
            this.primitiveType = primitiveType;
            this.baseVertexIndex = baseVertexIndex;
            this.minimumVertexIndex = minimumVertexIndex;
            this.numVertices = numVertices;
            this.startIndex = startIndex;
            this.primCount = primCount;
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
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                startIndex = value;
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

        public int MinimumVertexIndex
        {
            get
            {
                return minimumVertexIndex;
            }
            set
            {
                minimumVertexIndex = value;
            }
        }

        public int BaseVertexIndex
        {
            get
            {
                return baseVertexIndex;
            }
            set
            {
                baseVertexIndex = value;
            }

        }

        public SlimDX.Direct3D9.PrimitiveType Primitive
        {
            get
            {
                return primitiveType;
            }
            set
            {
                primitiveType = value;
            }
        }

        public IntPtr DevicePtr
        {
            get
            {
                return devicePtr;
            }
            set
            {
                devicePtr = value;
            }
        }
    
    }
    
}
