// D3DVtableLookup.h

#pragma once
#pragma comment(lib, "advapi32.lib")
using namespace System;
using namespace std;
using namespace msclr::interop;

namespace VtableLookup 
{
	public ref class LookUp
	{
		public: 
			void Init(String^ pName, String^ mod);
			DWORD GetD3DFunction(String^ csName);
			DWORD GetModuleBase(String^ cslpModuleName, DWORD dwProcessId);
			DWORD FindProcessId(System::String^ csprocessName);
			int iGetDebugPrivilege ();
		private:
			static marshal_context^ context = gcnew marshal_context();
			static HANDLE processHandle = NULL;
			static DWORD processID = NULL;
			static int version = 8;
			String^ module;

			void stdString(String^ csString, string & str);
			void stdString(String^ csString, wstring & str);

	};
}
