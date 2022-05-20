#ifndef __M7I_SETTINGS_H
#define __M7I_SETTINGS_H

#define ALI_DEBUG(x,...) do { printf(x, __VA_ARGS__); fflush(stdout); } while(0)
#define ALI_WARN(x,...) do { printf(x, __VA_ARGS__); fflush(stdout); } while(0)
#define ALI_ERROR(x,...) do { printf(x, __VA_ARGS__); fflush(stdout); } while(0)

#define M7I_RSA_KEYSIZE	2048
#define M7I_RSA_E		65537
#define M7I_RSA_SALTLEN 0
#define M7I_RSA_ENCODING LTC_PKCS_1_V1_5
//#define M7I_RSA_ENCODING LTC_PKCS_1_PSS

#define M7I_DSA_KEYSIZE	1024
//#define M7I_DSA_LITTLE_ENDIAN

#define M7I_HMAC_KEY ((unsigned char *)"\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
#define M7I_HMAC_KEYLEN 20

#define M7I_HASH_SECT_MAGIC ('A' | ('L' << 8) | ('I' << 16) | ('H' << 24))

typedef struct {
	unsigned char r[20];
	unsigned char s[20];
} dsa_signature_t;

#define MAX_DIRS 4096
#define MAX_FILES 8192
#ifndef PATH_MAX
#define PATH_MAX  4096
#endif

#define WHITELIST_FILE		"whitelist"
#define MANIFEST_FILE		"manifest"
#define MANIFEST_SIG_FILE	"manifest.sig"

#endif //__M7I_SETTINGS_H
