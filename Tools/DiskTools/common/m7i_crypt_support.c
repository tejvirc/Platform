#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include "m7i_settings.h"
#include "m7i_crypt_support.h"


// Sign hash in custom format requested by MSC
int dsa_sign_hash_custom(const unsigned char *in, unsigned long inlen,
	unsigned char *out, unsigned long *outlen,
	prng_state *prng, int wprng, dsa_key *key)
{
	dsa_signature_t	*dsig;
	int				 i;
	unsigned long	 nlen;
	void			*r;
	int				 rv;
	void			*s;
	unsigned char	 tbuf[20];

	if (*outlen < sizeof(dsa_signature_t)) {
		*outlen = sizeof(dsa_signature_t);
		return CRYPT_BUFFER_OVERFLOW;
	}
	if (mp_init_multi(&r, &s, NULL) != CRYPT_OK)
		return CRYPT_MEM;

	// Sign text
	if ((rv = dsa_sign_hash_raw(in, inlen, r, s, prng, wprng, key)) != CRYPT_OK) {
		mp_clear_multi(r, s, NULL);
		return rv;
	}

	dsig = (dsa_signature_t *)out;
	memset(dsig, 0, sizeof(dsig));

	// Debug R,S
	//	if(0) {
	//		char dbuf[256];
	//		mp_toradix(r, dbuf, 16);
	//		printf("\nr = %s", dbuf);
	//		mp_toradix(s, dbuf, 16);
	//		printf("\ns = %s\n", dbuf);
	//	}

	// Convert R to raw byte array
	nlen = mp_unsigned_bin_size(r);
	if (nlen > sizeof(dsig->r) || ((rv = mp_to_unsigned_bin(r, tbuf)) != CRYPT_OK)) {
		mp_clear_multi(r, s, NULL);
		return CRYPT_INVALID_ARG;
	}
	for (i = 0; i<nlen; i++)
#ifdef M7I_DSA_LITTLE_ENDIAN
		dsig->r[i] = tbuf[nlen - 1 - i];
#else
		dsig->r[19 - i] = tbuf[nlen - 1 - i];
#endif
	// Convert S to raw byte array
	nlen = mp_unsigned_bin_size(s);
	if (nlen > sizeof(dsig->s) || ((rv = mp_to_unsigned_bin(s, tbuf)) != CRYPT_OK)) {
		mp_clear_multi(r, s, NULL);
		return CRYPT_INVALID_ARG;
	}
	for (i = 0; i<nlen; i++)
#ifdef M7I_DSA_LITTLE_ENDIAN
		dsig->s[i] = tbuf[nlen - 1 - i];
#else
		dsig->s[19 - i] = tbuf[nlen - 1 - i];
#endif
	*outlen = sizeof(dsa_signature_t);

	mp_clear_multi(r, s, NULL);

	return CRYPT_OK;
}


// Verify signature in custom format requested by MSC
int dsa_verify_hash_custom(unsigned char *sig, unsigned long siglen, 
						   const unsigned char *hash, unsigned long inlen,
						   int *stat, dsa_key *key)
{
	dsa_signature_t *dsig;
	int				 i;
	void			*r;
	int				 rv;
	void			*s;
	unsigned char	 tbuf[20];

	if(siglen < sizeof(dsa_signature_t))
		return CRYPT_INVALID_PACKET;

	if((rv = mp_init_multi(&r, &s, NULL)) != CRYPT_OK)
		return CRYPT_MEM;

	dsig = (dsa_signature_t *)(sig);

	// Import R
	for(i=0; i<20; i++)
#ifdef M7I_DSA_LITTLE_ENDIAN
		tbuf[i] = dsig->r[19-i];
#else
		tbuf[i] = dsig->r[i];
#endif
	if((rv = mp_read_unsigned_bin(r, tbuf, 20)) != CRYPT_OK) {
		mp_clear_multi(r, s, NULL);
		return CRYPT_INVALID_PACKET;
	}

	// Import S
	for(i=0; i<20; i++)
#ifdef M7I_DSA_LITTLE_ENDIAN
		tbuf[i] = dsig->s[19-i];
#else
		tbuf[i] = dsig->s[i];
#endif
	if((rv = mp_read_unsigned_bin(s, tbuf, 20)) != CRYPT_OK) {
		mp_clear_multi(r, s, NULL);
		return CRYPT_INVALID_PACKET;
	}

	// Debug R,S
//	if(0) {
//		char dbuf[256];
//		mp_toradix(r, dbuf, 16);
//		printf("\nr = %s", dbuf);
//		mp_toradix(s, dbuf, 16);
//		printf("\ns = %s\n", dbuf);
//	}

	// Verify text
	rv = dsa_verify_hash_raw(r, s, hash, inlen, stat, key);

	mp_clear_multi(r, s, NULL);
	return rv;
}

// Load key from file
// Returns key data size
int load_key(char *file, unsigned char *key, unsigned long keylen) {
	FILE *inf;
	int rv;

	// Load private key
	inf = fopen(file, "rb");
	if(!inf)
		return -errno;
	rv = fread(key, 1, keylen, inf);
	fclose(inf);
	return rv;
}

// Decrypt RSA signature
int rsa_decrypt_sig(unsigned char *sbuf, unsigned long slen, 
					unsigned char *dbuf, unsigned long *dlen, 
					int *stat, rsa_key *key) 
{
	int err;
	unsigned char tbuf[512];
	unsigned long tlen = sizeof(tbuf);
	unsigned long modulus_bitlen;

	LTC_ARGCHK(sbuf != NULL);
	LTC_ARGCHK(dbuf != NULL);
	LTC_ARGCHK(stat != NULL);
	LTC_ARGCHK(key  != NULL);
	
	modulus_bitlen = mp_count_bits((key->N));
	
	err = rsa_exptmod(sbuf, slen, tbuf, &tlen, PK_PUBLIC, key);
	if(err != CRYPT_OK)
		return err;
		
	err = pkcs_1_v1_5_decode(tbuf, tlen, LTC_PKCS_1_EMSA, modulus_bitlen, dbuf, dlen, stat);
	if(err != CRYPT_OK)
		return err;
		
	return CRYPT_OK;
}

// Import raw public key
int rsa_import_raw_pub(unsigned char *in, unsigned long inlen, unsigned long exp, rsa_key *key) {
	int           err;
	LTC_ARGCHK(in          != NULL);
	LTC_ARGCHK(key         != NULL);
	LTC_ARGCHK(ltc_mp.name != NULL);

	/* init key */
	if ((err = mp_init_multi(&key->e, &key->d, &key->N, &key->dQ,
							 &key->dP, &key->qP, &key->p, &key->q, NULL)) != CRYPT_OK) {
		return err;
	}

	if( ((err = mp_read_unsigned_bin(key->N, in, inlen)) == CRYPT_OK)
			&& ((err = mp_set_int(key->e, exp)) == CRYPT_OK) ) {
		key->type = PK_PUBLIC;
		return CRYPT_OK;
	}

	mp_clear_multi(key->d,  key->e, key->N, key->dQ, key->dP, key->qP, key->p, key->q, NULL);
	return err;
}

