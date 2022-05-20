// sign-ali.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <stdint.h>
#include <conio.h>

#define uint8_t byte;

char getYesNo()
{
	char answer = 0;
	while (1)
	{
		answer = _getch();
		if (answer == 'y') break;
		if (answer == 'Y') break;
		if (answer == 'n') break;
		if (answer == 'N') break;
	}
	return answer;
}

#define		SECTOR_BITS 9
#define		SECTOR_SIZE (1 << 9)

int WriteHash(const char* filename, const byte* hash, int hashlen)
{
	FILE* f = fopen(filename, "wb");
	if (f == 0) return 1;
	for (int i = 0; i < hashlen; ++i)
	{
		fprintf(f, "%02x", hash[i]);
	}
	fclose(f);
	return 0;
}

int WriteBin(const char* filename, const byte* hash, int hashlen)
{
	FILE* f = fopen(filename, "wb");
	if (f == 0) return 1;
	fwrite(hash, hashlen,1, f);
	fclose(f);
	return 0;
}


inline char fromhex(char d)
{
	return (d >= '0' && d <= '9') ? d - '0' :
		(d >= 'a' && d <= 'f') ? d - 'a' + 10 :
		(d >= 'A' && d <= 'F') ? d - 'A' + 10 :
		0xff;	// Indicates parse error
}

inline int parsehex(unsigned char* dest, uint64_t max_bytes, const char* src)
{
	if (src == 0 || dest == 0) return 0;

	const char* p = src;
	int i = 0;
	while (i < max_bytes)
	{
		unsigned char a = fromhex(*p++);
		unsigned char b = fromhex(*p++);
		if (a == 0xff || b == 0xff)
			return i;
		dest[i++] = a << 4 | b;
	}
	return i;
}

//
// Retrieves the version number from the resource file
//

bool GetVersionFromRC(char* strProductVersion, UINT32 strProductionVersionLength)
{
	bool bSuccess = false;
	BYTE* rgData = NULL;

	// get the filename of the executable containing the version resource
	char szFilename[MAX_PATH + 1] = { 0 };
	if (GetModuleFileName(NULL, szFilename, MAX_PATH) == 0)
	{
		printf("ERROR: GetVersionFromRC(): GetModuleFileName failed with %d\n", GetLastError());
		goto error;
	}

	// allocate a block of memory for the version info
	DWORD dummy;
	DWORD dwSize = GetFileVersionInfoSize(szFilename, &dummy);
	if (dwSize == 0)
	{
		printf("ERROR: GetVersionFromRC(): GetFileVersionInfoSize failed with %d\n", GetLastError());
		goto error;
	}

	rgData = new BYTE[dwSize];
	if (rgData == NULL)
	{
		printf("ERROR: GetVersionFromRC(): Unable to allocate memory\n");
		goto error;
	}

	// load the version info
	if (!GetFileVersionInfo(szFilename, NULL, dwSize, &rgData[0]))
	{
		printf("ERROR: GetVersionFromRC(): GetFileVersionInfo failed with %d\n", GetLastError());
		goto error;
	}

	// get the name and version strings
	LPVOID pvProductVersion = NULL;
	unsigned int iProductVersionLen = 0;

	// "040904b0" is the language ID of the resources
	if (!VerQueryValue(&rgData[0], "\\StringFileInfo\\040904b0\\ProductVersion", &pvProductVersion, &iProductVersionLen))
	{
		printf("ERROR: GetVersionFromRC(): VerQueryValue: Unable to get ProductVersion from the resources.\n");
		goto error;
	}

	if (0 != strcpy_s(strProductVersion, strProductionVersionLength, (char*)pvProductVersion))
	{
		printf("ERROR: GetVersionFromRC(): strcpy_s failed!\n");
		goto error;
	}

	bSuccess = true;

error:
	SAFE_ARRAY_DELETE(rgData)

	return bSuccess;
}

int main(int argc, const char** argv)
{
	CHAR strVersion[MAX_PATH];

	if (GetVersionFromRC(strVersion, MAX_PATH))
	{
		printf("\nAristocrat Hexconv-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Hexconv-ali\n");
	}

	unsigned long hashlen = 0;
	unsigned char hexdata[512];
	
	if (argc < 3)
	{
		printf("\nUsage: hexconv-ali.exe hex outfile.bin");
		return 1;
	}
	int len = parsehex(hexdata, 512, argv[1]);
	WriteBin(argv[2], hexdata, len);
    return 0;
}

