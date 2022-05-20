// sign-ali.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <stdint.h>
#include <conio.h>
extern "C"
{
	#include "m7i_crypt_support.h"
	#include "m7i_settings.h"
}
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

int hash_mbr(const byte* mbr, byte* gen_hash, unsigned long &hashlen)
{
	hash_state mst;
	int rv = 0;
	if (hashlen != 20)
	{
		printf("hash_mbr(): hashlen must be 20.\n");
		return 1;
	}
	hashlen = 20; /* must be set to sizeof */
	if ((rv = sha1_init(&mst)) != CRYPT_OK)
	{
		printf("failed to initialize hmac hash.\n");
	}
	
	sha1_process(&mst, mbr, 0x178);		/* Boot Code */
	
	if (mbr[0x1BC] != 0xFF) /* include PT in hash */
	{
		sha1_process(&mst, &mbr[0x1B4], 0x200-0x1B4); /* DI DI DI DI + FF/00 CHECK + PT + SIG */
	}
	else /* Ignore DI & PT i hash */
	{
		sha1_process(&mst, &mbr[0x1B4], 4); /* BL Se BL Si */
		sha1_process(&mst, &mbr[0x1BC], 2); /* FF/00 CHECK */
		sha1_process(&mst, &mbr[0x1FE], 2); /* SIG */
	}
	
	if ((rv = sha1_done(&mst, (byte*)gen_hash)) != CRYPT_OK)
	{
		printf("failed finalizing hash.\n");
		return 2;
	}

	return 0;
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
		printf("\nAristocrat Extracthash-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Extracthash-ali\n");
	}

	register_hash(&sha1_desc);
	ltc_mp = ltm_desc;

	unsigned long hashlen = 0;
	unsigned char gen_hash[32];
	
	char hashfilename[MAX_PATH];
	char devicename[MAX_PATH];
	byte* mbr = new byte[SECTOR_SIZE];
	byte* block = new byte[SECTOR_SIZE];
	
	char bootloaderhash_filename[MAX_PATH];
	char mbrhash_filename[MAX_PATH];

	HANDLE hDevice = INVALID_HANDLE_VALUE;
	hash_state mst;
	DWORD bytesRead = 0;
	DWORD bytesWritten = 0;
	int deviceId = 0;

	int rv = 0;

	if (argc < 2)
	{
		printf("\nUsage: extracthash-ali.exe <deviceindex or file>\n\nExample: extracthash-ali.exe 2\n");
		return 0;
	}

	if (::isdigit(*argv[1]))
	{
		deviceId = atoi(argv[1]);
		if (deviceId == 0)
		{
			printf("This is likely not the drive you wan't to touch, think again %d\n", deviceId);
			return 1;
		}

		sprintf(devicename, "\\\\.\\PhysicalDrive%d", deviceId);
		printf("You have selected Device %d = %s\n", deviceId, devicename);
		sprintf(hashfilename, "PhysicalDrive%d", deviceId);
	}
	else
	{
		strcpy(devicename, argv[1]);
		printf("You have selected file %s \n", devicename);
		strcpy(hashfilename, devicename);
	}

	hDevice = CreateFile(devicename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
	if (hDevice == INVALID_HANDLE_VALUE)
	{
		printf("Access denied, failed to opening device %s\n", devicename);
		goto fail;
	}
	if (!ReadFile(hDevice, mbr, SECTOR_SIZE, &bytesRead, 0))
	{
		printf("Failed reading MBR from device %s\n", devicename);
		goto fail;
	}

	//printf("Are you sure you want to continue? [nothing will be written yet] (Y/n)");
	//char answer = getYesNo();

	//if (answer != 'y' && answer != 'Y')
	//{
	//	printf("Aborting.\n");
	//	goto fail;
	//}
	//printf("\n");

	uint64_t start = 1;
	uint64_t end   = 1006; /* end sector, according to grub/boot.h:ARISTOCRAT_BLD_START  specified in Boot.S +1 (bug in bios) */
	uint64_t bytes = ((uint64_t)end - start) << SECTOR_BITS;

	LARGE_INTEGER moveTo;
	LARGE_INTEGER currentPos;
	moveTo.QuadPart = ((uint64_t)start) << SECTOR_BITS;
	SetFilePointerEx(hDevice, moveTo, &currentPos, FILE_BEGIN);
	if (moveTo.QuadPart != currentPos.QuadPart)
	{
		printf("Failed to seek to partition start, Partition table must be wrong!\n");
		goto fail;
	}

	if ((rv = sha1_init(&mst)) != CRYPT_OK)
	{
		printf("failed to initialize hmac hash.\n");
	}
	
	uint64_t current_sector = start;
	uint32_t read_size = SECTOR_SIZE;
	while (current_sector < end )
	{
		memset(block, 0, SECTOR_SIZE);
		if (!ReadFile(hDevice, block, read_size, &bytesRead, 0))
		{
			printf("Failed reading sector from device %s\n", devicename);
			goto fail;
		}
		
		if (bytesRead < read_size) /* pad with zeroes */
		{
			memset(&block[bytesRead],0 , SECTOR_SIZE - bytesRead);
			bytesRead = SECTOR_SIZE;
		}
		
		current_sector += bytesRead >> SECTOR_BITS;
		printf("Hashing Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - start), (uint64_t)(end - start), ((double)(current_sector - start)/(double)(end - start))*100);
		sha1_process(&mst, block, bytesRead);
	}

	hashlen = 20;
	if ((rv = sha1_done(&mst, (unsigned char*)gen_hash)) != CRYPT_OK)
	{
		printf("failed finalizing hash.\n");
		goto fail;
	}

	printf("\nHash for %llu -> %llu (0x%08llx): ",start, end, (current_sector*512));
	for (unsigned long i = 0; i < hashlen; ++i)
	{
		printf("%02x", gen_hash[i]);
	}


	printf("\nHash Complete.\n");
	
	sprintf(bootloaderhash_filename,"%s.bootloader.hash", hashfilename);
	sprintf(mbrhash_filename, "%s.mbr.hash", hashfilename);

	if (WriteHash(bootloaderhash_filename, gen_hash, hashlen) != 0)
	{
		printf("Writing bootloader hash to file '%s' failed, ", bootloaderhash_filename);
	}
	
	
	if (hash_mbr(mbr, gen_hash, hashlen))
	{
		printf("extracting hash from mbr failed, bad mbr code in '%s' failed, ", devicename);
		goto fail;
	}
	if ( WriteHash(mbrhash_filename, gen_hash, hashlen) != 0)
	{
		printf("Writing mbr hash to file '%s' failed, ", mbrhash_filename);
	}


fail:
	CloseHandle(hDevice);
	delete[] mbr;
	delete[] block;
    return 0;
}

