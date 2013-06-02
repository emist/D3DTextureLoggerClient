using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DHookingLibrary
{
    public class D3DFuncLookup
    {
        private VtableLookup.LookUp lookUpObject = new VtableLookup.LookUp();
        private string exe;
        private string module;

        public struct D3D8Functions
        {
            public const string EndScene = "CD3DBase::EndScene";
            public const string BeginScene = "CD3DBase::BeginScene";
            public const string DrawIndexedPrimitive = "CD3DBase::DrawIndexedPrimitive";
            public const string SetStreamSource = "CD3DBase::SetStreamSource";
        }

        public struct D3D9Functions
        {
            public const string EndScene = "CD3DBase::EndScene";
            public const string BeginScene = "CD3DBase::BeginScene";
            public const string DrawIndexedPrimitive = "CD3DBase::DrawIndexedPrimitive";
            public const string SetStreamSource = "CD3DBase::SetStreamSource";
            public const string Reset = "CBaseDevice::Reset";
            public const string SetLight = "CD3DBase::SetLight";
        }

        public struct D3D9XFunctions
        {
            public const string DrawText = "D3DXCore::CFont::DrawTextA";
        }


        public D3DFuncLookup(string exe, string module)
        {
            this.exe = exe;
            this.module = module;
            lookUpObject.Init(exe, module);
        }

        public uint GetD3DFunction(string funcName)
        {
            return lookUpObject.GetD3DFunction(funcName);
        }

        public string Exe
        {
            get
            {
                return exe;
            }
            set
            {
                exe = value;
            }
        }

        public string Module
        {
            get
            {
                return module;
            }
            set
            {
                module = value;
            }
        }

        

        public VtableLookup.LookUp LookUpObject
        {
            get
            {
                return lookUpObject;
            }
        }
    }
}
