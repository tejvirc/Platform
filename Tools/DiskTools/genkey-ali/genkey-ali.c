#include "stdafx.h"
#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>
#include <errno.h>

#include "m7i_crypt_support.h"

#include "m7i_settings.h"

#define VERSION "2.00"

#define DSA_KEY_RETRIES 10

typedef enum key_mode_e {
	KEY_RSA,
	KEY_DSA
} key_mode_t;


// Write buffer to file
int write_file(char *fname, unsigned char *out, unsigned long outlen) {
	FILE 		 *outf;
	int			  err;

	outf = fopen(fname, "wb");
	if (!outf) {
		fprintf(stderr, "Error opening %s: %s", fname, strerror(errno));
		return -errno;
	}
	err = fwrite(out, outlen, 1, outf);
	if (err < 0) {
		fprintf(stderr, "Error writing %s: %s", fname, strerror(errno));
		return -errno;
	}
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

// Display command line arguments
void help(char *exe) {
	printf("\nUsage: %s [OPTION..] <FILE>\n", exe);
	printf("Generate a RSA/DSA key pair\n\n");
	printf("Options:\n");
	printf("  -b, --bits=BITS    Key size in bits\n");
	printf("  -d, --dsa          Generate DSA key pair\n");
	printf("  -r, --rsa          Generate RSA key pair\n");
	printf("  -V, --version      Display program version and exit\n");
}

// Main function
int main(int argc, char *argv[]) {
	unsigned int  bits = -1;
	key_mode_t	  mode = KEY_RSA;
	int           err, prng_idx;
	unsigned char out[8192];
	unsigned long outlen;
	rsa_key       rsakey;
	dsa_key		  dsakey;
	char		  c;
	char		 *endp;
	char		 *file;
	char		  fname[PATH_MAX];
	char		  strVersion[PATH_MAX];

	if (0 == GetVersionFromRC(strVersion, PATH_MAX))
	{
		printf("\nAristocrat Genkey-ali %s\n", strVersion);
	}
	else
	{
		printf("\nAristocrat Genkey-ali\n");
	}

	// Handle command line args
	while (1) {
		int option_index = 0;
		static struct option long_options[] = {
			{ "bits",	required_argument,	0, 'b' },
			{ "rsa",		no_argument,		0, 'r' },
			{ "dsa",		no_argument,		0, 'd' },
			{ "version",	no_argument,		0, 'V' },
			{ 0, 0, 0, 0 }
		};

		c = getopt_long(argc, argv, "b:rdV",
			long_options, &option_index);
		if (c == -1)
			break;

		switch (c) {
		case 0:
			printf("option %s", long_options[option_index].name);
			if (optarg)
				printf(" with arg %s", optarg);
			printf("\n");
			break;

		case 'b':
			bits = strtoul(optarg, &endp, 0);
			if (endp == optarg) {
				help(argv[0]);
				return EXIT_FAILURE;
			}
			break;

		case 'r':
			mode = KEY_RSA;
			break;

		case 'd':
			mode = KEY_DSA;
			break;

		case 'V':
			printf("m7i_genkey.exe version " VERSION "\n");
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

	if (bits == -1) {
		if (mode == KEY_RSA)
			bits = M7I_RSA_KEYSIZE;
		else
			bits = M7I_DSA_KEYSIZE;
	}

	file = argv[optind];

	ALI_DEBUG("Generating %d bit %s key to file %s.pub, %s.prv\n", bits, mode == KEY_RSA ? "RSA" : "DSA", file, file);

	/* register prng/hash */
	if (register_prng(&sprng_desc) == -1) {
		printf("Error registering sprng");
		return EXIT_FAILURE;
	}
	/* register a math library (in this case TomFastMath) */
	//ltc_mp = tfm_desc;
	ltc_mp = ltm_desc;

	prng_idx = find_prng("sprng");

	switch (mode) {
	case KEY_RSA:
		// Make an RSA key 
		if ((err = rsa_make_key(NULL, prng_idx, bits / 8, M7I_RSA_E, &rsakey)) != CRYPT_OK) {
			fprintf(stderr, "Error: rsa_make_key %s\n", error_to_string(err));
			return EXIT_FAILURE;
		}

		// Export public key
		outlen = sizeof(out);
		if ((err = rsa_export(out, &outlen, PK_PUBLIC, &rsakey)) != CRYPT_OK) {
			fprintf(stderr, "Error: rsa_export %s\n", error_to_string(err));
			return EXIT_FAILURE;
		}
		sprintf(fname, "%s.pub", file);
		if ((err = write_file(fname, out, outlen)) < 0)
			return EXIT_FAILURE;

		// Export public key
		outlen = sizeof(out);
		if ((err = rsa_export(out, &outlen, PK_PRIVATE, &rsakey)) != CRYPT_OK) {
			fprintf(stderr, "Error: rsa_export %s\n", error_to_string(err));
			return EXIT_FAILURE;
		}
		sprintf(fname, "%s.prv", file);
		if ((err = write_file(fname, out, outlen)) < 0)
			return EXIT_FAILURE;
		break;

	case KEY_DSA:
	{
		int group_size;
		int modulus_size;
		int retry;
		int keystat;

		switch (bits) {
		case 1024:
			group_size = 20;
			modulus_size = 128;
			break;

		case 2048:
			group_size = 30;
			modulus_size = 256;
			break;

		case 3192:
			group_size = 35;
			modulus_size = 384;
			break;

		case 4096:
			group_size = 40;
			modulus_size = 512;
			break;

		default:
			fprintf(stderr, "Invalid DSA key size, please choose 1024, 2048, 3192 or 4096\n");
			return EXIT_FAILURE;
		}

		retry = 0;
		do {
			// Make an RSA key 
			if ((err = dsa_make_key(NULL, prng_idx, group_size, modulus_size, &dsakey)) != CRYPT_OK) {
				fprintf(stderr, "Error: dsa_make_key %s\n", error_to_string(err));
				return EXIT_FAILURE;
			}

			if ((err = dsa_verify_key(&dsakey, &keystat)) != CRYPT_OK) {
				fprintf(stderr, "Error: dsa_verify_key %s\n", error_to_string(err));
				return EXIT_FAILURE;
			}
		} while (!keystat && retry++ < DSA_KEY_RETRIES);

		if (!keystat) {
			fprintf(stderr, "Error: retries exceeded validating generating DSA keys\n");
			return EXIT_FAILURE;
		}
	}

	// Export public key
	outlen = sizeof(out);
	if ((err = dsa_export(out, &outlen, PK_PUBLIC, &dsakey)) != CRYPT_OK) {
		fprintf(stderr, "Error: dsa_export %s\n", error_to_string(err));
		return EXIT_FAILURE;
	}
	sprintf(fname, "%s.pub", file);
	if ((err = write_file(fname, out, outlen)) < 0)
		return EXIT_FAILURE;

	// Export public key
	outlen = sizeof(out);
	if ((err = dsa_export(out, &outlen, PK_PRIVATE, &dsakey)) != CRYPT_OK) {
		fprintf(stderr, "Error: dsa_export %s\n", error_to_string(err));
		return EXIT_FAILURE;
	}
	sprintf(fname, "%s.prv", file);
	if ((err = write_file(fname, out, outlen)) < 0)
		return EXIT_FAILURE;
	break;
	}

	return 0;
}