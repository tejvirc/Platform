// sign-ali.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <stdint.h>
#include <conio.h>
#include <getopt.h>
extern "C"
{
	#include "m7i_crypt_support.h"
	#include "m7i_settings.h"
}
#define uint8_t byte;


static int yestoall = 0;

char getYesNo()
{
	if (yestoall != 0) return 'y';
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

bool confirm(const char* text, ...)
{
	if (yestoall) return true;

	char szBuffer[4096] = {};

	va_list argptr;
	va_start(argptr, text);
	if (-1 == (vsprintf(szBuffer, text, argptr)))
	{
		assert(false);
	}
	va_end(argptr);

	printf("%s (Y/n)", szBuffer);
	char answer = getYesNo();

	if (answer == 'y' || answer == 'Y')
	{
		printf("\n");
		return true;
	}
	printf("\n");
	return false;
}

#pragma pack(1)
enum PartitionStatus : byte
{
	Inactive = 0x00,
	Active = 0x80
};


struct CHS
{
	byte Head;
	byte Sector;
	byte Cylindar;
};


struct PartitionTable
{
	PartitionStatus Status;
	CHS Start;
	byte Type;
	CHS End;
	uint32_t LBAStart;
	uint32_t NumSectors;
};
#pragma pack()



int WriteHash(const char* filename, const byte* hash, int hashlen)
{
	printf("Writing Hash %s\n", filename);
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
	printf("Writing Binary %s\n", filename);
	FILE* f = fopen(filename, "wb");
	if (f == 0) return 1;
	fwrite(hash, hashlen, 1, f);
	fclose(f);
	return 0;
}

//
// Retrieves the version number from the resource file
//

int GetVersionFromRC(char* strProductVersion, int strProductionVersionLength)
{
	int Success = -1;
	char* rgData = NULL;

	// get the filename of the executable containing the version resource
	char szFilename[MAX_PATH + 1] = { 0 };
	if (GetModuleFileName(NULL, szFilename, MAX_PATH) == 0)
	{
		printf("ERROR: GetVersionFromRC(): GetModuleFileName failed with %d\n", GetLastError());
		goto error;
	}

	// allocate a block of memory for the version info
	unsigned long dummy;
	unsigned long dwSize = GetFileVersionInfoSize(szFilename, &dummy);
	if (dwSize == 0)
	{
		printf("ERROR: GetVersionFromRC(): GetFileVersionInfoSize failed with %d\n", GetLastError());
		goto error;
	}

	rgData = (char*)malloc(dwSize);
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
	void* pvProductVersion = NULL;
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

	Success = 0;

error:
	SAFE_FREE(rgData)

		return Success;
}

#define		SECTOR_BITS 9
#define		SECTOR_SIZE (1 << 9)
#define		BULK_SECTORS (1024*4)
#define		BULK_SIZE   (BULK_SECTORS << SECTOR_BITS)
#define		MB	1024*1024
#define		GB	1024*1024*1024

// Display command line arguments
void help(char *exe) {
	printf("\nUsage: %s [OPTION..] <DIR>\n", exe);
	printf("Sign partition\n");
	printf("Writes to file or drive \n");
	printf("\n");
	printf("Options:\n");
	printf("  -p, --partition        Partition (0=root, 1-4)\n");
	printf("  -d, --disk             Disk id or file\n");
	printf("  -k, --key              Private Key\n");
	printf("\nOutputs to drive\n");
}

int main(int argc, char** argv)
{
	register_hash(&sha1_desc);
	register_hash(&sha256_desc);
	register_prng(&sprng_desc);
	ltc_mp = ltm_desc;
	char		  c;
	dsa_key		dsakey_priv;
	unsigned char	 dsig[2048];
	unsigned long	 dsiglen;
	unsigned long hashlen = 0;
	unsigned char gen_hash[32];
	unsigned char keybuf[4096]; 
	int			  keylen;
	char*		  keyfile = NULL;
	
	byte* block = new byte[BULK_SIZE];
	byte* mbr = new byte[SECTOR_SIZE];
	HANDLE hDevice = INVALID_HANDLE_VALUE;
	hmac_state mst;
	DWORD bytesRead = 0;
	DWORD bytesWritten = 0;
	char* device = 0;
	int deviceId = -1;
	char devicename[MAX_PATH];
	char hashfilename[MAX_PATH];
	char sigfilename[MAX_PATH];
	int partIndex = 0;
	int rv = 0;
	char* filename = 0;
	char strVersion[MAX_PATH];

	if (0 == GetVersionFromRC(strVersion, MAX_PATH))
	{
		printf("\nAristocrat Sign-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Sign-ali\n");
	}

	while (1) 
	{
		int option_index = 0;
		static struct option long_options[] = {
			{ "key",	required_argument,	0, 'k' },
			{ "partition",	required_argument,	0, 'p' },
			{ "disk",	required_argument,	0, 'd' },
			{ 0, 0, 0, 0 }
		};
		c = getopt_long(argc, argv, "k:p:d:y", long_options, &option_index);
		
		if (c == -1)
			break;
		
		switch (c) {
		case 0:
			printf("option %s", long_options[option_index].name);
			if (optarg)
				printf(" with arg %s", optarg);
			printf("\n");
			break;
		case 'd':
			device = optarg;
			break;
		case 'p':
			partIndex = atoi(optarg);
			break;
		case 'k':
			keyfile = (char*)optarg;
			break;
		
		case 'f':
			filename = (char*)optarg;
			break;
		case 'y':
			yestoall = 1;
			break;
		case '?':
		default:
			help(argv[0]);
			return EXIT_FAILURE;
			break;
		}
	}
	
	if (device == 0 || strlen(device) <= 0) return -2;
	if (::isdigit(device[1]))
	{
		deviceId = atoi(&device[1]);
		if (deviceId == 0)
		{
			printf("This is likely not the drive you wan't to touch, think again %d\n", deviceId);
			return 1;
		}

		sprintf(devicename, "\\\\.\\PhysicalDrive%d", deviceId);
		printf("You have selected Device %d = %s\n", deviceId, devicename);
		sprintf(hashfilename, "PhysicalDrive%d.hash", deviceId);
		sprintf(sigfilename, "PhysicalDrive%d.sig", deviceId);

	}
	else
	{
		strcpy(devicename, device);
		printf("You have selected file %s \n", devicename);
		sprintf(hashfilename, "%s.hash", devicename);
		sprintf(sigfilename, "%s.sig", devicename);
	}

	hDevice = CreateFile(devicename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
	if (hDevice == INVALID_HANDLE_VALUE)
	{
		printf("Access denied, failed to opening device %s\n", devicename);
		goto fail;
	}
	

	
	

	keylen = load_key(keyfile, keybuf, sizeof(keybuf));
	if (keylen < 0) {
		printf("Error loading DSA key '%s'\n", keyfile);
		goto fail;
	}

	if ((rv = dsa_import(keybuf, keylen, &dsakey_priv)) != CRYPT_OK) {
		printf("Error importing DSA key: %s\n", error_to_string(rv));
		goto fail;
	}

	
	
	uint32_t start = 0;
	uint32_t end = 0;
	uint64_t bytes = ((uint64_t)end - start) << SECTOR_BITS;

	if (deviceId != -1)
	{
		if ((partIndex > 4 || partIndex < 1))
		{
			printf("Partition index are 1 based, value is not in the range (1-4): specified %d\n", partIndex);
			printf("You have selected Device %d Partition %d\n", deviceId, partIndex);
		}
		
		--partIndex; /* sure */

		if (!ReadFile(hDevice, mbr, SECTOR_SIZE, &bytesRead, 0))
		{
			printf("Failed reading MBR from device %s\n", devicename);
			goto fail;
		}
		if (!ReadFile(hDevice, mbr, SECTOR_SIZE, &bytesRead, 0))
		{
			printf("Failed reading MBR from device %s\n", devicename);
			goto fail;
		}

		PartitionTable* ptables = (PartitionTable*)&mbr[0x1be];

		printf("Partition Table:\n");
		for (uint32_t i = 0; i < 4; ++i)
			printf("\t %cIndex :%lu \tSize: %lluMB \tStart: %lu  Ends: %lu %s\n", ptables[i].Status == 0x80 ? '*' : ' ',
				i,
				(((uint64_t)ptables[i].NumSectors) << SECTOR_BITS) / (MB),
				ptables[i].LBAStart,
				ptables[i].LBAStart + ptables[i].NumSectors,
				partIndex == i ? " <---" : "");

		if(!confirm("Are you sure you want to continue? [nothing will be written yet])")) goto fail;

		start = ptables[partIndex].LBAStart;
		end = ptables[partIndex].LBAStart + ptables[partIndex].NumSectors;
		bytes = ((uint64_t)end - start) << SECTOR_BITS;

		uint32_t next_start = end + 100;
		if (partIndex < 3)
		{
			next_start = ptables[partIndex + 1].LBAStart;
		}

		if ((next_start - start) <= 0)
		{
			printf("There is no partition gap to place the signature.\n");
			goto fail;
		}
	}
	else
	{
		LARGE_INTEGER size;
		GetFileSizeEx(hDevice, &size);
		if (size.QuadPart % SECTOR_SIZE != 0)
		{
			printf("input file %s is not a valid disk partition, must be sector aligned", devicename);
		}
		end = (uint32_t)(size.QuadPart >> SECTOR_BITS);
	}

	LARGE_INTEGER moveTo;
	LARGE_INTEGER currentPos;
	moveTo.QuadPart = ((uint64_t)start) << SECTOR_BITS;
	SetFilePointerEx(hDevice, moveTo, &currentPos, FILE_BEGIN);
	if (moveTo.QuadPart != currentPos.QuadPart)
	{
		printf("Failed to seek to partition start, Partition table must be wrong!\n");
		goto fail;
	}

	if ((rv = hmac_init(&mst, find_hash("sha1"), M7I_HMAC_KEY, M7I_HMAC_KEYLEN)) != CRYPT_OK)
	{
		printf("failed to initialize hmac hash.\n");
	}
	
	uint64_t current_sector = start;
	uint32_t read_size = BULK_SIZE;
	while (current_sector < end )
	{
		if (current_sector > (end - (BULK_SECTORS+1)))
			read_size = SECTOR_SIZE;
		if (!ReadFile(hDevice, block, read_size, &bytesRead, 0))
		{
			printf("Failed reading partition block from device %s\n", devicename);
			goto fail;
		}
		
		current_sector += bytesRead >> SECTOR_BITS;
		printf("Hashing Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - start), (uint64_t)(end - start), ((double)(current_sector - start)/(double)(end - start))*100);
		hmac_process(&mst, block, bytesRead);
	}
	hashlen = sizeof(gen_hash);
	if ((rv = hmac_done(&mst, (unsigned char*)gen_hash, &hashlen)) != CRYPT_OK)
	{
		printf("failed finalizing hash.\n");
		goto fail;
	}

	printf("Hash: ");
	for (unsigned long i = 0; i < hashlen; ++i)
	{
		printf("%02x", gen_hash[i]);
	}

	dsiglen = sizeof(dsig);
	if ((rv = dsa_sign_hash_custom(gen_hash, hashlen, dsig, &dsiglen, NULL, find_prng("sprng"), &dsakey_priv)) != CRYPT_OK) {
		printf("Error signing DSA chunk: %s\n", error_to_string(rv));
		goto fail;
	}

	printf("\nSig: ");
	for (unsigned long i = 0; i < dsiglen; ++i)
	{
		printf("%02x", dsig[i]);
	}

	

	memset(block, 0, SECTOR_SIZE);
	memcpy(block, dsig, dsiglen);
	printf("\nHash Completed.\n");
	if (deviceId != -1)
	{
		if (!confirm("do you wan't to write the signature at the end of partition?")) goto fail;
		if (!WriteFile(hDevice, block, SECTOR_SIZE, &bytesWritten, NULL))
		{
			printf("Failed writing signature to disk.\n");
			goto fail;
		}

		printf("Signature written.\n done.\n");
	}


	WriteBin(sigfilename, dsig, dsiglen);
	WriteBin(hashfilename, gen_hash, hashlen);

	

fail:
	CloseHandle(hDevice);
	delete[] mbr;
	delete[] block;
    return 0;
}

