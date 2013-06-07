using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DTextureLogger
{
    public class Primitive
    {
        int primCount, numVertices;
        bool selected = false;
        bool displayed = true;
        bool chamed = false;

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
