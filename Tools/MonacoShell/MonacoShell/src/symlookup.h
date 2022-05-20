#pragma once

#include <winapifamily.h>
#include <psapi.h>
#include <Shlwapi.h>
#include <vector>
#include <string>
#include <sstream>

#pragma warning( push )
#pragma warning( disable:4091 )
#define _NO_CVCONST_H
    #include <dbghelp.h>
#ifdef _NO_CVCONST_H

//
// CV_HREG_e, originally from CVCONST.H in DIA SDK 
//

typedef enum CV_HREG_e {
    //
    // Only a limited number of registers included here 
    //

    CV_REG_EAX = 17,
    CV_REG_ECX = 18,
    CV_REG_EDX = 19,
    CV_REG_EBX = 20,
    CV_REG_ESP = 21,
    CV_REG_EBP = 22,
    CV_REG_ESI = 23,
    CV_REG_EDI = 24
} CV_HREG_e;

enum SymTagEnum {
    SymTagNull,
    SymTagExe,
    SymTagCompiland,
    SymTagCompilandDetails,
    SymTagCompilandEnv,
    SymTagFunction,
    SymTagBlock,
    SymTagData,
    SymTagAnnotation,
    SymTagLabel,
    SymTagPublicSymbol,
    SymTagUDT,
    SymTagEnum,
    SymTagFunctionType,
    SymTagPointerType,
    SymTagArrayType,
    SymTagBaseType,
    SymTagTypedef,
    SymTagBaseClass,
    SymTagFriend,
    SymTagFunctionArgType,
    SymTagFuncDebugStart,
    SymTagFuncDebugEnd,
    SymTagUsingNamespace,
    SymTagVTableShape,
    SymTagVTable,
    SymTagCustom,
    SymTagThunk,
    SymTagCustomType,
    SymTagManagedType,
    SymTagDimension,
    SymTagCallSite,
    SymTagInlineSite,
    SymTagBaseInterface,
    SymTagVectorType,
    SymTagMatrixType,
    SymTagHLSLType
};

enum BasicType
{
    btNoType = 0,
    btVoid = 1,
    btChar = 2,
    btWChar = 3,
    btInt = 6,
    btUInt = 7,
    btFloat = 8,
    btBCD = 9,
    btBool = 10,
    btLong = 13,
    btULong = 14,
    btCurrency = 25,
    btDate = 26,
    btVariant = 27,
    btComplex = 28,
    btBit = 29,
    btBSTR = 30,
    btHresult = 31,
};

#endif // _NO_CVCONST_H
#pragma warning( pop ) 

#pragma comment(lib, "psapi")
#pragma comment(lib, "shlwapi")
#pragma comment(lib, "dbghelp")

namespace aristocrat
{
    class SymbolLookup
    {
    private:
        typedef BOOL(__stdcall *SymInitialize_t)(IN HANDLE hProcess, IN PSTR UserSearchPath, IN BOOL fInvadeProcess);
        typedef BOOL(__stdcall *SymCleanup_t)(IN HANDLE hProcess);
        typedef BOOL(__stdcall *SymGetSymFromAddr64_t)(IN HANDLE hProcess, IN DWORD64 dwAddr, OUT PDWORD64 pdwDisplacement, OUT PIMAGEHLP_SYMBOL64 Symbol);
        typedef DWORD(__stdcall *UnDecorateSymbolName_t)(PCSTR DecoratedName, PSTR UnDecoratedName, DWORD UndecoratedLength, DWORD Flags);
        typedef DWORD64(__stdcall *SymLoadModule64_t)(IN HANDLE hProcess, IN HANDLE hFile, IN PSTR ImageName, IN PSTR ModuleName, IN DWORD64 BaseOfDll, IN DWORD SizeOfDll);
        typedef BOOL(__stdcall *SymUnloadModule64_t)(IN HANDLE hProcess, IN DWORD64 BaseOfDll);
        typedef DWORD (__stdcall * SymSetOptions_t)(IN DWORD   SymOptions);
        typedef BOOL (__stdcall * SymGetLineFromAddr64_t)(IN HANDLE hProcess, IN DWORD64 qwAddr, OUT PDWORD pdwDisplacement, OUT PIMAGEHLP_LINE64 Line64);
        typedef PVOID(__stdcall * SymFunctionTableAccess64_t)(IN HANDLE hProcess, IN DWORD64 AddrBase);
        typedef DWORD64(__stdcall * SymGetModuleBase64_t)(IN HANDLE hProcess, IN DWORD64 qwAddr);
        typedef BOOL(__stdcall * SymEnumSymbols_t)(IN HANDLE hProcess, IN ULONG64 BaseOfDll,_In_opt_ PCSTR Mask,_In_ PSYM_ENUMERATESYMBOLS_CALLBACK EnumSymbolsCallback,_In_opt_ PVOID UserContext);
        typedef BOOL (__stdcall* SymSetContext_t)(IN HANDLE hProcess, IN PIMAGEHLP_STACK_FRAME StackFrame,_In_opt_ PIMAGEHLP_CONTEXT Context);
        typedef BOOL(__stdcall*  SymFromAddr_t)(_In_ HANDLE hProcess,_In_ DWORD64 Address,_Out_opt_ PDWORD64 Displacement,_Inout_ PSYMBOL_INFO Symbol);

        SymbolLookup(HANDLE hProcess, HMODULE hModule = 0)
        {
            _hDbgHelp = hModule;
            _hProcess = hProcess;
            pSymbol = (IMAGEHLP_SYMBOL64 *) ::malloc(sizeof(IMAGEHLP_SYMBOL64) + METHOD_MAX_NAMELEN);
        }

        static BOOL CALLBACK EnumThisSymbolsCallback(SYMBOL_INFO* pSymInfo, ULONG /*SymbolSize*/, PVOID UserContext)
        {
            SymbolLookup* pThis = static_cast<SymbolLookup*>(UserContext);

            return pThis->FindType(pSymInfo); // Found this, stop enumeration
        }

        BOOL FindType(SYMBOL_INFO* pSymInfo)
        {
            if (strcmp(pSymInfo->Name, "this") != 0)
                return TRUE;

            if (pSymInfo != 0)
            {
                WCHAR* pType_name = 0;
                DWORD dwType = 0;
                DWORD tag;

                dwType = pSymInfo->TypeIndex;
                ::SymGetTypeInfo(_hProcess, pSymInfo->ModBase, dwType, TI_GET_SYMTAG, &tag);

                while (tag == SymTagPointerType)
                {
                    DWORD subType = 0;

                    SymGetTypeInfo(_hProcess, pSymInfo->ModBase, dwType, TI_GET_TYPE, &subType);
                    dwType = subType;
                    tag = 0;
                    SymGetTypeInfo(_hProcess, pSymInfo->ModBase, subType, TI_GET_SYMTAG, &tag);
                }

                ::SymGetTypeInfo(_hProcess, pSymInfo->ModBase, dwType, TI_GET_SYMNAME, &pType_name);

                std::vector<char> ansi(wcslen(pType_name) + 1);
                WideCharToMultiByte(CP_ACP, 0, pType_name, (int)ansi.size(), &ansi[0], (int)ansi.size(), NULL, NULL);
                std::string TypeName = &ansi[0];
                LocalFree(pType_name);

                //
                // TODO: add symboldetail
                //

                if (TypeName == _currentType)
                {
                    SetThisPointer(*pSymInfo);

                    return FALSE;
                }
            }

            return TRUE;
        }

        void SetThisPointer(SYMBOL_INFO& SymInfo)
        {
            //
            // Offset
            //

            uintptr_t Offset = (uintptr_t)SymInfo.Address;
            uintptr_t addr = (uintptr_t)_currentFrame;
            uintptr_t offReal = 0;

            if (Offset >= 0)
            {
                addr+=Offset;
                offReal = Offset;
            }
            else
            {
                addr -= (UINT_MAX - Offset + 1);
                offReal = (UINT_MAX - Offset + 1);
            }

            //
            // Size
            //

            LPVOID local_stack  = (LPVOID)addr;
            LPVOID pArgument    = *(LPVOID*)local_stack;
            _currentThisPointer = pArgument;
        }
        
        ~SymbolLookup()
        {
            if (_hProcess != 0)
            {
                for (auto base =_module_bases.begin(); base != _module_bases.end(); ++base)
                {
                    SymUnloadModule64(_hProcess, *base);
                }

                _module_bases.clear();
                this->SymCleanup(_hProcess);
            }

            if (pSymbol != nullptr)
            {
                ::free(pSymbol);
            }

            if (_hDbgHelp != nullptr)
            {
                ::FreeLibrary(_hDbgHelp);
            }
        }

        bool Initialize()
        {
            SymCleanup = (SymCleanup_t)GetProcAddress(_hDbgHelp, "SymCleanup");
            SymGetSymFromAddr64 = (SymGetSymFromAddr64_t)GetProcAddress(_hDbgHelp, "SymGetSymFromAddr64");
            UnDecorateSymbolName = (UnDecorateSymbolName_t)GetProcAddress(_hDbgHelp, "UnDecorateSymbolName");
            SymLoadModule64 = (SymLoadModule64_t)GetProcAddress(_hDbgHelp, "SymLoadModule64");
            SymUnloadModule64 = (SymUnloadModule64_t)GetProcAddress(_hDbgHelp, "SymUnloadModule64");
            SymSetOptions = (SymSetOptions_t)GetProcAddress(_hDbgHelp, "SymSetOptions");
            SymGetLineFromAddr64 = (SymGetLineFromAddr64_t)GetProcAddress(_hDbgHelp, "SymGetLineFromAddr64");
            SymFunctionTableAccess64 = (SymFunctionTableAccess64_t)GetProcAddress(_hDbgHelp, "SymFunctionTableAccess64");
            SymGetModuleBase64 = (SymGetModuleBase64_t)GetProcAddress(_hDbgHelp, "SymGetModuleBase64");            
            SymEnumSymbols = (SymEnumSymbols_t)GetProcAddress(_hDbgHelp, "SymEnumSymbols");
            SymSetContext = (SymSetContext_t)GetProcAddress(_hDbgHelp, "SymSetContext");
            SymFromAddr = (SymFromAddr_t)GetProcAddress(_hDbgHelp, "SymFromAddr");

            if (!(SymCleanup && this->SymGetSymFromAddr64 && UnDecorateSymbolName && SymLoadModule64 && SymUnloadModule64 && SymSetOptions && SymGetLineFromAddr64))
                return false;

            SymSetOptions(SYMOPT_LOAD_LINES);

            HMODULE hMods[1024];
            DWORD cbNeeded;
            unsigned int i;
            
            if (::EnumProcessModules(_hProcess, hMods, sizeof(hMods), &cbNeeded))
            {
                MODULEINFO mi;

                for (i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
                {
                    GetModuleInformation(_hProcess, hMods[i], &mi, sizeof(mi));

                    char szModName[MAX_PATH] = { 0 };
                    char szModBaseName[MAX_PATH] = { 0 };

                    //
                    // Get the full path to the module's file.
                    //

                    if (GetModuleBaseNameA(_hProcess, hMods[i], szModBaseName, sizeof(szModBaseName) / sizeof(char)) && GetModuleFileNameExA(_hProcess, hMods[i], szModName, sizeof(szModName) / sizeof(char)))
                    {
                        if (SymLoadModule64(_hProcess, 0, szModName, szModBaseName, (DWORD64)mi.lpBaseOfDll, mi.SizeOfImage))
                        {
                            _module_bases.push_back((DWORD64)mi.lpBaseOfDll);
                        }
                    }
                }
            }

            return true;
        }

    public:
        void Release()
        {
            delete this;
        }

        static SymbolLookup* Create(HANDLE hProcess = 0)
        {
            HMODULE hDbgHelp = ::LoadLibraryA("dbghelp.dll");

            if (hDbgHelp == nullptr || hDbgHelp == INVALID_HANDLE_VALUE)
                return nullptr;

            SymInitialize_t symInitialize = (SymInitialize_t)GetProcAddress(hDbgHelp, "SymInitialize");

            if (symInitialize == nullptr)
            {
                ::FreeLibrary(hDbgHelp); 

                return nullptr;
            }

            static char szExeFolder[MAX_PATH] = { 0 };

            GetModuleFileNameA(NULL, szExeFolder, MAX_PATH);
            PathRemoveFileSpecA(szExeFolder);

            if (hProcess == 0)
                hProcess = GetCurrentProcess();

            if (symInitialize(hProcess, NULL, false) == FALSE)
            {
                ::FreeLibrary(hDbgHelp);

                return nullptr;
            }

            SymbolLookup* symLookup = new SymbolLookup(hProcess, hDbgHelp);

            if (symLookup->Initialize())
            {
                return symLookup;
            }

            symLookup->Release();

            return nullptr;
        }

        uint32_t StackWalkInternal(PCONTEXT pContext,DWORD64*  pAdresses, uint32_t max_address_count)
        {
            STACKFRAME64 sf = {0};
            uint32_t count = 0;
            DWORD machine = 0;

#if _WIN64
            sf.AddrFrame.Offset = pContext->Rbp;
            sf.AddrStack.Offset = pContext->Rsp;
            sf.AddrPC.Offset    = pContext->Rip;
            machine = IMAGE_FILE_MACHINE_AMD64;
#else // WIN32
            sf.AddrFrame.Offset = pContext->Ebp;
            sf.AddrStack.Offset = pContext->Esp;
            sf.AddrPC.Offset = pContext->Eip;
            machine = IMAGE_FILE_MACHINE_I386;
#endif
            sf.AddrFrame.Mode = AddrModeFlat;
            sf.AddrStack.Mode = AddrModeFlat;
            sf.AddrPC.Mode    = AddrModeFlat;

            //
            // Walk through the stack frames.
            //

            HANDLE hProcess = GetCurrentProcess();
            HANDLE hThread = GetCurrentThread();

            //
            // TODO: alternative easier method is ::CaptureStackBackTrace
            //

            while (StackWalk64(machine, hProcess, hThread, &sf, pContext, 0, SymFunctionTableAccess64, SymGetModuleBase64, 0))
            {
                if (sf.AddrFrame.Offset == 0 || count >= max_address_count)
                {
                    break;
                }
                
                pAdresses[count] = sf.AddrPC.Offset;
                count++;
            }

            return count;
        }

        uint32_t StackWalkWithFrames(PCONTEXT pContext, DWORD64*  pAdresses, DWORD64* pFrameAddress, uint32_t max_address_count) const
        {
            STACKFRAME64 sf = { 0 };
            uint32_t count = 0;
            DWORD machine = 0;

#if _WIN64
            sf.AddrFrame.Offset = pContext->Rbp;
            sf.AddrStack.Offset = pContext->Rsp;
            sf.AddrPC.Offset = pContext->Rip;
            machine = IMAGE_FILE_MACHINE_AMD64;
#else // WIN32
            sf.AddrFrame.Offset = pContext->Ebp;
            sf.AddrStack.Offset = pContext->Esp;
            sf.AddrPC.Offset = pContext->Eip;
            machine = IMAGE_FILE_MACHINE_I386;
#endif
            sf.AddrFrame.Mode = AddrModeFlat;
            sf.AddrStack.Mode = AddrModeFlat;
            sf.AddrPC.Mode = AddrModeFlat;

            //
            // Walk through the stack frames.
            //

            HANDLE hProcess = GetCurrentProcess();
            HANDLE hThread = GetCurrentThread();

            //
            // TODO: alternative easier method is ::CaptureStackBackTrace
            //

            while (StackWalk64(machine, hProcess, hThread, &sf, pContext, 0, SymFunctionTableAccess64, SymGetModuleBase64, 0))
            {
                if (sf.AddrFrame.Offset == 0 || count >= max_address_count)
                {
                    break;
                }

                pFrameAddress[count] = sf.AddrFrame.Offset;
                pAdresses[count] = sf.AddrPC.Offset;
                count++;
            }

            return count;
        }

        const char* Lookup(DWORD64 addrOffset) const
        {
            DWORD dwDisplacement;
            IMAGEHLP_LINE64 line;
            DWORD64 offsetFromSmybol = 0;

            line.SizeOfStruct = sizeof(IMAGEHLP_LINE64);
            memset(pSymbol, 0, sizeof(IMAGEHLP_SYMBOL64) + METHOD_MAX_NAMELEN);
            pSymbol->SizeOfStruct = sizeof(IMAGEHLP_SYMBOL64);
            pSymbol->MaxNameLength = METHOD_MAX_NAMELEN;
            memset(&_undocratedName[0], 0, METHOD_MAX_NAMELEN);
            
            IMAGEHLP_MODULE64 moduleInfo = { 0 };
            moduleInfo.SizeOfStruct = sizeof(IMAGEHLP_MODULE64);
            SymGetModuleInfo64(_hProcess, addrOffset, &moduleInfo);
            PathStripPathA(moduleInfo.ImageName);

            if (SymGetSymFromAddr64(_hProcess, addrOffset, &offsetFromSmybol, pSymbol) != FALSE)
            {
                UnDecorateSymbolName(pSymbol->Name, _undocratedName, METHOD_MAX_NAMELEN, UNDNAME_COMPLETE);
                //UnDecorateSymbolName(pSymbol->Name, _undocratedName, METHOD_MAX_NAMELEN, UNDNAME_NAME_ONLY);

                if (SymGetLineFromAddr64(_hProcess, addrOffset, &dwDisplacement, &line))
                {
                    PathStripPathA(line.FileName);
                    sprintf(_lookup_data, "[%s] %s(%d): %s", moduleInfo.ImageName, line.FileName, line.LineNumber, _undocratedName);
                }
                else
                {
                    sprintf(_lookup_data, "[%s] %s [missing symbol information]", moduleInfo.ImageName, pSymbol->Name);
                }

                return _lookup_data;
            }
            else
            {
                sprintf(_lookup_data, "%s:x%llx()", moduleInfo.ImageName, addrOffset);
            }

            return _lookup_data;
        }

        const LPVOID GetThisPointerOfType(const char* fulltype, DWORD64 addrOffset) const
        {
            _currentThisPointer = nullptr;
            CONTEXT context;
            RtlCaptureContext(&context);
            DWORD64 vAddrStack[64];
            DWORD64 vFrameStack[64];
            uint32_t stack_count = StackWalkWithFrames(&context, vAddrStack, vFrameStack, 64);
            //LPVOID cur = (LPVOID)addrOffset;

            BOOL bRet = FALSE;
            SYMBOL_INFO_PACKAGE sip = { 0 };
            sip.si.SizeOfStruct = sizeof(SYMBOL_INFO);
            sip.si.MaxNameLen = sizeof(sip.name);
            DWORD64 Displacement = 0;
            int curStackIndex = 0;
            _currentType = fulltype;

            for (unsigned int i = 0; i < stack_count;++i)
            {
                if (vAddrStack[i] == addrOffset)
                {
                    _currentFrame = (LPVOID)vFrameStack[i];
                    curStackIndex = i;
                    break;
                }
            }

            bRet = SymFromAddr(
                _hProcess, 
                addrOffset,
                &Displacement,
                &sip.si
            );

            if (sip.si.Tag == SymTagFunction)
            {
                IMAGEHLP_STACK_FRAME sf = {0};

                sf.InstructionOffset = addrOffset;
                bRet = SymSetContext(
                    _hProcess,
                    &sf,
                    0
                );

                bRet = SymEnumSymbols(
                    _hProcess, 
                    0,                   
                    0,                  
                    EnumThisSymbolsCallback,
                    (void*)this
                );
            }

            return _currentThisPointer;
        }

    private:
        SYMBOL_INFO                 si;
        IMAGEHLP_SYMBOL64           *pSymbol;
        enum { METHOD_MAX_NAMELEN = 1024 };
        mutable char                _undocratedName[METHOD_MAX_NAMELEN];
        mutable char                _lookup_data[METHOD_MAX_NAMELEN * 2];
        SymInitialize_t             SymInitialize;
        SymCleanup_t                SymCleanup;
        SymGetSymFromAddr64_t       SymGetSymFromAddr64;
        UnDecorateSymbolName_t      UnDecorateSymbolName;
        SymLoadModule64_t           SymLoadModule64;
        SymUnloadModule64_t         SymUnloadModule64;
        SymSetOptions_t             SymSetOptions;
        SymGetLineFromAddr64_t      SymGetLineFromAddr64;
        SymFunctionTableAccess64_t  SymFunctionTableAccess64;
        SymGetModuleBase64_t        SymGetModuleBase64;
        SymEnumSymbols_t            SymEnumSymbols;
        SymSetContext_t             SymSetContext;
        SymFromAddr_t               SymFromAddr;
        HANDLE                      _hProcess;
        HMODULE                     _hDbgHelp;
        std::vector<DWORD64>        _module_bases;
        mutable PVOID               _currentThisPointer;
        mutable PVOID               _currentFrame;
        mutable std::string         _currentType;
    };
}