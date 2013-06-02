using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DTextureLogger
{
    public class PrimitiveList : List<Primitive>
    {
        public new bool Contains(Primitive prim)
        {
            foreach (Primitive primitive in this)
            {
                if (primitive.PrimCount == prim.PrimCount && primitive.NumVertices == prim.NumVertices)
                    return true;
            }
            return false;
        }

        public Primitive GetSelectedPrimitive()
        {
            foreach (Primitive prim in this)
            {
                if (prim.Selected)
                    return prim;
            }
            return null;
        }

    }
}
