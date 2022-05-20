// sign-ali.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <Windows.h>
#include <stdint.h>
#include <conio.h>
#include <getopt.h>
#include <functional>

extern "C"
{
	#include "m7i_crypt_support.h"
	#include "m7i_settings.h"
}

#define uint8_t byte;
const uint32_t MAX_DBUFFER = 1024 * 1024 * 100;
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

static bool log_enabled = false;
static char log_filename[MAX_PATH] = { 0 };

void LogMsg(const char* Format, ...)
{
	if (!log_enabled) return;

	FILE* logFile = NULL;
	va_list args;

	if (0 == fopen_s(&logFile, log_filename, "a+"))
	{
		va_start(args, Format);
		vfprintf_s(logFile, Format, args);
		fprintf_s(logFile, "\n");
		fflush(logFile);
		va_end(args);
	}

	if (logFile != NULL)
	{
		fclose(logFile);
		logFile = NULL;
	}
}

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

#define		SECTOR_BITS 9
#define		SECTOR_SIZE (1 << 9)
#define		BULK_SECTORS (1024*4)
#define		BULK_SIZE   (BULK_SECTORS << SECTOR_BITS)
#define		MB	1024*1024
#define		GB	1024*1024*1024

int WriteHex(const char* filename, const byte* hash, int hashlen)
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

int WriteAFile(const char* filename, const byte* data, int datalen)
{
	FILE* f = fopen(filename, "w");
	if (f == 0) return 1;
	int numel = fwrite(data, datalen, 1, f);
	fclose(f);
	return numel == 1;
}

bool MakeHex(char* hex, int maxlen, const char* source,int hashlen)
{
	if (maxlen < hashlen * 2 + 1) 
		return false;
	for (int i = 0; i < hashlen; ++i)
	{
		sprintf(&hex[i*2], "%02x", (unsigned char)source[i]);
	}

	hex[hashlen * 2] = 0;
	return true;
}

 static bool get_filename_from_path(wchar_t* filename, const wchar_t* filepath)
{
	size_t nlen = wcslen(filepath);
	size_t n = nlen;
	size_t i = 0;
	wchar_t *fn = filename;
	while (n--)
	{
		if (filepath[n] == L'\\') break;
	}

	for (i = n + 1; i < nlen; ++i)
	{
		*fn++ = filepath[i];
	}
	*fn++ = 0;
	return true;
}

static bool get_folder_from_path(char* folder, const char* filepath)
{
	size_t n = strlen(filepath);
	size_t i = 0;
	while (n--)
	{
		if (filepath[n] == '\\') break;
		if (filepath[n] == '//') break;
	}

	for (i = 0; i <= n; ++i)
	{
		folder[i] = filepath[i];
	}
	folder[i] = 0;
	return true;
}



int64_t get_file_size(FILE* f)
{
	long cur = fseek(f, 0, SEEK_CUR);
	fseek(f, 0, SEEK_END);
	int64_t length = _ftelli64(f);
	fseek(f, cur, SEEK_SET);
	return length;
}

int load_file(const char *file, char *data, unsigned long datalen) {
	FILE *inf;
	int len;

	// Load private key
	inf = fopen(file, "r");
	if (!inf)
		return -errno;
	len = fread(data, 1, datalen, inf);
	data[len] = 0;
	fclose(inf);
	return len;
}

inline char fromhex(char d)
{
	return (d >= '0' && d <= '9') ? d - '0' :
		(d >= 'a' && d <= 'f') ? d - 'a' + 10 :
		(d >= 'A' && d <= 'F') ? d - 'A' + 10 :
		0xff;	// Indicates parse error
}

inline int parsehex(unsigned char* dest, size_t max_bytes, const char* src)
{
	if (src == 0 || dest == 0) return 0;

	char* p = (char*)src;
	size_t i = 0;
	while (i < max_bytes)
	{
		unsigned char a = fromhex(*p++);
		unsigned char b = fromhex(*p++);
		if (a == 0xff || b == 0xff)
			return 0;
		dest[i++] = a << 4 | b;
	}
	return i;
}

struct DiskToolInfo
{
	int deviceid;
	HANDLE hDevice;
	char device_path[MAX_PATH];
	byte mbr[512];
	PartitionTable* PartitionTables;

	inline bool hasMBR(){return (mbr[511] == 0xAA && mbr[510] == 0x55);}

	bool SeekToSector(uint64_t pos)
	{
		LARGE_INTEGER moveTo;
		LARGE_INTEGER currentPos;
		moveTo.QuadPart = ((uint64_t)pos) << SECTOR_BITS;
		SetFilePointerEx(hDevice, moveTo, &currentPos, FILE_BEGIN);
		if (moveTo.QuadPart != currentPos.QuadPart)
		{
			printf("Failed to seek to partition start, Partition table must be wrong!\n");
			return false;
		}
		return true;
	}

	bool SeekTo(uint64_t pos)
	{
		LARGE_INTEGER moveTo;
		LARGE_INTEGER currentPos;
		moveTo.QuadPart = ((uint64_t)pos);
		SetFilePointerEx(hDevice, moveTo, &currentPos, FILE_BEGIN);
		if (moveTo.QuadPart != currentPos.QuadPart)
		{
			printf("Failed to seek to partition start, Partition table must be wrong!\n");
			return false;
		}
		return true;
	}

	static bool Open(const char* deviceorfile, DiskToolInfo* info)
	{
		if (info == NULL) return false;
		info->hDevice = CreateFile(deviceorfile, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
		if (info->hDevice == INVALID_HANDLE_VALUE)
		{
			return false;
		}
		memset(info->mbr, 0, sizeof(info->mbr));
		DWORD bytesRead = 0;
		ReadFile(info->hDevice, info->mbr, SECTOR_SIZE, &bytesRead, 0);
		info->SeekTo(0);
		info->PartitionTables = (PartitionTable*)&info->mbr[0x1be];
		return true;
	}
	void Close() { CloseHandle(hDevice); memset(this, 0, sizeof(this)); }
};

struct PartitionTask
{
	int source_partition;
	int target_partition;
	uint32_t offset;
	uint32_t length;
	uint64_t num_sectors;
	int ntfs_patch;
	byte sha1_hash[32];
	byte hmac_hash[32];
	dsa_key private_key;
	char* key_file;
	hash_state mst;
	hmac_state hmst;
	unsigned long hmac_len;
	char name[64];
	bool ignore_existing_vpart_table;
	bool begin()
	{
		hmac_len = sizeof(hmac_hash);
		if ((sha1_init(&mst)) != CRYPT_OK)
		{
			printf("failed to initialize sha1 hash.\n");
			return false;
		}

		if (hmac_init(&hmst, find_hash("sha1"), M7I_HMAC_KEY, M7I_HMAC_KEYLEN) != CRYPT_OK)
		{
			printf("failed to initialize hmac hash.\n");
			return false;
		}
		return true;
	}

	bool process(byte* data, size_t length)
	{
		if (sha1_process(&mst, data, length) == CRYPT_OK && hmac_process(&hmst, data, length) == CRYPT_OK)
			return true;

		return false;
	}

	bool end()
	{
		if ((sha1_done(&mst, (unsigned char*)sha1_hash)) != CRYPT_OK)
		{
			printf("failed finalizing hash.\n");
			return false;
		}

		hmac_len = sizeof(hmac_hash);
		if (hmac_done(&hmst, (unsigned char*)hmac_hash, &hmac_len) != CRYPT_OK)
		{
			printf("failed finalizing hmac hash.\n");
			return false;
		}
		return true;
	}

	void print(DiskToolInfo* info)
	{
		printf("Partition Table:\n");
		if (info->PartitionTables == NULL)
		{
			printf("No partitions found\n");
			return;
		}

		for (uint32_t i = 0; i < 4; ++i)
		{
			printf("\t %cIndex :%lu \tSize: %lluMB \tStart: %lu  Ends: %lu %s\n", info->PartitionTables[i].Status == 0x80 ? '*' : ' ',
				i,
				(((uint64_t)info->PartitionTables[i].NumSectors) << SECTOR_BITS) / (MB),
				info->PartitionTables[i].LBAStart,
				info->PartitionTables[i].LBAStart + info->PartitionTables[i].NumSectors,
				source_partition == i ? " <--- source" : (target_partition == i ? " <--- target" : ""));
		}
	}
};

bool confirm(const char* text,...)
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

int error(int err, const char* text, ...)
{
	char szBuffer[4096] = {};

	va_list argptr;
	va_start(argptr, text);
	if (-1 == (vsprintf(szBuffer, text, argptr)))
	{
		assert(false);
	}
	va_end(argptr);

	printf("%s\n", szBuffer);
	return err;
}

int ClonePartition(PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->source_partition <0) || (task->source_partition > 3)) return error(3, "no valid source partition provided");
	if ((task->target_partition <0) || (task->target_partition > 3)) return error(4, "no valid target partition provided");
	if (!confirm("Do you wan't to continue writing to partition %d to %d", task->source_partition,task->target_partition)) return false;

	int status = -1;
	byte* block = new byte[BULK_SIZE];
	DWORD bytesRead = 0;
	uint64_t start = info->PartitionTables[task->source_partition].LBAStart;
	uint64_t end = info->PartitionTables[task->source_partition].LBAStart + info->PartitionTables[task->source_partition].NumSectors;
	uint64_t bytes = ((uint64_t)end - start) << SECTOR_BITS;
	uint64_t num_sectors = info->PartitionTables[task->source_partition].NumSectors;

	uint64_t target_start = info->PartitionTables[task->target_partition].LBAStart;
	uint64_t target_end = info->PartitionTables[task->target_partition].LBAStart + info->PartitionTables[task->source_partition].NumSectors;
	uint64_t target_bytes = ((uint64_t)target_end - target_start) << SECTOR_BITS;

	if (target_bytes != bytes)
	{
		status = error(5, "partitions are of different sizes. aborting");
		goto fail;
	}

	uint64_t current_sector = 0;
	uint32_t read_size = BULK_SIZE;
	task->begin();
	while (current_sector < num_sectors)
	{
		if (read_size == BULK_SIZE && current_sector >(end - BULK_SECTORS))
			read_size = SECTOR_SIZE;

		memset(block, 0, BULK_SIZE);
		info->SeekToSector((start + current_sector));
		if (!ReadFile(info->hDevice, block, read_size, &bytesRead, 0))
		{
			printf("Failed reading sector from device %s\n", info->device_path);
			goto fail;
		}

		info->SeekToSector((target_start + current_sector));

		if (!WriteFile(info->hDevice, block, bytesRead, &bytesRead,0))
		{
			status = error(6, "write fail");
			goto fail;
		}

		current_sector += bytesRead >> SECTOR_BITS;
		printf("Cloning Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - num_sectors), (uint64_t)(num_sectors - start), ((double)(current_sector - start) / (double)(num_sectors - start)) * 100);

		task->process(block, bytesRead); 
	}

	task->end();
	char hashfile[MAX_PATH];
	sprintf(hashfile, "dev%d_partition%d.hash",info->deviceid,task->target_partition);
	WriteHex(hashfile, task->sha1_hash, 20);

	printf("\nHash for %llu -> %llu (0x%08llx): written to %s\n", start, end, (current_sector * 512), hashfile);
	status = 0;

fail:
	
	if (block) delete[] block;
	return status;
}

int ExtractPartition( const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if((task->source_partition <0) || (task->source_partition > 3)) return error(3, "no valid source partition provided");
	if (filename == NULL) return error(4, "no filename provided");
	if (!confirm("Do you wan't to continue extracting partition %d to %s", task->source_partition, filename)) return false;

	int status = -1;
	FILE* out_file = fopen(filename, "wb");

	if (out_file == 0) return error(5, "failed to create/open file %s", filename);

	byte* block = new byte[BULK_SIZE];
	DWORD bytesRead = 0;
	uint64_t start = info->PartitionTables[task->source_partition].LBAStart;
	uint64_t end = info->PartitionTables[task->source_partition].LBAStart + (uint64_t)info->PartitionTables[task->source_partition].NumSectors;
	uint64_t bytes = ((uint64_t)end - start) << SECTOR_BITS;
	uint64_t num_sectors = end - start;

	info->SeekToSector(start);

	uint64_t current_sector = start;
	uint32_t read_size = BULK_SIZE;
	task->begin();

	while (current_sector < end)
	{
		if (read_size == BULK_SIZE && current_sector > (end - BULK_SECTORS))
			read_size = SECTOR_SIZE;

		memset(block, 0, BULK_SIZE);
		if (!ReadFile(info->hDevice, block, read_size, &bytesRead, 0))
		{
			printf("Failed reading sector from device %s\n", info->device_path);
			goto fail;
		}

		if (bytesRead < read_size) /* pad with zeroes */
		{
			memset(&block[bytesRead], 0, SECTOR_SIZE - bytesRead);
			bytesRead = SECTOR_SIZE;
		}

		if (current_sector == start)
		{
			block[0] = 'A';
			block[1] = 'L';
			block[2] = 'I';
			block[3] = 'P';
			block[4] = 'A';
			block[5] = 'R';
			block[6] = 'T';
		}
		if (fwrite(block, bytesRead, 1, out_file) != 1)
		{
			status = error(6, "write fail");
			goto fail;
		}
		task->process(block, bytesRead);
		current_sector += bytesRead >> SECTOR_BITS;
		printf("Extracting Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - start), (uint64_t)(end - start), ((double)(current_sector - start) / (double)(end - start)) * 100);
	}
	task->end();
	fclose(out_file);
	char hashfile[MAX_PATH];
	sprintf(hashfile, "%s.hash", filename);
	
	WriteHex(hashfile, task->sha1_hash, 20);

	printf("\nHash for %llu -> %llu (0x%08llx): written to %s\n", start, end, (current_sector * 512), hashfile);

	/* write partition json file */
	char partfile[MAX_PATH];
	char hexhmac[50];
	char hexsig[90];
	hexsig[0] = 0;

	sprintf(partfile, "%s.part", filename);

	/* generate hmac hex string */

	MakeHex(hexhmac, sizeof(hexhmac), (const char*)task->hmac_hash, 20);

	/* generate signature if key is present */
	if (task->key_file)
	{
		int rv = 0;
		unsigned char	 dsig[2048];
		unsigned long	 dsiglen;
		dsiglen = sizeof(dsig);
		if ((rv = dsa_sign_hash_custom(task->hmac_hash,task->hmac_len, dsig, &dsiglen, NULL, find_prng("sprng"), &task->private_key)) != CRYPT_OK) {
			printf("Error signing DSA chunk: %s\n", error_to_string(rv));
			goto fail;
		}
		/* generate signature hex string */
		MakeHex(hexsig, sizeof(hexsig), (const char*)dsig, dsiglen);
	}

	char partcontent[1024];
	sprintf(partcontent, "{name:\"%s\", source_partition:%d, source_offset:%llu, target_partition:%d, size:%llu, file:%s, hash:\"%s\", sig:\"%s\"}", task->name, 0, 0ull, task->source_partition + 1, num_sectors, filename, hexhmac, hexsig);
	WriteAFile(partfile, (const byte*)partcontent, strlen(partcontent));
	status = 0;

fail:
	if(out_file) fclose(out_file);
	if(block) delete[] block;
	return status;
}

int WritePartition( const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->target_partition <0) || (task->target_partition > 3)) return error(3, "no valid target partition provided");
	if (filename == NULL) return error(4, "no filename provided");
	if (!confirm("Do you wan't to continue writing to partition %d to %s", task->target_partition, filename)) return false;

	int status = -1;
	byte* block = NULL;
	FILE* in_file = fopen(filename, "rb");

	if (in_file == 0) return error(5, "failed to open file %s", filename);

	int64_t file_size = get_file_size(in_file);
	if (file_size % SECTOR_SIZE != 0)
	{
		status = error(6, "input file %s must be of sector size", filename);
		goto fail;
	}

	block = new byte[BULK_SIZE];
	DWORD bytesRead = 0;
	uint64_t start = info->PartitionTables[task->target_partition].LBAStart;
	uint64_t end = info->PartitionTables[task->target_partition].LBAStart + info->PartitionTables[task->target_partition].NumSectors;
	uint64_t bytes = ((uint64_t)end - start) << SECTOR_BITS;

	info->SeekToSector(start);

	uint64_t current_sector = start;
	uint32_t read_size = BULK_SIZE;

	if (file_size != bytes)
	{
		status = error(7, "the size of %s does not match partition size", filename);
		goto fail;
	}

	task->begin();
	while (current_sector < end)
	{
		if (read_size == BULK_SIZE && current_sector >(end - BULK_SECTORS))
			read_size = SECTOR_SIZE;
		memset(block, 0, BULK_SIZE);

		bytesRead = fread(block, 1, BULK_SIZE, in_file);
		
		task->process(block, bytesRead);

		if (current_sector == start && task->ntfs_patch)
		{
			block[0] = 0xEB; // e
			block[1] = 0x52; // R
			block[2] = 0x90; // 
			block[3] = 0x4E; // N
			block[4] = 0x54; // T
			block[5] = 0x46; // F
			block[6] = 0x53; // S
		}
		if (!WriteFile(info->hDevice, block, bytesRead, &bytesRead, 0))
		{
			printf("Failed patching ntfs header on device %s at sector %llu\n", info->device_path, current_sector);
			goto fail;
		}

		if (bytesRead < read_size) /* pad with zeroes */
		{
			memset(&block[bytesRead], 0, SECTOR_SIZE - bytesRead);
			bytesRead = SECTOR_SIZE;
		}

		current_sector += bytesRead >> SECTOR_BITS;
		printf("Writing Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - start), (uint64_t)(end - start), ((double)(current_sector - start) / (double)(end - start)) * 100);
	}
	task->end();
	fclose(in_file);
	char hashfile[MAX_PATH];
	sprintf(hashfile, "%s.hash", filename);

	WriteHex(hashfile, task->sha1_hash, 20);

	printf("\nHash for %llu -> %llu (0x%08llx): written to %s\n", start, end, (current_sector * 512), hashfile);
	status = 0;

fail:
	if (in_file) fclose(in_file);
	if (block) delete[] block;
	return status;
}

inline char* ali_stranyof(char* str, char* dels)
{
	char *s = str;
	char *d = 0;
	char *dn = 0;
	while (*dels)
	{
		if (*dels == 0) break;
		dn = strchr(s, *dels);
		if ((dn != 0 && dn < d) || d == 0)
			d = dn;
		++dels;
	}
	return d;
}

/* tiny json parser */
char* parseJson(const char* root, char* data, std::function<bool( const char* root, const char* name, const char* value, int index)> parseItem)
{
	char* s = data;
	char* e = &data[strlen(data)];
	char name[64];
	char val[512];
	strcpy(name, "noname");
	strcpy(val, "");
	int arrindex = 0;
	char* scope = 0;
	bool arr = false;
	char* n = s;
	while (s < e)
	{
		while (*s == ' ')++s;
		if (*s == '\"') { scope = s; }

		if (scope)
			n = ali_stranyof(scope+1, "\"");
		else
			n = ali_stranyof(s, "{:,[]}");

		if (n == 0) 
			n = e;
		switch (*n)
		{
		case '{':
			n = parseJson(name, ++n, parseItem);
				break;
		case ']':
			arrindex = 0;
			arr = false;
			break;
		case '[':
			arr = true;
			break;
		case '}':
			return ++n;
			break;
		case ':':
			strncpy(name, s, n - s);
			name[n - s] = 0;
			break;
		case '\"':
			if (scope == 0)
			{
				scope = n;
				continue;
			}
			else
			{
				scope = 0;
			}

			strncpy(val, s + 1, n  - s);
			val[n -1 - s] = 0;
			break;
		case ',':
			strncpy(val, s, n - s);
			val[n - s] = 0;
			if (arr) ++arrindex;
			break;
		}
		if (parseItem && val[0] != 0 && name[0] != 0)
		{
			parseItem(root, name, val, arrindex);
			val[0] = 0;
			name[0] = 0;
		}
		s = n+1;
	}
	return s;
}

int MakeDisk(const char** filenames, PartitionTask* task, DiskToolInfo* info)
{
	LogMsg("MakeDisk start");
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->target_partition <0) || (task->target_partition > 3)) return error(3, "no valid target partition provided");
	if (filenames[0] == NULL) return error(4, "no filename provided");
	if (!confirm("Do you wan't to continue writing %s to partition %d", filenames[0], task->target_partition)) return 4;

	int status = -1;
	byte* block = NULL;
	FILE* in_file = NULL;
	DWORD bytesWritten = 0;
	DWORD bytesRead = 0;
#pragma pack(1) 
	struct disk_part
	{
		enum vpart_flags
		{
			VPART_INACTIVE = 0,
			VPART_ACTIVE = 0x1,
			VPART_OLDACTIVE = 0x101,
		};
		union
		{
			struct {
				char magic[4];
				char name[64];
				unsigned char hash[20];
				unsigned char sig[40];
				unsigned int source_partition;
				uint64_t source_offset;
				unsigned int target_partition;
				uint64_t size;
				char filename[64];
				unsigned int flags; // 0x1 =  active
				// add more data here
			};
			char sector[512];
		};
		char part_file[MAX_PATH];
		static void read(disk_part* part, const char* filename)
		{
			char* buffer = new char[4096];
			load_file(filename, buffer, 4096);
			strcpy(part->part_file, filename);
			parseJson("root",buffer, [part](const char* root, const char* name, const char* value, int index) 
			{
				if (strcmp(name, "name") == 0) strcpy(part->name, value);
				if (strcmp(name, "file") == 0) strcpy(part->filename, value);
				if (strcmp(name, "source_partition") == 0) part->source_partition = strtoul(value,0, 10);
				if (strcmp(name, "source_offset") == 0) part->source_offset = strtoull(value, 0, 10);
				if (strcmp(name, "target_partition") == 0) part->target_partition = strtoul(value, 0, 10);
				if (strcmp(name, "size") == 0) part->size = strtoul(value, 0, 10);
				if (strcmp(name, "hash") == 0) parsehex(part->hash,40, value);
				if (strcmp(name, "sig") == 0) parsehex(part->sig, 80, value);
				return true;
			});

			delete[] buffer;
		}
		static void print(disk_part* part)
		{
			printf("magic: %c%c%c%c\n",part->magic[0], part->magic[1], part->magic[2], part->magic[3]);
			printf("name: %s\n",part->name);
			printf("hash: %s\n",part->hash);
			printf("sig: %s\n",part->sig);
			printf("source_partition: %d\n",part->source_partition);
			printf("offset: %llu sector\n",part->source_offset);
			printf("target_partition: %d\n",part->target_partition);
			printf("size: %llu sector\n",part->size);
			printf("name: %s\n",part->filename);
			printf("flags: %x\n",part->flags);
			printf("flags: %x\n",part->flags);
		}
	};
#pragma pack()
const int NUM_VPARTS_SLOTS = 4;
	assert(sizeof(disk_part::sector) == 512);
	disk_part parts[NUM_VPARTS_SLOTS];
	memset(&parts[0], 0, sizeof(parts));
	disk_part in_parts[NUM_VPARTS_SLOTS];
	memset(&in_parts[0], 0, sizeof(parts));
	uint64_t partition_start = info->PartitionTables[task->target_partition].LBAStart;
	uint64_t partition_end = info->PartitionTables[task->target_partition].LBAStart + info->PartitionTables[task->target_partition].NumSectors;
	uint64_t partition_bytes = ((uint64_t)partition_end - partition_start) << SECTOR_BITS;
	uint64_t partition_size = ((uint64_t)partition_end - partition_start);

	// Loadup existing vpart if any
	int num_existing_parts = 0;
	if(!task->ignore_existing_vpart_table)
	{
		info->SeekToSector(partition_end);
		for (int i = 0; i < NUM_VPARTS_SLOTS; ++i)
		{
			if (!ReadFile(info->hDevice, &parts[i].sector, sizeof(disk_part::sector), &bytesRead, 0))
			{
				status = error(5, "Failed reading virtual partition table index: %d at %llu\n", i, partition_end + i * sizeof(disk_part::sector));
				LogMsg("MakeDisk: ERROR (5): Failed reading virtual partition table index: %d at %llu", i, partition_end + i * sizeof(disk_part::sector));
				goto fail;
			}
			else
			{
				if(parts[i].magic[0] == 'V' && 
					parts[i].magic[1] == 'P' &&
					parts[i].magic[2] == 'R' &&
					parts[i].magic[3] == 'T')
					{
						++num_existing_parts;
						if((parts[i].flags & disk_part::VPART_ACTIVE) == disk_part::VPART_ACTIVE)
						{
							parts[i].flags = disk_part::VPART_OLDACTIVE;
						}
					}
					else
					{
						int remaining_slots = (NUM_VPARTS_SLOTS-i);
						memset(&parts[i], 0, sizeof(disk_part)*remaining_slots); // clear rest
						break; // halt when the sequence of parts are not valid
					}
			}
		}
	}

	// Read part files from disk and populate in going vpartitions.
	int part_index = 0;
	while (*filenames)
	{
		in_parts[part_index].magic[0] = 'V';
		in_parts[part_index].magic[1] = 'P';
		in_parts[part_index].magic[2] = 'R';
		in_parts[part_index].magic[3] = 'T';
		disk_part::read(&in_parts[part_index],*filenames++);
		in_parts[part_index].source_partition = task->target_partition; /* where we write the source partition */
		part_index++;
	}

	int num_new_parts = part_index;
	int num_parts = num_existing_parts + num_new_parts;
	if(num_parts > NUM_VPARTS_SLOTS) 
		num_parts = NUM_VPARTS_SLOTS;

	uint64_t total_sectors = 0;
	uint64_t offset = partition_start;

	int found_parts = 0;
	bool write_slot = false;
	// find inactive partitions that matches number of sectors of incoming partitions
	for( int i = 0; i < num_new_parts; ++i )
	{
		disk_part* src_part = &in_parts[i];
		offset = partition_start;
		for(int o = 0; o < NUM_VPARTS_SLOTS;++o)
		{
			disk_part* dst_part = &parts[o];
			if( dst_part->magic[0] == 'V' &&  dst_part->magic[1] == 'P' && dst_part->magic[2] == 'R' && dst_part->magic[3] == 'T' )
			{
				if( (dst_part->flags & disk_part::VPART_ACTIVE) == 0 ) // inactive or empty slot
				{
					if ( dst_part->size == src_part->size || // matching incoming size? 
						dst_part->size == 0 ) // empty
					
					{
						src_part->flags = disk_part::VPART_ACTIVE;  // activate new partitions
						memcpy(dst_part, src_part,sizeof(disk_part));
						dst_part->source_offset = offset; /* where we write the source partition */
						++found_parts;
						break;
					}
				}
			}
			else // new empty slot
			{
				src_part->flags = disk_part::VPART_ACTIVE;  // activate new partitions
				memcpy(dst_part, src_part,sizeof(disk_part));
				dst_part->source_offset = offset; /* where we write the source partition */
				++found_parts;
				break;
			}

			offset += dst_part->size;
			
		}
	}

	// revoke old active
	for(int i = 0; i < NUM_VPARTS_SLOTS;++i)
	{
		if((parts[i].flags & disk_part::VPART_OLDACTIVE) == disk_part::VPART_OLDACTIVE)
		{
			parts[i].flags = disk_part::VPART_INACTIVE;
		}
		total_sectors += parts[i].size;
	}

	if(found_parts < num_new_parts)
	{
		status = error(6, "Could not find empty or inactive slots in virtual partition table that matches incoming parts.%s: %llu < %llu\n", info->device_path, partition_size, total_sectors);
        LogMsg("MakeDisk: ERROR (6): Could not find empty or inactive slots in virtual partition table that matches incoming parts.%s: %llu < %llu", info->device_path, partition_size, total_sectors);
		printf("In Virtual Partition:\n");
		for(int i = 0; i < num_new_parts;++i)
		{
			disk_part::print(&in_parts[i]);
			printf("\n");
		}

		printf("Disk Virtual Partition:\n");
		for(int i = 0; i < num_new_parts;++i)
		{
			disk_part::print(&parts[i]);
			printf("\n");
		}

		goto fail;
	}
	if (partition_size < total_sectors)
	{
		status = error(7, "Partition not big enough %s: %llu < %llu\n", info->device_path, partition_size, total_sectors);
        LogMsg("MakeDisk: ERROR (7): Partition not big enough %s: %llu < %llu", info->device_path, partition_size, total_sectors);
		goto fail;
	}

	uint64_t next_start = partition_end + 10;
	if (task->target_partition < 3)
	{
		next_start = info->PartitionTables[task->target_partition + 1].LBAStart;
	}

	if ((next_start - partition_start) <= 0)
	{
		status = error(8, "There is no partition gap to place the partition data table.\n");
        LogMsg("MakeDisk: ERROR (8): There is no partition gap to place the partition data table.");
		goto fail;
	}

	for (int i = 0; i < num_parts; ++i)
	{
		if((parts[i].flags & disk_part::VPART_ACTIVE) != disk_part::VPART_ACTIVE)
			continue;

		block = NULL;
		in_file = fopen(parts[i].filename, "rb");

		if (in_file == 0)
		{   // try relative to input file
			char full_path[MAX_PATH*2];
			get_folder_from_path(full_path, parts[i].part_file);
			strcat(full_path, parts[i].filename);
			in_file = fopen(full_path, "rb");
			if (in_file == 0)
			{
				status = error(9, "failed to open file %s\n", full_path);
                LogMsg("MakeDisk: ERROR (9): failed to open file %s", full_path);
				goto fail;
			}
		}

		int64_t file_size = get_file_size(in_file);
		if (file_size % SECTOR_SIZE != 0)
		{
			status = error(10, "input file %s must be of sector size\n", parts[i].filename);
            LogMsg("MakeDisk: ERROR (10): input file %s must be of sector size", parts[i].filename);
			goto fail;
		}

		block = new byte[BULK_SIZE];
		
		uint64_t start = parts[i].source_offset;
		uint64_t end = start + parts[i].size;
		uint64_t bytes = (parts[i].size) << SECTOR_BITS;

		info->SeekToSector(start);

		uint64_t current_sector = start;
		uint32_t read_size = BULK_SIZE;

		if (file_size != bytes)
		{
			status = error(11, "the size of %s does not match partition size\n", parts[i].filename);
            LogMsg("MakeDisk: ERROR (11): the size of %s does not match partition size", parts[i].filename);
			goto fail;
		}

		printf("Writing virtual partition %s to sector %llu\n", parts[i].filename, start);
        LogMsg("MakeDisk: Writing virtual partition %s to sector %llu", parts[i].filename, start);

		while (current_sector < end)
		{
			if (read_size == BULK_SIZE && current_sector > (end - BULK_SECTORS))
				read_size = SECTOR_SIZE;
			memset(block, 0, BULK_SIZE);

			bytesRead = fread(block, 1, BULK_SIZE, in_file);

			if (!WriteFile(info->hDevice, block, bytesRead, &bytesWritten, 0))
			{
				status = error(12, "Failed writing to device %s at sector %llu\n", info->device_path, current_sector);
                LogMsg("MakeDisk: ERROR (12): Failed writing to device %s at sector %llu", info->device_path, current_sector);
				goto fail;
			}

			if (bytesRead < read_size) /* pad with zeroes */
			{
				memset(&block[bytesRead], 0, SECTOR_SIZE - bytesRead);
				bytesRead = SECTOR_SIZE;
			}

			current_sector += bytesRead >> SECTOR_BITS;
			printf("Writing Sector %llu of %llu (%.2f%%)\r             ", (uint64_t)(current_sector - start), (uint64_t)(end - start), ((double)(current_sector - start) / (double)(end - start)) * 100);
		}
		
		fclose(in_file);
		printf("\n");
        LogMsg("MakeDisk: Finished writing virtual partition %s to sector %llu", parts[i].filename, start);
	}

	/* write aristocrat partition table after the partition*/
	info->SeekToSector(partition_end);
	for (int i = 0; i < NUM_VPARTS_SLOTS; ++i)
	{
		if (!WriteFile(info->hDevice, &parts[i].sector, sizeof(disk_part::sector), &bytesWritten, 0))
		{
            status = error(13, "Failed writing partition table index: %d at %llu\n", i, partition_end + i * sizeof(disk_part::sector));
            LogMsg("MakeDisk: ERROR (13): Failed writing partition table index: %d at %llu", i, partition_end + i * sizeof(disk_part::sector));
			goto fail;
		}
	}
	status = 0;

fail:
	if (in_file) fclose(in_file);
	if (block) delete[] block;

	LogMsg("MakeDisk finish");
	return status;
}

int DiskWrite(const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->offset < 0)) return error(3, "no valid offset/length");
	if (filename == NULL) return error(4, "no filename provided");
	
	if (!confirm("Do you wan't to continue writing %s to offset %d", filename, task->offset)) return false;
	DWORD bytes_read = 0;
	int status = -1;
	byte* block = NULL;
	FILE* in_file = fopen(filename, "rb");

	if (in_file == 0) return error(5, "failed to open file %s", filename);

	int64_t file_size = get_file_size(in_file);
	if (file_size > task->length)
		file_size = task->length;
	if (file_size > MAX_DBUFFER)
	{
		printf("too large buffer, max buffer is %dmb\n", MAX_DBUFFER / (1024 * 1024));
		goto fail;
	}
	int32_t block_size = (int32_t)(file_size + (SECTOR_SIZE - (file_size % SECTOR_SIZE)) + SECTOR_SIZE);

	uint64_t start_sect_aligned = task->offset / SECTOR_SIZE;
	uint64_t offset_to_aligned = task->offset % SECTOR_SIZE;

	block = new byte[block_size];
	memset(block, 0, block_size);
	byte* block_start = &block[offset_to_aligned];
	info->SeekTo(start_sect_aligned);

	DWORD bytesRead = 0;
	
	info->SeekToSector(start_sect_aligned);
	if (!ReadFile(info->hDevice, block, SECTOR_SIZE, &bytes_read, NULL))
	{
		status = error(6, "failed reading from disk");
		goto fail;
	}

	uint64_t last_sector = start_sect_aligned + block_size - SECTOR_SIZE;
	if (last_sector != start_sect_aligned)
	{
		info->SeekTo(last_sector);
		if (!ReadFile(info->hDevice, &block[block_size - SECTOR_SIZE], SECTOR_SIZE, &bytes_read, NULL))
		{
			status = error(7, "failed reading from disk");
			goto fail;
		}
	}

	if (fread(block_start, (uint32_t)file_size, 1, in_file) != 1)
	{
		printf("failed reading %s.", filename);
		goto fail;
	}

	info->SeekTo(start_sect_aligned);

	if (!WriteFile(info->hDevice, block, block_size, &bytesRead, 0))
	{
		printf("Failed writing sector to device %s at sector location %llu size: %d\n", info->device_path, start_sect_aligned, block_size);
		goto fail;
	}

	fclose(in_file);
	status = 0;

fail:
	if (in_file) fclose(in_file);
	if (block) delete[] block;
	return status;
}

int DiskRead(const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->offset < 0) || task->length <=0) return error(3, "no valid offset/length");
	if (filename == NULL) return error(4, "no filename provided");
	int status = -1;
	byte* block = NULL;
	FILE* out_file = fopen(filename, "wb");
	if (out_file == 0) return error(5, "failed to open file %s", filename);
	if (task->length > MAX_DBUFFER)
	{
		printf("too large buffer, max buffer is %dmb\n", MAX_DBUFFER / (1024 * 1024));
		goto fail;
	}

	DWORD bytes_read = 0;

	uint64_t start_sect_aligned = task->offset / SECTOR_SIZE;
	uint64_t offset_to_aligned = task->offset % SECTOR_SIZE;

	uint32_t file_size = task->length;
	int32_t block_size = (int32_t)(file_size + (SECTOR_SIZE - (file_size % SECTOR_SIZE)) + SECTOR_SIZE);

	block = new byte[block_size];
	
	info->SeekTo(start_sect_aligned);
	if (!ReadFile(info->hDevice, block, block_size, &bytes_read, NULL))
	{
		status = error(6, "failed reading from disk");
		goto fail;
	}
	fwrite(&block[offset_to_aligned], file_size, 1, out_file);
	fclose(out_file);
	status = 0;

fail:
	if (out_file) fclose(out_file);
	if (block) delete[] block;
	return status;
}

int WriteSignature(const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->target_partition <0) || (task->target_partition > 3)) return error(3, "no valid target partition provided, provide -t ");
	if (filename == NULL) return error(4, "no filename provided");

	if (!confirm("Do you wan't to continue writing signature %s to end of partition %d", filename, task->target_partition)) return false;

	byte block[512];
	memset(block, 0, 512);
	int status = -1;
	FILE* in_file = fopen(filename, "rb");

	if (in_file == 0) return error(5, "failed to open file %s", filename);

	if (fread(block, 1, 512, in_file) < 20)
	{
		return error(6, "failed reading signature file");
		fclose(in_file);
	}

	DWORD bytesWritten = 0;
	uint64_t start = info->PartitionTables[task->target_partition].LBAStart;
	uint64_t end = info->PartitionTables[task->target_partition].LBAStart + info->PartitionTables[task->target_partition].NumSectors;

	info->SeekToSector(end);

	if (!WriteFile(info->hDevice, block, 512, &bytesWritten, 0))
	{
		printf("Failed writing sector on device %s\n", info->device_path);
		goto fail;
	}

	fclose(in_file);

	printf("Signature written.");
	status = 0;

fail:
	if (in_file) fclose(in_file);
	return status;
}

int WriteBootloader(const char* filename, PartitionTask* task, DiskToolInfo* info)
{
	if (task == NULL || info == NULL) return error(2, "task or info parameters are invalid");
	if ((task->target_partition > 3)) return error(3, "no valid target partition provided");
	if (filename == NULL) return error(4, "no filename provided");

	if (task->target_partition > 0)
	{
		if (!confirm("Do you wan't to continue writing to signature %s to the start of partition %d", task->target_partition, filename)) return false;
	}
	else
	{
		if (!confirm("Do you wan't to continue writing to signature %s to the selected disk (mbr).", filename)) return false;
	}

	int status = -1;
	FILE* in_file = fopen(filename, "rb");

	if (in_file == 0) return error(5, "failed to open file %s", filename);

	uint64_t file_size = get_file_size(in_file);
	size_t block_size = 1024 * 1024;
	byte* block = new byte[block_size]; /* 1mb */

	memset(block, 0, block_size);

	if (fread(block, (size_t)file_size, 1, in_file) != 1)
	{
		return error(6, "failed reading bootloader file");
		fclose(in_file);
	}

	DWORD bytesWritten = 0;
	uint64_t start = 0;
	if (task->target_partition > 0) /* write to partition*/
	{
		// patch grub for chainload
		start = info->PartitionTables[task->target_partition].LBAStart;
		(*(unsigned int*)&block[0x5C]) = (uint32_t)start+1;		// grub-core\boot\i386\pc\boot.S: GRUB_BOOT_MACHINE_KERNEL_SECTOR offset
		(*(unsigned int*)&block[0x3F4]) = (uint32_t)start+2;		// grub-core\boot\i386\pc\diskboot.S blocklist_default_start: offset
	}
	else /* write to mbr */
	{
		/* patch partition table from existing mbr */
		memcpy(&block[0x1be], &info->mbr[0x1be], sizeof(PartitionTable) * 4);
	}

	info->SeekToSector(start);

	if (!WriteFile(info->hDevice, block, block_size, &bytesWritten, 0))
	{
		printf("Failed reading sector from device %s\n", info->device_path);
		goto fail;
	}

	fclose(in_file);
	status = 0;

fail:
	if (in_file) fclose(in_file);
	return status;
}

enum op_index
{
	OP_WRITE = 0,
	OP_EXTRACT,
	OP_CLONE,
	OP_WRITESIG,
	OP_WRITEBOOT,
	OP_DREAD,
	OP_DWRITE,
	OP_MAKEDISK,
	OP_UNKNOWN
};
char* operations[] = { "write","extract","clone","writesig","boot", "dread", "dwrite","make", 0 };

int getoperation(const char* str)
{
	if (str == NULL) return OP_UNKNOWN;
	char** op = operations;
	while (*op)
	{
		if (_stricmp(*op, str) == 0) break;
		++op;
	}

	int index = op - operations;
	return index;
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

int help(char *exe, int reval) {
	printf("\nUsage: %s [OPTIONS]\n", exe);
	printf("Writes to file or drive \n");
	printf("\n");
	printf("Options:\n");
	printf("  -p, --partition                       Partition (0=root, 1-4)\n");
	printf("  -t, --target     [idx]                Target Partition (0=root, 1-4)\n");
	printf("  -d, --disk       [id/file]            Disk/device id\n");
	printf("  -x, --offset     [bytes position]     Offset in disk\n");
	printf("  -n, --ntfs                            Patch ntfs\n");
	printf("  -k, --key                             Key (extracts will be signed)\n");
	printf("  -z, --name                            Name partition when extracting\n");
	printf("  -l, --length     [num bytes]          Number of bytes to read\n");
	printf("  -o, --operation  [op]                 op =[");
	char** op = operations;
	while (*op) { printf("%s,", *op); ++op; }
	printf("none]\n");
	printf("  -f, --file                            Bsource/target file for partition write/extract\n");
	printf("  -y, --yes                             'yes' to all questions.\n");
	printf("  -g, --log        [filename]           Creates a log file at the specified location.\n");
	printf("\nOutputs to drive\n");
	return reval;
}

int main(int argc, char** argv)
{
	CHAR strVersion[MAX_PATH];

	if (GetVersionFromRC(strVersion, MAX_PATH))
	{
		printf("\nAristocrat Disktool-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Disktool-ali\n");
	}

	register_hash(&sha1_desc);
	register_hash(&sha256_desc);
	register_prng(&sprng_desc);
	ltc_mp = ltm_desc;

	char devicename[MAX_PATH];
	byte* mbr = new byte[SECTOR_SIZE];
	memset(devicename, 0, MAX_PATH);
	memset(mbr, 0, SECTOR_SIZE);
	
	const char* bootloaderhash_filename = "bootloader.hash";
	const char* mbrhash_filename = "mbr.hash";

	HANDLE hDevice = INVALID_HANDLE_VALUE;
	
	DWORD bytesRead = 0;
	DWORD bytesWritten = 0;
	int deviceId = -1;
	int partIndex = -1;
	int targetIndex = -1;
	char* file_names[64];
	int num_files = 0;
	char* operation = 0;
	int offset = 0;
	int length = 0;
	int status = -1;
	int ntfs = 0;
	bool ignore_vpart = false;
	char* name = 0;
	char *key_file = 0;
	char c;
	while (1)
	{
		int option_index = 0;
		static struct option long_options[] = {
			{ "key",            optional_argument,	0, 'k' },
			{ "partition",      required_argument,	0, 'p' },
			{ "disk",           required_argument,	0, 'd' },
			{ "target",         optional_argument,	0, 't' },
			{ "operation",      required_argument,	0, 'o' },
			{ "file",           optional_argument,	0, 'f' },
			{ "log",            optional_argument,	0, 'g' },
			{ "yes",            optional_argument,	0, 'y' },
			{ "offset",         optional_argument,	0, 'x' },
			{ "length",         optional_argument,	0, 'l' },
			{ "ntfs",           optional_argument,	0, 'n' },
			{ "ignorevpart",    optional_argument,	0, 'i' },
			{ "name",           optional_argument,	0, 'z' },

			{ 0, 0, 0, 0 }
		};
		c = getopt_long(argc, argv, "z:k:p:d:t:o:f:g:x:l:iyn", long_options, &option_index);

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
			if (!::isdigit(*optarg)) return help (argv[0],-1);
			deviceId = atoi(optarg);
			break;
		case 'p':
			if (!::isdigit(*optarg)) return help(argv[0], -1);
			partIndex = atoi(optarg) -1;
			break;
		case 't':
			if (!::isdigit(*optarg)) return help(argv[0], -1);
			targetIndex = atoi(optarg) - 1;
			break;
		case 'f':
			file_names[num_files] = optarg;
			if (file_names[num_files]) printf("file : %s\n", file_names[num_files]);
			++num_files;
			file_names[num_files] = 0;
			break;
		case 'o':
			operation = optarg;
			break;
		case 'k':
			key_file = optarg;
			if (key_file) printf("key file : %s\n", key_file);
			break;
		case 'z':
			name = optarg;
			break;
		case 'x':
			offset = strtoul(optarg,NULL,10);
			if (optarg) printf("offset : %d\n", offset);
			break;
		case 'l':
			length = strtoul(optarg, NULL, 10);
			if (optarg) printf("length: %d\n", length);
			break;
		case 'y':
			yestoall = 1;
			break;
		case 'i':
			ignore_vpart = 1;
			break;
		case 'n':
			ntfs = 1;
			break;
		case 'g':
			if (optarg != NULL)
			{
				log_enabled = true;
				strcpy_s(log_filename, optarg);
			}
			if (log_filename) printf("log file : %s\n", log_filename);
			LogMsg("-- disktool-ali.exe started --");
			break;
		case '?':
		default:
			return help(argv[0], EXIT_FAILURE);
		}
	}

	if (deviceId != -1)
	{
		sprintf(devicename, "\\\\.\\PhysicalDrive%d", deviceId);
		printf("You have selected Device %d = %s\n", deviceId, devicename);
		LogMsg("main: You have selected Device %d = %s", deviceId, devicename);
	}
	else if(strlen(devicename) >1)
	{
		printf("You have selected file %s\n", devicename);
		LogMsg("main: You have selected file %s", devicename);
	}
	else
	{
		return help(argv[0], EXIT_FAILURE);
	}

	PartitionTask task;
	task.ignore_existing_vpart_table = ignore_vpart;
	task.source_partition = partIndex;
	task.target_partition = targetIndex;
	task.offset = offset;
	task.length = length;
	task.ntfs_patch = ntfs;
	strcpy(task.name, name==0?"noname":name);
	task.key_file = key_file;
	if (task.key_file)
	{
		int rv = 0;
		unsigned char* keybuf = new unsigned char[4096];
		int keylen = load_key(task.key_file, keybuf, 4096);
		if (keylen < 0) {
			printf("Error loading DSA key '%s'\n", task.key_file);
			LogMsg("main: ERROR: loading DSA key '%s'", task.key_file);
			delete[] keybuf;
			goto fail;
		}

		if ((rv = dsa_import(keybuf, keylen, &task.private_key)) != CRYPT_OK) {
			printf("Error importing DSA key: %s\n", error_to_string(rv));
			LogMsg("main: ERROR: importing DSA key: %s", error_to_string(rv));
			delete[] keybuf;
			goto fail;
		}
		delete[] keybuf;
	}
	
	DiskToolInfo info;
	info.deviceid = deviceId;
	strcpy(info.device_path, devicename);
	if(!DiskToolInfo::Open(devicename, &info))
	{
		printf("Access denied, failed to opening device %s\n", devicename);
		LogMsg("main: ERROR: Access denied, failed to opening device %s", devicename);
		goto fail;
	}

	task.print(&info);

	if (num_files > 0) printf("File source/target : %s\n", file_names[0]);

	if (!confirm("Are you sure you want to continue? [nothing will be written yet] ")) { status = -3;  goto fail; }
	
	LogMsg("main: Operation is '%s'", operation);
	switch (getoperation(operation))
	{
	case OP_CLONE:
		status = (ClonePartition(&task, &info) ? -2 : 0);
		break;
	case OP_MAKEDISK:
		status = MakeDisk((const char**)file_names, &task, &info);
		break;
	case OP_EXTRACT:
		status = (ExtractPartition(file_names[0], &task, &info) ? -2 : 0);
		break;
	case OP_WRITE:
		status = (WritePartition(file_names[0], &task, &info) ? -2 : 0);
		break;
	case OP_WRITESIG:
		status = (WriteSignature(file_names[0], &task, &info) ? -2 : 0);
		break;
	case OP_WRITEBOOT:
		status = (WriteBootloader(file_names[0], &task, &info) ? -2 : 0);
		break;
	case OP_DREAD:
		status = (DiskRead(file_names[0], &task, &info) ? -2 : 0);
		break;
	case OP_DWRITE:
		status = (DiskWrite(file_names[0], &task, &info) ? -2 : 0);
		break;
	default:
	case OP_UNKNOWN:
		status = error(-3, "no such operation");
		break;
	}

fail:
	info.Close();
	delete[] mbr;
	return status;
 }