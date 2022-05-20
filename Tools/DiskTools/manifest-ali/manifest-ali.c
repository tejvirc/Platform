// manifest-ali.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <dirent.h> /* from common, port of *nix dirent */
#include <sys/types.h>  
#include <sys/stat.h>  
#include <getopt.h>
#include <compat.h>
#include "m7i_crypt_support.h"
#include "m7i_settings.h"
#include <crtdbg.h>
#define HIVEX_IMPLEMENTATION
#include <hivex.h>
#include <hivex_tools.h>
#define VERSION "2.01"

typedef enum hash_mode_e {
	HMODE_HASH,
	HMODE_HMAC
} hash_mode_t;

typedef enum key_mode_e {
	KEY_RSA,
	KEY_DSA
} key_mode_t;

const char *hex = "0123456789abcdef";
int quiet = 0;
int hash_symlinks = 0;
dsa_key		dsakey_priv;
dsa_key		dsakey_pub;
int dir_prune;
#define			MAX_WHITE_LIST  1024

char*		whitelist_table[MAX_WHITE_LIST];

int read_whitelist(const char* filename)
{
	FILE *plist = NULL;
	int i = 0;
	int total = 0;

	plist = fopen(filename, "rb");
	if (plist == NULL) return 0;
	whitelist_table[0] = calloc(MAX_PATH,1);
	while (fgets(whitelist_table[i], MAX_PATH, plist) && i < MAX_WHITE_LIST) {
		/* get rid of ending \n from fgets */
		if(whitelist_table[i][strlen(whitelist_table[i]) - 1] == '\n')
			whitelist_table[i][strlen(whitelist_table[i]) - 1] = '\0';
		if (whitelist_table[i][strlen(whitelist_table[i]) - 1] == '\r')
			whitelist_table[i][strlen(whitelist_table[i]) - 1] = '\0'; 
		if (strlen(whitelist_table[i]) > 0) // ignore empty lines
		{
			whitelist_table[++i] = calloc(MAX_PATH, 1);
		}
	}
	free(whitelist_table[i]);
	whitelist_table[i] = 0;
	total = i;
	fclose(plist);
	return total;

}
int is_whitelisted(const char* fname)
{
	char** p = &whitelist_table[0];
	while (*p)
	{
		if (strcasecmp(*p++, fname) == 0) return 1;
	}
	return 0;
}
void free_whitelist()
{
	char** p = &whitelist_table[0];
	while (*p)
	{
		free(*p++);
	}
}

// Display command line arguments
void help(char *exe) {
	int i;

	printf("\nUsage: %s [OPTION..] <DIR>\n", exe);
	printf("Generate manifest file with optional signature\n");
	printf("Outputs manifest to .manifest. in the current directory\n");
	printf("\n");
	printf("Options:\n");
	printf("  -h, --hash=ALGORITHM   Hash algorithm used (");
	for (i = 0; i<100; i++) {
		if (!hash_descriptor[i].name)
			break;
		printf("%s%s", (i>0) ? "," : "", hash_descriptor[i].name);
	}
	printf(")\n");
	printf("  -l, --hash-links       Hash symlink contents\n");
	printf("  -m, --hash-mode        Hash mode (hash,hmac)\n");
	printf("  -d, --dsa-sign         DSA sign manifest\n");
	//	printf("  -r, --rsa-sign         RSA sign manifest\n");
	printf("  -k, --key              Key file to use for signing\n");
	printf("  -V, --version          Display program version and exit\n");
	printf("  -w, --whitelist        Use whitelist file to ignore certain files (newline seperated list)\n");
	printf("\nOutputs to ./manifest\n");
}

struct hash_ctx
{
	hash_mode_t hashmode;
	hmac_state mst;
	hash_state hst;
};

static void hive_hasher(void* ctx, const unsigned char *in, unsigned long inlen)
{
	struct hash_ctx* hctx = (struct hash_ctx*)ctx;
	if (hctx->hashmode == HMODE_HMAC)
		hmac_process(&hctx->mst, in, (unsigned long)(inlen));
	else
		sha1_process(&hctx->hst, in, (unsigned long)(inlen));
}

char* generate_hive_hash(char* fname, int hash, hash_mode_t hashmode)
{
	unsigned char gen_hash[32];
	unsigned long i = 0;
	int rv = 0;
	static char out[129];
	char *outpos;
	struct hash_ctx hctx;
	hctx.hashmode = hashmode;
	
	if (hashmode == HMODE_HMAC)
	{
		rv = hmac_init(&hctx.mst, find_hash("sha1"), M7I_HMAC_KEY, M7I_HMAC_KEYLEN);
		
	}
	else
	{
		rv = sha1_init(&hctx.hst);
		
	}
	if (rv != CRYPT_OK)
	{
		ALI_DEBUG("failed initializing hasher\n");
		return 0;
	}
	
	hive_h* hive = hivex_open(fname, 0);

	if (hive == 0) return 0;

	
	hive_node_h root = hivex_root(hive);
	hashnode(hive_hasher, &hctx, hive, root);
	hivex_close(hive);
	
	
	
	unsigned long hashlen = sizeof(gen_hash);
	if (hashmode == HMODE_HMAC)
	{
		rv = hmac_done(&hctx.mst, (unsigned char*)gen_hash, &hashlen);
	}
	else
	{
		hashlen = 20;
		rv= sha1_done(&hctx.hst, (unsigned char*)gen_hash);
	}

	if (rv != CRYPT_OK) return 0;
	
	outpos = out;
	for (i = 0; i<hashlen; i++) {
		*outpos++ = hex[gen_hash[i] >> 4 & 0xf];
		*outpos++ = hex[gen_hash[i] & 0xf];
	}
	*outpos = 0;
	return out;
}

char *generate_file_hash(char *fname, int hash, hash_mode_t hashmode) {
	int rv;
	static char out[129];
	char *outpos;
	unsigned char data[64];
	unsigned long datalen;
	unsigned long i;

	datalen = sizeof(data);

	if (hashmode == HMODE_HMAC)
		rv = hmac_file(hash, fname, M7I_HMAC_KEY, M7I_HMAC_KEYLEN, data, &datalen);

	else
		rv = hash_file(hash, fname, data, &datalen);

	if (rv != CRYPT_OK) {
		ALI_ERROR("Error hashing '%s': %s\n", fname, error_to_string(rv));
		return NULL;
	}

	// Convert hash to ASCII
	outpos = out;
	for (i = 0; i<datalen; i++) {
		*outpos++ = hex[data[i] >> 4 & 0xf];
		*outpos++ = hex[data[i] & 0xf];
	}
	*outpos = 0;

	return out;
}

char *generate_hash(char *fname, int hash, hash_mode_t hashmode, char* ftype ) 
{
	

	FILE *file = NULL;
	int i = 0;
	int total = 0;
	unsigned char magic[4];
	file = fopen(fname, "rb");

	if (file == NULL) return 0;
	fread(magic, 1, 4, file);
	fclose(file);

	if (magic[0] == 'r' && magic[1] == 'e' && magic[2] == 'g' && magic[3] == 'f')
	{
		*ftype = 'r';
		return generate_hive_hash(fname, hash, hashmode);
	}
	
	*ftype = 'f';

	return generate_file_hash(fname, hash, hashmode);
}

int fnamecmp(const void *a, const void *b) {
	int r;
	r = strcmp(*((char**)a), *((char**)b));
	//DEBUG("fnamecmp(%p '%s', %p '%s') returns %d\n", a, *((char**)a), b, *((char**)b), r);
	return r;
}
char* win_to_nix_path(char* str) 
{
	char find = '\\';
	char replace = '/';
	char *p = strchr(str, find);
	while (p) {
		*p = replace;
		p = strchr(p, find);
	}
	return str;
}
int generate_manifest(FILE *outf, const char *target_dir, int hash, hash_mode_t hashmode) {
	DIR				*d;
	struct dirent	*de;
	char			 dirstr[PATH_MAX];
	int				 dirstrlen;
	char			*dir_name;
	char			*dir_stack[MAX_DIRS];
	int				 dir_stack_ents;
	char			 *file_str;
	char			*file_str_pos;
	char			*file_arr[MAX_FILES];
	int				 file_arr_ents;
	char			*hashstr;
	int				 i;
	int				 saved_dir_stack_ents;
	struct STAT	 st;

	

	if (!(file_str = malloc(MAX_FILES*NAME_MAX)))
		return -1;

	if (!target_dir || strlen(target_dir) == 0) {
		ALI_ERROR("Invalid dir '%s'\n", target_dir);
		return -1;
	}

	char dir[MAX_PATH];
	strcpy(dir, target_dir);
	// Truncate trailing /s
	dir_prune = strlen(dir) - 1;
	while (dir_prune > 0 && dir[dir_prune] == _DDC_)
		dir[dir_prune--] = 0;

	dir_stack[0] = (char*)malloc(strlen(dir) + 2);
	sprintf(dir_stack[0], "%s%s", dir, dir[strlen(dir) - 1] != _DDC_ ? _DD_ : "");
	dir[++dir_prune] = _DDC_; dir[++dir_prune] = 0;
	dir_stack_ents = 1;
	dir_prune = strlen(dir_stack[0]);

	while (dir_stack_ents > 0) {
		if (strlen(dir_stack[0]) == 0) {
			ALI_ERROR("Null dir!\n");
			return -1;
		}
		dirstrlen = sprintf(dirstr, "%s", dir_stack[0]); //, dir_stack[0][strlen(dir_stack[0])-1] != '/' ? "/" : "");

		d = opendir(dirstr);
		if (!d) {
			ALI_ERROR("Opening dir '%s': %s\n", dirstr, strerror(errno));
			return -errno;
		}

		// Read all file entries and save to either dir_stack or file_arr for further processing
		file_arr_ents = 0;
		file_str_pos = 0;
		file_str_pos = file_str;
		saved_dir_stack_ents = dir_stack_ents;
		do {
			errno = 0;
			de = readdir(d);
			if (!de && errno != 0) 
			{
				ALI_ERROR("Error reading dir '%ls': %s (%d)\n", d->wdirp->patt, strerror(errno), errno);
				errno = 0;
				return -errno;
			}
			if (!de)
				continue;
			
			// Ignore "." ".." and "manifest" in the root directory
			if (!strcmp(de->d_name, "..") || !strcmp(de->d_name, ".") || is_whitelisted(de->d_name)
				|| (!strncmp(dirstr, dir, dirstrlen) && !strcmp(de->d_name, "manifest"))
				|| (!strncmp(dirstr, dir, dirstrlen) && !strcmp(de->d_name, "whitelist")))
				continue;

			// Generate absolute file path
			strncpy(&dirstr[dirstrlen], de->d_name, PATH_MAX - dirstrlen);
			if (STAT(dirstr, &st) < 0) {
				ALI_ERROR("Unable to stat '%s': %s\n", dirstr, strerror(errno));
				errno = 0;
				//return -1;
			}

			// Save file name
			if (!S_ISDIR(st.st_mode)) {
				if (file_arr_ents >= MAX_FILES) {
					ALI_ERROR("Maximum number of files (%d) exceeded, please change MAX_FILES and recompile\n", MAX_FILES);
					return -1;
				}
				file_arr[file_arr_ents++] = file_str_pos;
				file_str_pos += sprintf(file_str_pos, "%s", de->d_name) + 1;
			}
			else {
				if (dir_stack_ents >= MAX_DIRS) {
					ALI_ERROR("Maximum number of dirs (%d) exceeded, please change MAX_DIRS and recompile\n", MAX_DIRS);
					return -1;
				}
				snprintf(&dirstr[dirstrlen], PATH_MAX - dirstrlen, "%s" _DD_, de->d_name);
				dir_stack[dir_stack_ents] = strdup(dirstr);
				dir_stack_ents++;
			}
		} while (de);

		closedir(d);

		// Sort file and dir entries
		//DEBUG("file_arr = %p\n", file_arr);
		qsort(&file_arr[0], file_arr_ents, sizeof(file_arr[0]), fnamecmp);
		qsort(&dir_stack[saved_dir_stack_ents], dir_stack_ents - saved_dir_stack_ents, sizeof(dir_stack[0]), fnamecmp);

		// Process files
		for (i = 0; i<file_arr_ents; i++) {
			// Generate absolute file path
			strncpy(&dirstr[dirstrlen], file_arr[i], PATH_MAX - dirstrlen);
			if (STAT(dirstr, &st) < 0) {
				ALI_ERROR("Unable to stat '%s': %s\n", dirstr, strerror(errno));
				errno = 0;
				//return -1;
				continue;
			}

			char nix_path[MAX_PATH];

			dir_name = &dir_stack[0][dir_prune];
			strcpy(nix_path, dir_name);
			win_to_nix_path(nix_path);
			char file_type = 'f';
			if (S_ISREG(st.st_mode)) {
				hashstr = generate_hash(dirstr, hash, hashmode, &file_type);
				if (!hashstr)
					return -1;

				fprintf(outf, "%s%s %c %lld %s\n", nix_path, file_arr[i], file_type, st.st_size, hashstr);

			}
			else if (S_ISCHR(st.st_mode))
				fprintf(outf, "%s%s c\n", nix_path, file_arr[i]);

			else if (S_ISBLK(st.st_mode))
				fprintf(outf, "%s%s b\n", nix_path, file_arr[i]);

			else if (S_ISFIFO(st.st_mode))
				fprintf(outf, "%s%s p\n", nix_path, file_arr[i]);

#ifndef WIN32
			else if (S_ISLNK(st.st_mode)) {
				if (hash_symlinks) {
					hashstr = generate_symlink_hash(dirstr, hash, hashmode);
					if (!hashstr)
						return -1;
					fprintf(outf, "%s%s l %lld %s\n", dir_name, file_arr[i], st.st_size, hashstr);
				}
				else
					fprintf(outf, "%s%s l\n", dir_name, file_arr[i]);

			}
			else if (S_ISSOCK(st.st_mode))
				fprintf(outf, "%s%s s\n", dir_name, file_arr[i]);
#endif
		}

		// Remove head and shuffle every element up one
		free(dir_stack[0]);
		dir_stack_ents--;
		for (i = 0; i<dir_stack_ents; i++)
			dir_stack[i] = dir_stack[i + 1];
	}

	return 0;
}



// Sign file with RSA/DSA
int sign_file(char *fname, key_mode_t signmode, char *keyfile) {
	unsigned char	 hash[64];
	unsigned long	 hashlen;
	unsigned char	 dsig[2048];
	unsigned long	 dsiglen;
	unsigned int	 i;
	int				stat = 0;
	FILE			*outf;
	int				 rv;

	hashlen = sizeof(hash);
	//	if(hashmode == HMODE_HMAC)
	rv = hmac_file(find_hash("sha1"), fname, M7I_HMAC_KEY, M7I_HMAC_KEYLEN, hash, &hashlen);
	//	else
	//		rv = hash_file(find_hash("sha1"), fname, hash, &hashlen);

	if (rv < 0)
		return rv;

	// Sign hash
	dsiglen = sizeof(dsig);
	if ((rv = dsa_sign_hash_custom(hash, hashlen, dsig, &dsiglen, NULL, find_prng("sprng"), &dsakey_priv)) != CRYPT_OK) {
		ALI_ERROR("Error signing DSA chunk: %s\n", error_to_string(rv));
		return -1;
	}
	if (dsakey_pub.qord)
	{
		rv = dsa_verify_hash_custom(dsig, dsiglen, hash, hashlen, &stat, &dsakey_pub);
	}

	// Write signature line
	outf = fopen(MANIFEST_FILE, "ab");
	if (!outf) {
		ALI_ERROR("Can't open '%s' for append: %s\n", MANIFEST_FILE, strerror(errno));
		return EXIT_FAILURE;
	}
	fprintf(outf, "manifest h ");
	for (i = 0; i<hashlen; i++) {
		fprintf(outf, "%02x", hash[i]);
	}
	fprintf(outf, " ");
	for (i = 0; i<dsiglen; i++) {
		fprintf(outf, "%02x", dsig[i]);
	}
	fclose(outf);

	return 0;
}

int add_whitelist_hash( char *whitelist, int hash, hash_mode_t hashmode) {
	
	unsigned long	 hashlen;
	FILE			*outf;
	struct STAT	 st;
	hashlen = sizeof(hash);
	//	if(hashmode == HMODE_HMAC)
	char* hashstr = generate_file_hash(whitelist, hash, hashmode);
	if (!hashstr)
		return -1;

	STAT(whitelist, &st);

	// Write signature line
	outf = fopen(MANIFEST_FILE, "ab");
	if (!outf) {
		ALI_ERROR("Can't open '%s' for append: %s\n", MANIFEST_FILE, strerror(errno));
		return EXIT_FAILURE;
	}

	fprintf(outf, "%s w %lld %s\n", &whitelist[dir_prune], st.st_size, hashstr);

	fclose(outf);

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
	char szFilename[PATH_MAX + 1] = { 0 };
	if (GetModuleFileName(NULL, szFilename, PATH_MAX) == 0)
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

// Main function
int main(int argc, char *argv[]) {
	char		  c;
	char		 *dir;
	int			  hash;
	hash_mode_t	  hashmode = HMODE_HMAC;
	unsigned char keybuf[4096];
	unsigned char keybuf_pub[4096];
	
	char		 *keyfile = NULL;
	char		 *whitelist = NULL;
	int			  keylen;
	int			  keylen_pub;
	FILE		 *outf;
	int			  rv;
	key_mode_t	  signmode = KEY_DSA;
	char		  strVersion[PATH_MAX];

	if (0 == GetVersionFromRC(strVersion, PATH_MAX))
	{
		printf("\nAristocrat Manifest-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Manifest-ali\n");
	}

	register_hash(&sha1_desc);
	register_hash(&sha256_desc);
	register_prng(&sprng_desc);
	ltc_mp = ltm_desc;

	hash = find_hash("sha1");

	// Handle command line args
	while (1) {
		
		int option_index = 0;
		static struct option long_options[] = {
			{ "hash",	required_argument,	0, 'h' },
			{ "hash_links",	no_argument,	0, 'l' },
			{ "mode",	required_argument,	0, 'm' },
			{ "dsa-sign", no_argument,		0, 'd' },
			{ "key",		required_argument,	0, 'k' },
			{ "whitelist",	no_argument,	0, 'i' },
			//			{"rsa-sign",no_argument,		0, 'r'},
			{ "version",	no_argument,		0, 'V' },
			{ 0, 0, 0, 0 }
		};
		_CrtCheckMemory();
		c = getopt_long(argc, argv, "h:lm:di:k:rV",long_options, &option_index);
		_CrtCheckMemory();
		if (c == -1)
			break;
		_CrtCheckMemory();
		switch (c) {
		case 0:
			printf("option %s", long_options[option_index].name);
			if (optarg)
				printf(" with arg %s", optarg);
			printf("\n");
			break;

		case 'h':
			hash = find_hash(optarg);
			if (hash < 0) {
				help(argv[0]);
				return EXIT_FAILURE;
			}
			break;

		case 'l':
			hash_symlinks = 1;
			break;

		case 'm':
			if (!strcasecmp(optarg, "hash"))
				hashmode = HMODE_HASH;
			else if (!strcasecmp(optarg, "hmac"))
				hashmode = HMODE_HMAC;
			else {
				help(argv[0]);
				return EXIT_FAILURE;
			}
			break;

		case 'r':
			signmode = KEY_RSA;
			break;

		case 'd':
			signmode = KEY_DSA;
			break;

		case 'k':
			keyfile = optarg;
			break;
		case 'i':
			whitelist = optarg;
			break;
		case 'V':
			printf("m7i_manifest version " VERSION "\n");
			exit(0);
			break;

		case '?':
		default:
			help(argv[0]);
			return EXIT_FAILURE;
			break;
		}
	}

	if (argc - optind != 1) {
		help(argv[0]);
		return EXIT_FAILURE;
	}

	if (!keyfile) {
		printf("No key specified for signing!\n\n");
		help(argv[0]);
		return EXIT_FAILURE;
	}
	dir = argv[optind];

	if (whitelist != 0)
	{
		if (strlen(whitelist) < strlen(dir) || (strncasecmp(whitelist, dir, strlen(dir)-1) != 0)  )
		{
			printf("whitelist must be on target directory\n\n");
			help(argv[0]);
			return EXIT_FAILURE;
		} 
	}
	// Import DSA key
	keylen = load_key(keyfile, keybuf, sizeof(keybuf));
	if (keylen < 0) {
		ALI_ERROR("Error loading DSA key '%s'\n", keyfile);
		return keylen;
	}
	if ((rv = dsa_import(keybuf, keylen, &dsakey_priv)) != CRYPT_OK) {
		ALI_ERROR("Error importing DSA key: %s\n", error_to_string(rv));
		return -1;
	}
	int l = strlen(keyfile);
	keyfile[l - 3] = 'p'; keyfile[l - 2] = 'u'; keyfile[l - 1] = 'b';
	keylen_pub = load_key(keyfile, keybuf_pub, sizeof(keybuf_pub));
	if (keylen_pub > 0) {

		if ((rv = dsa_import(keybuf_pub, keylen_pub, &dsakey_pub)) != CRYPT_OK) {
			ALI_ERROR("Error importing DSA key: %s\n", error_to_string(rv));
			return -1;
		}
	}

	
	//	if((signmode == KEY_RSA || signmode == KEY_DSA) && !keyfile) {
	//		ERROR("No key file for %s signature!\n", signmode == KEY_RSA ? "RSA" : "DSA");
	//		return EXIT_FAILURE;
	//	}

	ALI_DEBUG("Generating manifest of '%s' with %s%s, signed with %s\n",
		dir, hash_descriptor[hash].name, hashmode == HMODE_HMAC ? "-hmac" : "",
		signmode == KEY_RSA ? "RSA" : "DSA");

	// Generate manifest file
	outf = fopen(MANIFEST_FILE, "wb");
	if (!outf) {
		ALI_ERROR("Can't open '%s' for write: %s\n", MANIFEST_FILE, strerror(errno));
		return EXIT_FAILURE;
	}

	if (whitelist)
	{
		int size = read_whitelist(whitelist);
		if (size < 0)
		{
			ALI_ERROR("Can't open '%s' for read: %s\n", whitelist, strerror(errno));
			return EXIT_FAILURE;
		}
		ALI_DEBUG("imported %s whitelist with %d entries\n", whitelist, size);
	}
	rv = generate_manifest(outf, dir, hash, hashmode);
	if (rv < 0) {
		ALI_ERROR("Error generating manifest: %s", strerror(errno));
		return EXIT_FAILURE;
	}
	fclose(outf);
	
	add_whitelist_hash(whitelist, hash, hashmode);
	free_whitelist();


	// Sign manifest file
	rv = sign_file(MANIFEST_FILE, signmode, keyfile);
	if (rv < 0) {
		ALI_ERROR("Error signing manifest: %s", strerror(errno));
		return EXIT_FAILURE;
	}

	return 0;
}
