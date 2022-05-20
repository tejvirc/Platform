// mkaudio.cpp : Defines the entry point for the console application.
//


#include "stdafx.h"

#define INITGUID
const GUID nullGuid = { 0 };

#include <Mmdeviceapi.h>
#include <Endpointvolume.h>
#include <Audiopolicy.h>
#include <atlstr.h>
#include <mmdeviceapi.h>
#include <devicetopology.h>
#include <functiondiscoverykeys.h>
#include <Propvarutil.h>
#include "optionparser.h"
#include <vector>
#pragma comment(lib,"Propsys")

using namespace std;
using namespace aristocrat;

class CCoInitialize {
public:
	operator bool() const { return SUCCEEDED(_hr);}
	bool const operator ! () const { return  FAILED(_hr); }
	HRESULT result() { return _hr; }
private:
	HRESULT _hr;
public:
	CCoInitialize()
		: _hr(E_UNEXPECTED) {
		_hr = CoInitialize(NULL);
	}
	~CCoInitialize() { if (SUCCEEDED(_hr)) { CoUninitialize(); } }
};

class CCoTaskMemFree {
private:
	void* _p;
public:
	CCoTaskMemFree(void* p) : _p(p) {}
	~CCoTaskMemFree() { CoTaskMemFree(_p); }
};

enum  optionIndex { UNKNOWN, HELP, VOLUME, PID, SAVESTORE, SOURCE, DEST, PRINTSTORE, PRINTPROP, RESTORE, LIST, MASTER, PAUSE
};

const option::Descriptor usage[] =
{
    { UNKNOWN,  "", "", option::Arg::None, "USAGE: example [options]\n\n"
    "Options:" },
    { HELP,			"h", "help", option::Arg::None,             "  --help                       Print usage and exit." },
    { VOLUME,		"v", "volume", option::Arg::Numeric,        "  -v,--volume <vol>            the volume (0-100) to be set." },
    { PID,			"p", "pid", option::Arg::Numeric,           "  -p,--pid                     set the volume of process id." },
    { SAVESTORE,	"s", "save", option::Arg::String,           "  -s,--save <filename>         Save Audio Endpoint Properties (or diff) store to file." },
    { SOURCE,		"1", "source", option::Arg::String,         "  -1,--source <filename>       source comparator of property store." },
    { DEST,			"2", "dest", option::Arg::String,           "  -2,--dest <filename>         destination comparator of property store." },
    { PRINTSTORE,	"ps", "printstore", option::Arg::String,    "  -ps,--printstore <filename>  prints the store file." },
    { PRINTPROP,	"pp", "printprops", option::Arg::None,      "  -pp,--printprops             prints current properties." },
    { RESTORE,		"r", "restore", option::Arg::String,        "  -r,--restore <filename>      restore Audio Endpoint Properties from file." },
    { LIST,			"l", "list", option::Arg::None,             "  -l,--list                    list all devices." },
    { MASTER,		"m", "master", option::Arg::None,           "  -m,--master                  set the volume of master mixer." },
    { PAUSE,		"b", "pause", option::Arg::None,            "  -b,--pause                   wait for keypress at exit." },
    { UNKNOWN,  "", "", option::Arg::None, "\nExamples:\n"
    "  mkaudio64.exe -m -v 10 \n"
    "  mkaudio64.exe -p 0 -v 10 \n"
    "  mkaudio64.exe -r surround_config.bin \n" },
    { 0, 0, 0, option::Arg::None, 0 }
};

struct serialize_data
{
	ULONG size;
	PROPERTYKEY key;
	SERIALIZEDPROPERTYVALUE* spv;
		
};
typedef  std::vector<serialize_data> PropStore;

void PrintProp(const PROPVARIANT& pv, const PROPERTYKEY& key)
{
	WCHAR keystring[1024];
	PropVariantToString(pv, keystring, 1024);
	const GUID &guid = key.fmtid;
	printf("{%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X} [%d] = %ls\n", guid.Data1, guid.Data2, guid.Data3, guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7], key.pid, keystring);
}

void FreePropStore(PropStore& propdata)
{
	for (auto &v : propdata)
	{
		if (v.spv != nullptr && v.size != 0)
		{
			delete[](char*)v.spv;
		}
	}
	propdata.clear();
}
bool SavePropStore(PropStore& propdata, const char* filename)
{
	FILE* f = nullptr;
	fopen_s(&f, filename, "wb");
	if (f == 0) return false;
	for (auto& v : propdata)
	{
		fwrite(&v.size, 1, sizeof(ULONG), f);
		fwrite(&v.key, 1, sizeof(PROPERTYKEY), f);
		fwrite(v.spv, 1, v.size, f);
	}
	fclose(f);
	return true;
}

bool GetPropStore(CComPtr<IPropertyStore>& pProps, PropStore& propdata)
{
	FreePropStore(propdata);
	
	DWORD propCount = 0;
	pProps->GetCount(&propCount);
		
	for (DWORD i = 0; i < propCount; ++i)
	{

		PROPERTYKEY key;
		PROPVARIANT vPropI;

		PropVariantInit(&vPropI);
		pProps->GetAt(i, &key);
		pProps->GetValue(key, &vPropI);
		ULONG spvSize = 1024;
		SERIALIZEDPROPERTYVALUE* spv= nullptr;
		StgSerializePropVariant(&vPropI, &spv, &spvSize);
		SERIALIZEDPROPERTYVALUE* spvbuffer = (SERIALIZEDPROPERTYVALUE*)new char[spvSize];
		memcpy(spvbuffer, spv, spvSize );
		propdata.push_back({ spvSize, key, spvbuffer });
		PropVariantClear(&vPropI);
		
	}

	return true;
}

void RemoveDupes(PropStore &dest, const PropStore &source)
{
	PropStore delta;
	bool dupe = false;
	for (auto & d : dest)
	{
		dupe = false;
		for (auto & s : source)
		{
			if (memcmp(&d.key, &s.key, sizeof(PROPERTYKEY)) == 0 &&
				d.size == s.size &&
				memcmp(d.spv, s.spv, d.size) == 0)
			{
				delete[] d.spv;
				d.spv = nullptr;
				d.size = 0;
				dupe = true;
			}
		}
		
		if(!dupe)
		{
			delta.push_back(d);
		}
	}

	dest = delta;
}

bool LoadPropStore(PropStore& propdata, const char* filename)
{
	FreePropStore(propdata);

	FILE* f = nullptr;
	fopen_s(&f, filename, "rb");
	if (f == 0) return false;
	while (!feof(f))
	{
		serialize_data v;

		fread(&v.size, 1, sizeof(ULONG), f);
		fread(&v.key, 1, sizeof(PROPERTYKEY), f);
		v.spv = (SERIALIZEDPROPERTYVALUE*)new char[v.size];
		size_t res = fread(v.spv, 1, v.size, f);
		if (res == (size_t)v.size)
		{
			propdata.push_back(v);
		}
	}
	fclose(f);
	return true;
}

bool SaveDiffPropStore(const char* filenameA, const char* filenameB, const char* outfile)
{
	PropStore propdataA;
	PropStore propdataB;
	if (LoadPropStore(propdataA, filenameA) && LoadPropStore(propdataB, filenameB))
	{
		RemoveDupes(propdataA, propdataB);
		SavePropStore(propdataA, outfile);
		FreePropStore(propdataA);
		FreePropStore(propdataB);
		
		return true;
	}

	return false;
}

bool PrintPropStore(const PropStore& propdata)
{

	for (auto &v: propdata)
	{
		PROPVARIANT vProp;
		SERIALIZEDPROPERTYVALUE* spv = v.spv;
		
		StgDeserializePropVariant(spv,v.size,&vProp);
		
		PrintProp(vProp,v.key);
		PropVariantClear(&vProp);

	}

	return true;
}

bool RestorePropStore(CComPtr<IPropertyStore>& pProps, PropStore& propdata)
{
	bool result = true;
	DWORD propCount = 0;
	pProps->GetCount(&propCount);

	for (auto &v : propdata)
	{
		PROPVARIANT vProp;
		SERIALIZEDPROPERTYVALUE* spv = v.spv;

		StgDeserializePropVariant(spv, v.size, &vProp);
		
		HRESULT hr = pProps->SetValue(v.key, vProp);
		WCHAR keystring[1024];
		PropVariantToString(vProp, keystring, 1024);
		const GUID &guid = v.key.fmtid;

		if (FAILED(hr))
		{
			
			printf("Failed Writing: {%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X} [%d] = %ls\n", guid.Data1, guid.Data2, guid.Data3, guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7], v.key.pid, keystring);			
			result = false;
		}
		else
		{
			printf("Wrote: {%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X} [%d] = %ls\n", guid.Data1, guid.Data2, guid.Data3, guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7], v.key.pid, keystring);
		}
		PropVariantClear(&vProp);
	}

	return result;
}

//
// Retrieves the version number from the resource file
//

bool GetVersionFromRC(WCHAR* wstrProductVersion, UINT32 wstrProductionVersionLength)
{
    bool bSuccess = false;
    BYTE* rgData = NULL;

    // get the filename of the executable containing the version resource
    TCHAR szFilename[MAX_PATH + 1] = { 0 };
    if (GetModuleFileName(NULL, szFilename, MAX_PATH) == 0)
    {
        wcout << L"ERROR: GetVersionFromRC(): GetModuleFileName failed with " << GetLastError() << endl;
        goto error;
    }

    // allocate a block of memory for the version info
    DWORD dummy;
    DWORD dwSize = GetFileVersionInfoSize(szFilename, &dummy);
    if (dwSize == 0)
    {
        wcout << L"ERROR: GetVersionFromRC(): GetFileVersionInfoSize failed with " << GetLastError() << endl;
        goto error;
    }

    rgData = new BYTE[dwSize];
    if (rgData == NULL)
    {
        wcout << L"ERROR: GetVersionFromRC(): Unable to allocate memory" << endl;
        goto error;
    }

    // load the version info
    if (!GetFileVersionInfo(szFilename, NULL, dwSize, &rgData[0]))
    {
        wcout << L"ERROR: GetVersionFromRC(): GetFileVersionInfo failed with " << GetLastError() << endl;
        goto error;
    }

    // get the name and version strings
    LPVOID pvProductVersion = NULL;
    unsigned int iProductVersionLen = 0;

    // "040904b0" is the language ID of the resources
    if (!VerQueryValue(&rgData[0], _T("\\StringFileInfo\\040904b0\\ProductVersion"), &pvProductVersion, &iProductVersionLen))
    {
        wcout << L"ERROR: GetVersionFromRC(): VerQueryValue: Unable to get ProductVersion from the resources." << endl;
        goto error;
    }

    if (0 != wcscpy_s(wstrProductVersion, wstrProductionVersionLength, (WCHAR*)pvProductVersion))
    {
        wcout << L"ERROR: GetVersionFromRC(): wcscpy_s failed!" << endl;
        goto error;
    }

    bSuccess = true;

error:
    SAFE_ARRAY_DELETE(rgData)

        return bSuccess;
}

int main(int argc, char** argv)
{
	HRESULT			hr;
	CCoInitialize	coinit;
	int				vol = 100;
	float			fVolume = 1.0f;
	option::Options opts((option::Descriptor*)usage);
    WCHAR wstrDisManVersion[MAX_PATH];

    if (GetVersionFromRC(wstrDisManVersion, MAX_PATH))
    {
        wcout << endl << L"Aristocrat MkAudio " << wstrDisManVersion << endl;
    }
    else
    {
        wcout << endl << L"Aristocrat MkAudio" << endl;
    }

	argc -= (argc > 0); argv += (argc > 0);

	if (!coinit)
	{
        wcout << endl;
        printf("CoInitialize failed : hr = 0x%08x\n", coinit.result());
		return __LINE__;
	}

	if (!opts.Parse(argv, argc))
	{
        wcout << endl;
        printf(opts.error_msg());
		opts.PrintHelp();
		return __LINE__;
	}

	if (opts[HELP] || (0 == argc))
	{
        wcout << endl;
        opts.PrintHelp();
		return __LINE__;
	}
	if ((opts[MASTER] || opts[PID]) && !opts[VOLUME])
	{
        wcout << endl;
        printf("volume argument missing for options Master or Pid");
		opts.PrintHelp();
		return __LINE__;
	}

	if (opts[VOLUME])
	{
		opts.GetArgument(VOLUME, vol);
		fVolume = vol / 100.0f;
	}
    
	CComPtr<IMMDeviceEnumerator> pMMDeviceEnumerator;
	hr = pMMDeviceEnumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));
	if (FAILED(hr)) {
        wcout << endl;
        printf("IMMDeviceEnumerator::CoCreateInstance() failed: hr = 0x%08x\n", hr);
		return __LINE__;
	}

	// get default render/capture endpoints
	CComPtr<IMMDevice> pRenderEndpoint;
	hr = pMMDeviceEnumerator->GetDefaultAudioEndpoint(eRender, eMultimedia, &pRenderEndpoint);
	if (FAILED(hr)) {
        wcout << endl;
		printf("IMMDeviceEnumerator::GetDefaultAudioEndpoint() failed: hr = 0x%08x\n", hr);
		return __LINE__;
	}

	if (opts[RESTORE])
	{
		CComPtr<IPropertyStore> pProps = NULL;
		hr = pRenderEndpoint->OpenPropertyStore(STGM_READWRITE, &pProps);
		if (FAILED(hr))
		{
            wcout << endl;
			printf("IMMDevice::OpenPropertyStore(STGM_READWRITE) - Failed to open prop store  hr = 0x%08x\n", hr);
			printf("Requires Administrator.\n");
			return __LINE__;
		}
		PropStore loadstore;
		LoadPropStore(loadstore, opts.GetValue(RESTORE));
		printf("Restoring %d properties.\n", (int)loadstore.size());
		bool success = RestorePropStore(pProps, loadstore);
		// TODO: write store
		FreePropStore(loadstore);
		
		if (!success)
		{
			return 1;
		}
	}

	if (opts[SOURCE] && opts[DEST] && opts[SAVESTORE])
	{
		SaveDiffPropStore(opts.GetValue(SOURCE), opts.GetValue(DEST), opts.GetValue(SAVESTORE));
	}
	else if (opts[SAVESTORE])
	{
		CComPtr<IPropertyStore> pProps = NULL;
		hr = pRenderEndpoint->OpenPropertyStore(STGM_READ, &pProps);
		if (FAILED(hr))
		{
            wcout << endl;
			printf("IMMDevice::OpenPropertyStore(STGM_READ) - Failed to open prop store  hr = 0x%08x\n", hr);
			return __LINE__;
		}
		PropStore savestore;
		GetPropStore(pProps, savestore);
		SavePropStore(savestore, opts.GetValue(SAVESTORE));
		FreePropStore(savestore);
	}
	
	if (opts[PRINTSTORE])
	{
	
		PropStore printstore;
		LoadPropStore(printstore, opts.GetValue(PRINTSTORE));
		PrintPropStore(printstore);
		FreePropStore(printstore);
	}

	if (opts[PRINTPROP])
	{
		CComPtr<IPropertyStore> pProps = NULL;
		hr = pRenderEndpoint->OpenPropertyStore(STGM_READ, &pProps); // STGM_READ
		if (FAILED(hr))
		{
            wcout << endl;
			printf("IMMDevice::OpenPropertyStore(STGM_READWRITE) - Failed to open prop store  hr = 0x%08x\n", hr);
			return __LINE__;
		}
		PropStore printstore;
		GetPropStore(pProps, printstore);
		PrintPropStore(printstore);
		FreePropStore(printstore);
	}
	//
	if (opts[MASTER])
	{
		CComPtr<IAudioEndpointVolume> pMasterVol;
		//pRenderEndpoint->Activate(IID_IAudioEndpointVolume, CLSCTX_ALL,nullptr, (void**)&pMasterVol);
		pRenderEndpoint->Activate(__uuidof(IAudioEndpointVolume), CLSCTX_ALL, NULL, (void**)&pMasterVol);
		
		pMasterVol->SetMasterVolumeLevelScalar(fVolume, &nullGuid);
		if (fVolume > 0)
		{
			pMasterVol->SetMute(false, &nullGuid);
		}
		else
		{
			pMasterVol->SetMute(true, &nullGuid);
		}
		return 0;
	}

	int nPid = 0;
	if(opts[PID] || opts[LIST])
	{
		opts[PID]?opts.GetArgument(PID,nPid):((void)0);
		CComPtr<IAudioSessionManager2> pAudioSession = nullptr;
		CComPtr<IAudioSessionEnumerator> pSessionEnumerator = nullptr;
		hr = pRenderEndpoint->Activate(__uuidof(IAudioSessionManager2),CLSCTX_ALL,NULL, (void**)&pAudioSession);
		if(FAILED(hr) || pAudioSession == nullptr)
		{
            wcout << endl;
			printf("IAudioEndpointVolume::Activate(IAudioSessionManager2) failed: hr = 0x%08x\n", hr);
			return __LINE__;
		}
		hr = pAudioSession->GetSessionEnumerator(&pSessionEnumerator);
		if (FAILED(hr) || pSessionEnumerator == nullptr)
		{
            wcout << endl;
			printf("IAudioSessionManager2::GetSessionEnumerator() failed: hr = 0x%08x\n", hr);
			return __LINE__;
		}
		int count = 0;
		hr = pSessionEnumerator->GetCount(&count);
		if(FAILED(hr))
		{
            wcout << endl;
			printf("IAudioSessionEnumerator::GetCount() failed: hr = 0x%08x\n", hr);
			return __LINE__;
		}
		CComPtr<ISimpleAudioVolume> pVolumeControl = nullptr;
		for (int i = 0; i < count; ++i)
		{
			CComPtr<IAudioSessionControl> ctl = nullptr;
			CComPtr<IAudioSessionControl2> ctl2 = nullptr;
			
			hr = pSessionEnumerator->GetSession(i, &ctl);
			if(SUCCEEDED(hr))
			{
				LPWSTR szDisplayName;
				LPWSTR szSessionId;
				DWORD pid;
				// NOTE: we could also use the app name from ctl.GetDisplayName()
				if(SUCCEEDED(ctl.QueryInterface(&ctl2)) && ctl2 != nullptr)
				{
					ctl2->GetDisplayName(&szDisplayName);
					ctl2->GetProcessId(&pid);
					ctl2->GetSessionIdentifier(&szSessionId);

					CCoTaskMemFree freeDisplayName(szDisplayName);
					CCoTaskMemFree freeSessionId(szSessionId);
					GUID guid;
					ctl2->GetGroupingParam(&guid);
					if(opts[PID])
					{
						if(pid == (DWORD)nPid)
						{
							hr = ctl->QueryInterface(&pVolumeControl);
							if(FAILED(hr))
							{
                                wcout << endl;
								printf("IAudioSessionControl::QueryInterface(ISimpleAudioVolume) failed: hr = 0x%08x\n", hr);
								return __LINE__;
							}
							pVolumeControl->SetMasterVolume(fVolume, &nullGuid);
                            pVolumeControl->SetMute(false, &nullGuid);
							return 0;
						}
					}

					if(opts[LIST])
					{
						CComBSTR guidBstr(guid);
						printf("DisplayName: '%ls'[(pid: %d guid: %ls]\n\t\tsession: %ls\n", szDisplayName,pid, guidBstr.m_str, szSessionId);
					}
					
				}else
				{
					printf("IAudioSessionControl::QueryInterface(IAudioSessionControl2) failed: hr = 0x%08x\n", hr);
				}
			}
		}

	}
	if(opts[PAUSE])
	{
		getchar();
	}
    return 0;
}

