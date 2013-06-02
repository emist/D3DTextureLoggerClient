//
// Credits: http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/310188-sample-code-another-way-of-getting-endscene-address.html
//
//

#include "Stdafx.h"
#pragma comment(lib, "dbghelp")

using namespace std;

namespace VtableLookup
{
	SymbolHandler::SymbolHandler()
	{
		_process = NULL;
		Init(GetCurrentProcess(), NULL);
	}

	SymbolHandler::SymbolHandler(HANDLE process)
	{
		_process = NULL;
		Init(process, NULL);
	}

	SymbolHandler::SymbolHandler(HANDLE process, const std::wstring& searchPath)
	{
		_process = NULL;

		Init(process, searchPath);
	}

	SymbolHandler::~SymbolHandler()
	{
		Cleanup();
	}

	void SymbolHandler::Init(HANDLE process, const std::wstring& searchPath)
	{
		if(_process)
			Cleanup();

		// SYMOPT_DEBUG is not really needed, but debug output is always good
		// if something goes wrong
		SymSetOptions(SYMOPT_DEBUG | SYMOPT_DEFERRED_LOADS | SYMOPT_UNDNAME);
		if(!SymInitialize(process, (PCWSTR)searchPath.c_str(), FALSE))
			BOOST_THROW_EXCEPTION(SHError() << ErrorString("SymInitialize() failed") << ErrorCode(GetLastError()));
		_process = process;
	}

	void SymbolHandler::Cleanup()
	{
		if(_process)
		{
			if(!SymCleanup(_process))
				BOOST_THROW_EXCEPTION(SHError() << ErrorString("SymCleanup() failed") << ErrorCode(GetLastError()));
			_process = NULL;
		}
	}

	void SymbolHandler::LoadSymbolsForModule(const std::wstring& moduleName, HANDLE processHandle)
	{
		RemoteOps ops;
		
		string name( moduleName.begin(), moduleName.end() );

		if(processHandle == NULL)
		{
			cout << "ProcessHandle is Null" << endl;
			return;
		}

		HANDLE moduleHandle = ops.GetRemoteModuleHandle(processHandle, name.c_str());
		if(moduleHandle == NULL)
			cout << "No MODULE HANDLE " << endl;

		if(!SymLoadModuleEx(_process, NULL, moduleName.c_str(), NULL, (DWORD)moduleHandle, 0, NULL, 0))
			BOOST_THROW_EXCEPTION(SHError() << ErrorString("SymLoadModuleEx() failed") << ErrorCode(GetLastError()));
		
	}

	DWORD SymbolHandler::GetAddressFromSymbol(const std::wstring& name, bool throwOnFailure)
	{
		std::vector<char> buffer;
		buffer.resize(sizeof(SYMBOL_INFO) + name.length() * sizeof(wchar_t) + 1);
		PSYMBOL_INFO pSymbol = (PSYMBOL_INFO)buffer.data();
		pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
		pSymbol->MaxNameLen = name.length() * sizeof(wchar_t) + 1;
		LPVOID ret = NULL;
		if(!SymFromName(_process, name.c_str(), pSymbol))
		{
			// If you know your symbol name is valid then this most likely happens
			// because symsrv.dll isn't loaded.
			if(throwOnFailure)
				BOOST_THROW_EXCEPTION(SHError() << ErrorString("SymFromName() failed") << ErrorCode(GetLastError()));
			else
				return NULL;
		}
		return pSymbol->Address;
	}
}