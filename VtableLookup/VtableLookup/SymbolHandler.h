#pragma once

#include "Stdafx.h"
#include <Windows.h>
// Set DbgHelp to use unicode strings
#define DBGHELP_TRANSLATE_TCHAR
#include <DbgHelp.h>
#include <string>
#include <boost/exception/all.hpp>

namespace VtableLookup
{
	// Exception handling code stolen from HadesMem
	typedef boost::error_info<struct TagErrorString, std::string> ErrorString;
	typedef boost::error_info<struct TagErrorCode, DWORD> ErrorCode;

	class SHError : public virtual std::exception, public virtual boost::exception
	{
	};

	class SymbolHandler
	{
	public:
		SymbolHandler();
		SymbolHandler(HANDLE process);
		SymbolHandler(HANDLE process, const std::wstring& searchPath);
		~SymbolHandler();

		void LoadSymbolsForModule(const std::wstring& moduleName, HANDLE h);
		DWORD GetAddressFromSymbol(const std::wstring& name, bool throwOnFailure = true);

	private:
		void Init(HANDLE process, const std::wstring& searchPath);
		void Cleanup();

		HANDLE _process;
	};
}