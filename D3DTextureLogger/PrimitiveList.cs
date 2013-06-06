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
                if (primitive.Equals(prim))
                    return true;
            }
            return false;
        }
        
        public new int IndexOf(Primitive prim)
        {
            int i = 0;
            foreach(Primitive in_prim in this)
            {
                if (in_prim.Equals(prim))
                    return i;
                i++;
            }
            return -1;
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
