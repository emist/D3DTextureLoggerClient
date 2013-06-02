// This is the main DLL file.

#include "stdafx.h"

#include "VtableLookup.h"


using namespace System;
using namespace std;
using namespace msclr::interop;

namespace VtableLookup 
{

		// TODO: Add your methods for this class here.
	int LookUp::iGetDebugPrivilege ()
	{
		HANDLE hToken;
		TOKEN_PRIVILEGES CurrentTPriv;
		LUID luidVal;

		if ( OpenProcessToken(processHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken) == FALSE )
			return 0;

		if ( LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luidVal) == FALSE )
		{
			CloseHandle( hToken );
			return 0;
		}

			CurrentTPriv.PrivilegeCount = 1;
			CurrentTPriv.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
			CurrentTPriv.Privileges[0].Luid = luidVal;
			int iRet = AdjustTokenPrivileges(hToken, FALSE, &CurrentTPriv, sizeof( TOKEN_PRIVILEGES ), NULL, NULL);
			CloseHandle(hToken);
			return iRet;
		}

		void LookUp::Init(String^ pName, String^ mod)
		{
			module = mod;
			if(!iGetDebugPrivilege())
			{
				cout << "Failed to set debug privileges" << endl;
			}

			processID = FindProcessId(pName);
			if(processID == 0)
				cout << "PID NULL" << endl;

			cout << "PROCESS ID " << processID << endl;

			processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | 
								PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_CREATE_THREAD, 
								FALSE, processID);

			cout << "Process Handle " << processHandle << endl;

			if(processHandle == NULL)
				cout << "Handle is null" << endl;
		}

		DWORD LookUp::GetD3DFunction(String^ csName)
		{
			wstring name;
			wstring lib;
			DWORD address = 0;
			stdString(csName, name);
			stdString(module, lib);

			std::wstring searchPath = L"SRV*C:\\ProgramData\\Symbols*http://msdl.microsoft.com/download/symbols";
			try
			{
				SymbolHandler sh(processHandle, searchPath);
				sh.LoadSymbolsForModule(lib, processHandle);
				address = sh.GetAddressFromSymbol(name);
				std::ostringstream str;
				wcout << name << " address: 0x" << std::hex << address;
				cout << str.str().c_str() << endl;
			}
			catch(std::exception const& e)
			{
				cout << boost::diagnostic_information(e).c_str() << endl;
			}
				
			return address;
		}

		DWORD LookUp::GetModuleBase(String^ cslpModuleName, DWORD dwProcessId)
		{
			wstring lpModuleName;
			stdString(cslpModuleName, lpModuleName);
			MODULEENTRY32 lpModuleEntry = {0};
			HANDLE hSnapShot = CreateToolhelp32Snapshot( TH32CS_SNAPMODULE, dwProcessId );

			if(!hSnapShot)
				return NULL;

			lpModuleEntry.dwSize = sizeof(lpModuleEntry);

			BOOL bModule = Module32First( hSnapShot, &lpModuleEntry );

			
			stringstream ss;
			ss << lpModuleName.c_str();
			string moduleName(ss.str());
			ss.clear();

			while(bModule)
			{
				ss << lpModuleEntry.szModule;	
				if(!strcmp(ss.str().c_str(), moduleName.c_str()))
				{
					CloseHandle( hSnapShot );
					return (DWORD)lpModuleEntry.modBaseAddr;
				}

				ss.clear();
				bModule = Module32Next( hSnapShot, &lpModuleEntry );
			}

			CloseHandle( hSnapShot );

			return NULL;
		}

		DWORD LookUp::FindProcessId(System::String^ csprocessName)
		{
			wstring processName;
			stdString(csprocessName, processName);
			PROCESSENTRY32 processInfo;
			processInfo.dwSize = sizeof(processInfo);
			
			HANDLE processesSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
			if ( processesSnapshot == INVALID_HANDLE_VALUE )
				return 0;

			Process32First(processesSnapshot, &processInfo);
			if ( !processName.compare(processInfo.szExeFile) )
			{
				CloseHandle(processesSnapshot);
				processID = processInfo.th32ProcessID;
				return processID;			
			}

			while ( Process32Next(processesSnapshot, &processInfo) )
			{
				if ( !processName.compare(processInfo.szExeFile) )
				{
					CloseHandle(processesSnapshot);
					processID = processInfo.th32ProcessID;
					return processID;
				}
			}

			CloseHandle(processesSnapshot);
			return 0;
		}

		void LookUp::stdString(String^ csString, string & str)
		{
			str.append(context->marshal_as<const char *>( csString ));
		}
		
		void LookUp::stdString(String^ csString, wstring & str)
		{
			wstringstream is;
			is << context->marshal_as<const char *>( csString );
			str.append(is.str());
		}
}

