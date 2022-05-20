#ifndef __CRYPT_SUPPORT_H
#define __CRYPT_SUPPORT_H

#define LTM_DESC
#define LTC_SOURCE
#define LTC_NO_PROTOTYPES

#if defined(__cplusplus)
extern "C"  {
#endif

	#include <tomcrypt.h>

#if defined(__cplusplus)
 }
#endif
int dsa_verify_hash_custom(unsigned char *sig, unsigned long siglen, 
						   const unsigned char *hash, unsigned long inlen,
						   int *stat, dsa_key *key);
int dsa_sign_hash_custom(const unsigned char *in,  unsigned long inlen,
                         unsigned char *out, unsigned long *outlen,
						 prng_state *prng, int wprng, dsa_key *key);
int load_key(char *file, unsigned char *key, unsigned long keylen);
int rsa_decrypt_sig(unsigned char *sbuf, unsigned long slen, 
					unsigned char *dbuf, unsigned long *dlen, 
					int *stat, rsa_key *key);
int rsa_import_raw_pub(unsigned char *in, unsigned long inlen, unsigned long exp, rsa_key *key);


#endif //__CRYPT_SUPPORT_H
