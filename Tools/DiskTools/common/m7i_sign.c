#define _GNU_SOURCE

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>

#define LTC_SOURCE
#include <tomcrypt.h>

#include <libxml/xmlmemory.h>
#include <libxml/parser.h>
#include <libxml/xmlschemas.h>
#include <libxml/xmlschemastypes.h>

#include "m7i_settings.h"
#include "memmem.h"
#include "m7i_crypt_support.h"

#define VERSION "2.00"

#define DEBUG(x...) do { if(!quiet) printf(x); fflush(stdout); } while(0)
#define WARN(x...) do { printf(x); fflush(stdout); } while(0)
#define ERROR(x...) do { printf(x); fflush(stdout); } while(0)
#define NUM_HASH_BUFS 16
#define NUM_SAVED_ADDR 10

//#define DSA_LITTLE_ENDIAN

typedef unsigned char hash_buf_t[512];

int hash_idx, prng_idx;
int quiet;
hash_buf_t hash_storage[NUM_HASH_BUFS];
unsigned long hash_len[NUM_HASH_BUFS];
unsigned long saved_addr[NUM_SAVED_ADDR];

// Parse address of form "(@[0-9])?[+-]?(0x)?[0-9a-fA-F]+" eg. "@3-0x100" means offset -0x100 from saved address #3
static unsigned long parse_address(char *str_addr, int *valid) {
	char *endp;
	long n;

	*valid = 0;
	n = 0;
	if(str_addr[0] == '@') {
		int idx = str_addr[1] - '0';
		n = saved_addr[idx];
		str_addr += 2;
	}

	n += strtoul(str_addr, &endp, 0);

	if(*endp)
		return n;

	if(n < 0) {
		ERROR("Error: calculated address (%lx) is negative!\n", n);
		return n;
	}

	*valid = 1;
	return n;
}

// Parse and handle a MARK tag
// Mark address
static int parse_mark(xmlNodePtr cur, unsigned char *img, unsigned long imglen) {
	xmlChar *str_id;
	xmlChar *str_offset;
	xmlChar *find_str;
	xmlChar *find_hex;
	int id;
	unsigned long offset;
	char *endp;

	str_id = xmlGetProp(cur, (const xmlChar *)		"id");
	str_offset = xmlGetProp(cur, (const xmlChar *)	"offset");
	find_str = xmlGetProp(cur, (const xmlChar *)	"find_str");
	find_hex = xmlGetProp(cur, (const xmlChar *)	"find_hex");

	// Offset
	if(!str_id) {
		ERROR("Error, no mark ID specified\n");
		return -1;
	}
	id = strtoul(str_id, &endp, 0);
	if(*endp || id < 0 || id >= NUM_SAVED_ADDR) {
		ERROR("Error: bad mark id '%s'\n", str_id);
		return -1;
	}

	// Offset
	if(str_offset) {
		offset = strtoul(str_offset, &endp, 0);
		if(*endp) {
			ERROR("Bad mark offset '%s'\n", str_offset);
			return -1;
		}
	} else
		offset = 0;

	// Search for signature if specified
	if(find_hex || find_str) {
		// Search for signature offset
		unsigned long found_offset;
		char sig[256], *spos;
		unsigned long siglen;
		void *found;
		unsigned char match;

		// Convert offset match string hex data to binary
		if(find_hex) {
			spos = find_hex;
			siglen = 0;
			while(*spos) {
				if(!isxdigit(*spos)) {
					ERROR("Error: invalid character in offset match string\n");
					return -1;
				}
				match = (*spos >= '0'&& *spos <= '9') ? *spos - '0' : toupper(*spos) - 'A' + 10;
				match <<= 4;
				do { spos++; } while(*spos && isspace(*spos));
				if(!*spos) {
					WARN("Warning: offset match string is byte length multiple, ignoring cruft at end\n");
					continue;
				}
				match += (*spos >= '0'&& *spos <= '9') ? *spos - '0' : toupper(*spos) - 'A' + 10;
				do { spos++; } while(*spos && isspace(*spos));
				sig[siglen++] = match;
			}
		} else {
			siglen = strlen(find_str);
			if(siglen > sizeof(sig)-1)
				siglen = sizeof(sig)-1;
			memcpy(sig, find_str, siglen);
		}
	
		found = memmem(img, imglen, sig, siglen);

		if(!found) {
			ERROR("Error, signature '%s' not found in image\n", find_str);
			return -1;
		}

		found_offset = (char*)found - (char*)img;
		offset += found_offset;
	}

	saved_addr[id] = offset;
	DEBUG("Mark id #%d is offset 0x%lX\n", id, offset);

	return 0;
}

// Parse and handle an EMBED tag
// Embeds a file inside current image
static int parse_embed(xmlNodePtr cur, unsigned char *img, unsigned long imglen) {
	FILE *inf;
	int rv;
	xmlChar *file;
	xmlChar *str_hash_buf;
	xmlChar *str_place;
	xmlChar *str_insize;
	xmlChar *str_mark_start;
	xmlChar *str_mark_end;
	unsigned long place, insize, fsize;
	int mark_start, mark_end;
	int hash_buf_id;
	int valid;
	char *endp;

	file = xmlGetProp(cur, 			(const xmlChar *) "file");
	str_hash_buf = xmlGetProp(cur,	(const xmlChar *) "hash_buf");
	str_place = xmlGetProp(cur, 	(const xmlChar *) "place");
	str_insize = xmlGetProp(cur, 	(const xmlChar *) "size");
	str_mark_start = xmlGetProp(cur,(const xmlChar *) "mark_start_id");
	str_mark_end = xmlGetProp(cur, 	(const xmlChar *) "mark_end_id");

	if(!file && !str_hash_buf) {
		WARN("Warning: no input selected for embed\n");
		return 0;
	}
	if(file && str_hash_buf) {
		ERROR("Error: can't embed file and hash buffer at the same time\n");
		return -1;
	}

	// Hash buffer to store result
	if(str_hash_buf) {
		hash_buf_id = strtoul(str_hash_buf, &endp, 0);
		if(*endp) {
			ERROR("Error: bad hash_buf id '%s'\n", str_hash_buf);
			return -1;
		}
		if(hash_buf_id < 0 || hash_buf_id >= NUM_HASH_BUFS) {
			ERROR("Error, invalid hash buffer ID\n");
			return -1;
		}
	} else
		hash_buf_id = (unsigned)-1;

	// Extract number arguments
	// Offset
	if(str_place) {
		place = parse_address(str_place, &valid);
		if(!valid) {
			ERROR("Error: bad place '%s'\n", str_place);
			return -1;
		}
	} else
		place = 0;

	// In size
	if(str_insize) {
		insize = strtoul(str_insize, &endp, 0);
		if(*endp) {
			ERROR("Error: bad in size '%s'\n", str_insize);
			return -1;
		}
	} else
		insize = (unsigned)-1;

	// Mark start ID
	if(str_mark_start) {
		mark_start = strtoul(str_mark_start, &endp, 0);
		if(*endp) {
			ERROR("Error: bad mark start id '%s'\n", str_mark_start);
			return -1;
		}
	} else
		mark_start = -1;

	// Mark start ID
	if(str_mark_end) {
		mark_end = strtoul(str_mark_end, &endp, 0);
		if(*endp) {
			ERROR("Error: bad mark end id '%s'\n", str_mark_end);
			return -1;
		}
	} else
		mark_end = -1;

	if(file) {
		// Load input file
		inf = fopen(file, "rb");
		if(!inf) {
			ERROR("Error opening embed file '%s': %s\n", file, strerror(errno));
			return -1;
		}
	
		// Seek to end to (portably) discover file size
		rv = fseek(inf, 0, SEEK_END);
		if(rv < 0) {
			fclose(inf);
			ERROR("Error seeking embed file '%s': %s\n", file, strerror(errno));
			return -1;
		}
		fsize = ftell(inf);
		rv = fseek(inf, 0, SEEK_SET);
		if(rv < 0) {
			fclose(inf);
			ERROR("Error seeking embed file '%s': %s\n", file, strerror(errno));
			return -1;
		}
	}

	if(str_hash_buf) {
		insize = hash_len[hash_buf_id];
	}

	if(file && insize > fsize)
		insize = fsize;

	if(place + insize > imglen) {
		WARN("Warning: embed data will not fit in file at location 0x%lX, requires 0x%lX bytes\n", place, insize);
		insize = imglen - place;
	}

	if(file)
		DEBUG("Embedding file '%s' (0x%lX bytes) at offset 0x%lX\n", file, insize, place);
	else 
		DEBUG("Embedding hash buffer #%d (0x%lX bytes) at offset 0x%lX\n", hash_buf_id, insize, place);

	// Save start address if requested
	if(str_mark_start && mark_start >= 0 && mark_start < NUM_SAVED_ADDR) {
		saved_addr[mark_start] = place;
		DEBUG("Marked embed start address id #%d offset 0x%lX\n", mark_start, saved_addr[mark_start]);
	}
	// Save end address if requested
	if(str_mark_end && mark_end >= 0 && mark_end < NUM_SAVED_ADDR) {
		saved_addr[mark_end] = place+insize;
		DEBUG("Marked embed end address id #%d offset 0x%lX\n", mark_end, saved_addr[mark_end]);
	}

	// Read input file
	if(file) {
		rv = fread(img+place, 1, insize, inf);
		if(rv < 0) {
			fclose(inf);
			fprintf(stderr, "Error reading embed file '%s': %s\n", file, strerror(errno));
			return -1;
		}
	
		if(rv != insize) {
			fprintf(stderr, "Warning: only read 0x%x bytes\n", rv);
		}
	}

	if(str_hash_buf) {
		memcpy(img+place, hash_storage[hash_buf_id], insize);
	}

	return 0;
}

// Parse and handle a HASH range
static int parse_range(xmlNodePtr cur, unsigned char *img, unsigned long imglen, unsigned long *start, unsigned long *end) {
	xmlChar *str_start;
	xmlChar *str_end;
	int valid;

	str_start = xmlGetProp(cur, 	(const xmlChar *) "start");
	str_end = xmlGetProp(cur, 		(const xmlChar *) "end");

	// Extract number arguments
	// Start position
	if(str_start) {
		*start = parse_address(str_start, &valid);
		if(!valid) {
			ERROR("Error: bad start address '%s'\n", str_start);
			return -1;
		}
	} else
		*start = 0;

	// End position
	if(str_end) {
		*end = parse_address(str_end, &valid);
		if(!valid) {
			fprintf(stderr, "Bad end address '%s'\n", str_end);
			return -1;
		}
	} else
		*end = 0xffffffff;

	if(*end > imglen)
		*end = imglen;

	if(*start > *end)
		*start = *end;

	return 0;
}

// Handle hash setup
static int hash_init(char *type, char *mode, char *key_file, unsigned long *outlen, 
					 hash_state *hst, hmac_state *hmt, rsa_key *rsakey, dsa_key *dsakey) {
	int		 hash;
	struct ltc_hash_descriptor	*hdesc;
	char	 keybuf[4096];
	int		 rv;

	// Handle RSA/DSA setup
	if(!xmlStrcasecmp(type, (const xmlChar *) "RSA") || !xmlStrcasecmp(type, (const xmlChar *) "DSA")) {
		if(!key_file) {
			ERROR("Error: no key file specified for RSA/DSA signing\n");
			return -1;
		}

		int keylen = load_key(key_file, keybuf, sizeof(keybuf));
		if(keylen < 0) {
			ERROR("Error loading DSA key '%s'\n", key_file);
			return keylen;
		}

		// Import key
		if(!xmlStrcasecmp(type, (const xmlChar *) "RSA")) {
			if((rv = rsa_import(keybuf, keylen, rsakey)) != CRYPT_OK) {
				ERROR("Error importing RSA key: %s\n", error_to_string(rv));
				return -1;
			}
		} else {
			if((rv = dsa_import(keybuf, keylen, dsakey)) != CRYPT_OK) {
				ERROR("Error importing DSA key: %s\n", error_to_string(rv));
				return -1;
			}
		}

		return 0;
	}

	// Find hash descriptor
	hash = find_hash(type);
	if(hash < 0) {
		ERROR("Unknown hash type '%s'\n", type);
		return -1;
	}
	hdesc = &hash_descriptor[hash];

	// Handle HMAC hashing
	if(!xmlStrcasecmp(mode, (const xmlChar *) "HMAC")) {
		if((rv = hmac_init(hmt, hash, M7I_HMAC_KEY, M7I_HMAC_KEYLEN)) != CRYPT_OK) {
			ERROR("Error setting up HMAC-%s: %s", type, error_to_string(rv));
			return -1;
		}
		*outlen = hdesc->hashsize;

	// Handle straight hashing
	} else if(!xmlStrcasecmp(mode, (const xmlChar *) "HASH")) {
		if((rv = hdesc->init(hst)) != CRYPT_OK) {
			ERROR("Error setting up %s: %s", type, error_to_string(rv));
			return -1;
		}
		*outlen = hdesc->hashsize;

	} else {
		fprintf(stderr, "Error unknown hash mode '%s'\n", mode);
		return -1;
	}

	return 0;
}

// Process hash function
static int hash_process( xmlNodePtr cur, unsigned char *img, unsigned long imglen, unsigned long place,
						 char *type, char *mode, int verify, unsigned long start, unsigned long end, 
						 unsigned char *out, unsigned long *outlen,// int *verify_stat,
						 hash_state *hst, hmac_state *hmt, rsa_key *rsakey, dsa_key *dsakey )
{
	int		 hash;
	struct ltc_hash_descriptor	*hdesc;
	int rv;
	int verify_stat;

	// Handle RSA signing
	if(!xmlStrcasecmp(type, (const xmlChar *) "RSA")) {
		// If verify option set, verify existing signature
		if(verify) {
			// Verify text
			if((rv = rsa_verify_hash_ex(img+place, 256, img+start, end-start, M7I_RSA_ENCODING, hash_idx, M7I_RSA_SALTLEN, &verify_stat, rsakey)) != CRYPT_OK) {
				ERROR("Error verifying RSA chunk: %s\n", error_to_string(rv));
				return -1;
			}

			if(!verify_stat) {
				ERROR("Warning: RSA signature did not verify\n");
			}

		// Otherwise calculate signature 
		} else {
			// Sign text
			if((rv = rsa_sign_hash_ex(img+start, end-start, out, outlen, M7I_RSA_ENCODING, NULL, prng_idx, hash_idx, M7I_RSA_SALTLEN, rsakey)) != CRYPT_OK) {
				ERROR("Error signing RSA chunk: %s\n", error_to_string(rv));
				return -1;
			}
		}
		return 0;

	// Handle DSA signing
	} else if(!xmlStrcasecmp(type, (const xmlChar *) "DSA")) {
		// If verify option set, verify existing signature
		if(verify) {
			// Verify text
			if((rv = dsa_verify_hash_custom(img+place, imglen-place, img+start, end-start, &verify_stat, dsakey)) != CRYPT_OK) {
				ERROR("Error verifying DSA chunk: %s\n", error_to_string(rv));
				return -1;
			}

			if(!verify_stat) {
				fprintf(stderr, "Warning: DSA signature did not verify\n");
			}

		// Otherwise calculate signature 
		} else {
			// Sign text
			if((rv = dsa_sign_hash_custom(img+start, end-start, out, outlen, NULL, prng_idx, dsakey)) != CRYPT_OK) {
				ERROR("Error signing DSA chunk: %s\n", error_to_string(rv));
				return -1;
			}
		}
		return 0;
	}

	// Find hash descriptor
	hash = find_hash(type);
	if(hash < 0) {
		ERROR("Unknown hash type '%s'\n", type);
		return -1;
	}

	// Handle HMAC hashing
	if(!xmlStrcasecmp(mode, (const xmlChar *) "HMAC")) {
		if((rv = hmac_process(hmt, img+start, end-start)) != CRYPT_OK) {
			ERROR("Error hashing HMAC-%s chunk: %s", type, error_to_string(rv));
			return -1;
		}

	// Handle straight hashing
	} else if(!xmlStrcasecmp(mode, (const xmlChar *) "HASH")) {
		hdesc = &hash_descriptor[hash];

		if((rv = hdesc->process(hst, img+start, end-start)) != CRYPT_OK) {
			ERROR("Error hashing %s chunk: %s", type, error_to_string(rv));
			return -1;
		}
	} else {
		fprintf(stderr, "Error unknown hash mode '%s'\n", mode);
		return -1;
	}

	return 0;
}

// Complete hash
static int hash_done( xmlNodePtr cur, unsigned char *img, unsigned long imglen, unsigned long place,
					  char *type, char *mode, int verify,  unsigned char *out, unsigned long outlen,
					  int rangecnt, hash_state *hst, hmac_state *hmt )
{
	int		 hash;
	struct ltc_hash_descriptor	*hdesc;
	int rv;

	// Handle RSA/DSA completion
	if(!xmlStrcasecmp(type, (const xmlChar *) "RSA") || !xmlStrcasecmp(type, (const xmlChar *) "DSA")) {
		if(rangecnt > 1) {
			WARN("Warning: multiple signing ranges not supported for RSA/DSA\n");
		}
		return 0;
	}

	// Find hash descriptor
	hash = find_hash(type);
	if(hash < 0) {
		ERROR("Unknown hash type '%s'\n", type);
		return -1;
	}

	// Handle HMAC hashing
	if(!xmlStrcasecmp(mode, (const xmlChar *) "HMAC")) {
		if((rv = hmac_done(hmt, out, &outlen)) != CRYPT_OK) {
			ERROR("Error completing HMAC-%s: %s", type, error_to_string(rv));
			return -1;
		}
		if(verify) {
			if(imglen-place < outlen || memcmp(out, img+place, outlen)) {
				WARN("Warning: SHA1 hash does not match\n");
			}
		}

	// Handle straight hashing
	} else if(!xmlStrcasecmp(mode, (const xmlChar *) "HASH")) {
		hdesc = &hash_descriptor[hash];

		if((rv = hdesc->done(hst, out)) != CRYPT_OK) {
			ERROR("Error completing %s: %s", type, error_to_string(rv));
			return -1;
		}
		if(verify) {
			if(imglen-place < outlen || memcmp(out, img+place, outlen)) {
				WARN("Warning: SHA1 hash does not match\n");
			}
		}

	} else {
		fprintf(stderr, "Error unknown hash mode '%s'\n", mode);
		return -1;
	}

	return 0;
}

// Parse and handle a HASH tag
// Hashes or signs a section of the binary
static int parse_hash(xmlNodePtr cur, unsigned char *img, unsigned long imglen) {
	int i;
	int rv;
	xmlChar *type;
	xmlChar *mode;
	xmlChar *str_place;
	xmlChar *str_hash_buf;
	xmlChar *key_file;
	xmlChar *str_verify;
	int verify = 0;
	int valid;
	hash_state hst;
	hmac_state hmt;
	char *endp;
	unsigned long outlen;
	unsigned char buf[2048];
	int hash_buf_id;
	int rangecnt;
	rsa_key rsakey;
	dsa_key dsakey;
	unsigned long start, end, place;

	type = xmlGetProp(cur, 			(const xmlChar *) "type");
	mode = xmlGetProp(cur, 			(const xmlChar *) "mode");
	str_place = xmlGetProp(cur, 	(const xmlChar *) "place");
	str_hash_buf = xmlGetProp(cur, 	(const xmlChar *) "hash_buf");
	key_file = xmlGetProp(cur, 		(const xmlChar *) "key_file");
	str_verify = xmlGetProp(cur,	(const xmlChar *) "verify");

	if(!mode)
		mode = "HASH";

	// Make hash type lower-case
	for(i=0; i<strlen(type); i++)
		type[i] = tolower(type[i]);

	// Where to place result
	if(str_place) {
		place = parse_address(str_place, &valid);
		if(!valid) {
			ERROR("Error: bad place '%s'\n", str_place);
			return -1;
		}
	} else
		place = (unsigned)-1;

	// Hash buffer to store result
	if(str_hash_buf) {
		hash_buf_id = strtoul(str_hash_buf, &endp, 0);
		if(*endp) {
			ERROR("Error: bad hash_buf id '%s'\n", str_hash_buf);
			return -1;
		}
		if(hash_buf_id < 0 || hash_buf_id >= NUM_HASH_BUFS) {
			ERROR("Error, invalid hash buffer ID\n");
			return -1;
		}
	} else
		hash_buf_id = (unsigned)-1;

	if(str_verify && (toupper(str_verify[0]) == 'Y' || toupper(str_verify[0]) == 'T'))
	   verify = 1;

	if(verify) {
		char from_target[256];
		from_target[0] = 0;
		if(str_place)
			sprintf(from_target, " from 0x%lX", place);
		else if(str_hash_buf)
			sprintf(from_target, " from hash buffer #%d", hash_buf_id);
		if(key_file) 
			DEBUG("Verifying hash (%s%s)%s key file '%s'", !strcasecmp(mode, "HMAC") ? "HMAC-" : "", type,
				  from_target, key_file);
		else
			DEBUG("Verifying hash (%s%s)%s", !strcasecmp(mode, "HMAC") ? "HMAC-" : "", type, from_target);
	} else {
		char place_target[256];
		place_target[0] = 0;
		if(str_place)
			sprintf(place_target, " place at 0x%lX", place);
		else if(str_hash_buf)
			sprintf(place_target, " place in hash buffer #%d", hash_buf_id);

		if(key_file) 
			DEBUG("Hashing (%s%s)%s key file '%s'", !strcasecmp(mode, "HMAC") ? "HMAC-" : "", type,
				  place_target, key_file);
		else
			DEBUG("Hashing (%s%s)%s", !strcasecmp(mode, "HMAC") ? "HMAC-" : "", type, place_target);
	}

	// Setup hash
	outlen = sizeof(buf);
	rv = hash_init(type, mode, key_file, &outlen, &hst, &hmt, &rsakey, &dsakey);
	if(rv < 0) {
		return rv;
	}

	// Process each range specified
	cur = cur->xmlChildrenNode;
	rangecnt = 0;
	while(cur) {
		// Skip whitespace and comments
		while ( cur && (xmlIsBlankNode(cur) || cur->type == XML_COMMENT_NODE) ) {
			cur = cur -> next;
		}
		if(!cur)
			break;

		// Handle hash range
		if(!xmlStrcasecmp(cur->name, (const xmlChar *) "RANGE")) {
			rv = parse_range(cur, img, imglen, &start, &end);
			if(rv < 0) {
				return rv;
			}
			DEBUG(" range 0x%lX-0x%lX", start, end);

			rv = hash_process(cur, img, imglen, place, type, mode, verify, start, end, buf, &outlen, &hst, &hmt, &rsakey, &dsakey);
			if(rv < 0) {
				return rv;
			}
			rangecnt++;

		// Otherwise unknown tag
		} else {
			fprintf(stderr,"unknown tag '%s', EMBED or HASH expected\n",
					cur->name);
			return -1;
		}

		cur = cur->next;
	}

	DEBUG("\n");

	// Complete hash
	rv = hash_done(cur, img, imglen, place, type, mode, verify, buf, outlen, rangecnt, &hst, &hmt);
	if(rv < 0) {
		return rv;
	}

	// Save signature
	if(!verify) {
		// Copy signature to output image if place address included
		if(str_place) {
			if(place > imglen) {
				ERROR("Error: signature place address is beyond end of file!\n");
				return -1;
			}
			if(place + outlen > imglen) {
				WARN("Warning: signature will not fit in file at location 0x%lx, requires 0x%lX bytes\n", place, outlen); 
				memcpy(img+place, buf, imglen-place);
			} else
				memcpy(img+place, buf, outlen);
		}

		// If requested, store to temporary hash buffer
		if(str_hash_buf && hash_buf_id >= 0 && hash_buf_id < NUM_HASH_BUFS) {
			memcpy(hash_storage[hash_buf_id], buf, outlen);
			hash_len[hash_buf_id] = outlen;
		}
	}

	return 0;
}

// Parse and handle a FILL tag
// Fill section of the binary with constant value (0 for now)
static int parse_fill(xmlNodePtr cur, unsigned char *img, unsigned long imglen) {
	int rv;
	unsigned long	 start, end;

	rv = parse_range(cur, img, imglen, &start, &end);
	if(rv < 0) {
		return rv;
	}

	memset(img+start, 0, end-start);

	return 0;
}

// Parse and handle an IMAGE tag
// Opens a binary image to manipulate
static int parse_image(xmlNodePtr cur) {
	FILE *inf, *outf;
	int rv;
	unsigned char *img;
	char *endp;
	unsigned long imglen, insize, outsize, fsize;
	xmlChar *file;
	xmlChar *output;
	xmlChar *str_insize;
	xmlChar *str_outsize;

	file = xmlGetProp(cur, 			(const xmlChar *) "file");
	output = xmlGetProp(cur, 		(const xmlChar *) "output");
	str_insize = xmlGetProp(cur, 	(const xmlChar *) "insize");
	str_outsize = xmlGetProp(cur, 	(const xmlChar *) "outsize");

	// Extract number arguments
	// Input size - how much data to read from image
	if(str_insize) {
		insize = strtoul(str_insize, &endp, 0);
		if(*endp) {
			ERROR("Error: bad in size '%s'\n", str_insize);
			return -1;
		}
	} else
		insize = (unsigned)-1;

	// Output size - how much to write out (excess zero padded)
	if(str_outsize) {
		outsize = strtoul(str_outsize, &endp, 0);
		if(*endp) {
			ERROR("Error: bad out size '%s'\n", str_outsize);
			return -1;
		}
	} else
		outsize = (unsigned)-1;

	// Load input file
	inf = fopen(file, "rb");
	if(!inf) {
		ERROR("Error opening image file '%s': %s\n", file, strerror(errno));
		return -1;
	}

	// Seek to end to (portably) discover file size
	rv = fseek(inf, 0, SEEK_END);
	if(rv < 0) {
		fclose(inf);
		ERROR("Error seeking image file '%s': %s\n", file, strerror(errno));
		return -1;
	}
	fsize = ftell(inf);
	rv = fseek(inf, 0, SEEK_SET);
	if(rv < 0) {
		fclose(inf);
		ERROR("Error seeking image file '%s': %s\n", file, strerror(errno));
		return -1;
	}
	if(insize > fsize)
		insize = fsize;

	if(!str_outsize)
		outsize = fsize;

	if(output)
		DEBUG("Processing image file '%s', output '%s' in 0x%lx bytes, out 0x%lX bytes\n", file, output, insize, outsize);
	else
		DEBUG("Processing image file '%s', in 0x%lx bytes\n", file, insize);

	// Allocate image storage
	if(insize > outsize) {
		WARN("Warning: insize > outsize, output file will be truncated\n");
		insize = outsize;
	}
	imglen = outsize;
	img = calloc(1, outsize);

	// Read image
	rv = fread(img, 1, insize, inf);
	if(rv < 0) {
		free(img);
		fclose(inf);
		ERROR("Error reading image file '%s': %s\n", file, strerror(errno));
		return -1;
	}
	if(str_insize) {
		if(rv < insize) {
			fprintf(stderr, "Warning: could not read %ld bytes, only %d bytes read\n", insize, rv);
			insize = rv;
		}
	} else
		insize = rv;
	fclose(inf);

	cur = cur->xmlChildrenNode;
	while(cur) {
		// Skip whitespace and comments
		while ( cur && (xmlIsBlankNode(cur) || cur->type == XML_COMMENT_NODE) ) {
			cur = cur -> next;
		}
		if(!cur) 
			break;

		// Handle embedding file
		if(!xmlStrcasecmp(cur->name, (const xmlChar *) "embed")) {
			rv = parse_embed(cur, img, imglen);
			if(rv < 0) {
				free(img);
				return rv;
			}

		// Handle mark address
		} else if(!xmlStrcasecmp(cur->name, (const xmlChar *) "mark")) {
			rv = parse_mark(cur, img, imglen);
			if(rv < 0) {
				free(img);
				return rv;
			}

		// Handle crypto hash
		} else if(!xmlStrcasecmp(cur->name, (const xmlChar *) "hash")) {
			rv = parse_hash(cur, img, imglen);
			if(rv < 0) {
				free(img);
				return rv;
			}

		// Handle range fill
		} else if(!xmlStrcasecmp(cur->name, (const xmlChar *) "fill")) {
			rv = parse_fill(cur, img, imglen);
			if(rv < 0) {
				free(img);
				return rv;
			}

		// Otherwise unknown tag
		} else {
			ERROR("Error: unknown tag '%s'\n",
					cur->name);
			free(img);
			return -1;
		}

		cur = cur->next;
	}

	if(output) {
		// Write output file
		outf = fopen(output, "wb");
		if(!outf) {
			free(img);
			ERROR("Error opening output file '%s': %s\n", output, strerror(errno));
			return -1;
		}
		rv = fwrite(img, outsize, 1, outf);
		if(rv < 0) {
			free(img);
			fclose(outf);
			ERROR("Error reading output file '%s': %s\n", output, strerror(errno));
			return -1;
		}
		fclose(outf);
	}

	free(img);
	return 0;
}

// Parse current config file
static int parse_config(char *filename) {
	xmlDocPtr doc;
	int rv;
	xmlNodePtr cur;

	DEBUG("Reading file '%s'\n", filename);

#ifdef LIBXML_SAX1_ENABLED
	/*
	 * build an XML tree from a the file;
	 */
	doc = xmlParseFile(filename);
	if (doc == NULL) return -1;
#else
	/*
	 * the library has been compiled without some of the old interfaces
	 */
	return -1;
#endif /* LIBXML_SAX1_ENABLED */

	/*
	 * Check the document is of the right kind
	 */

	cur = xmlDocGetRootElement(doc);
	if (cur == NULL) {
		ERROR("Error: empty document\n");
		xmlFreeDoc(doc);
		return -1;
	}
	//ns = xmlSearchNsByHref(doc, cur,
	//    (const xmlChar *) "http://www.gnome.org/some-location");
	//if (ns == NULL) {
	//    fprintf(stderr,
	//        "document of the wrong type, GJob Namespace not found\n");
	//xmlFreeDoc(doc);
	//return(NULL);
	//}

#if 0
	// Load schema
	xmlSchemaParserCtxtPtr pctxt = xmlSchemaNewParserCtxt("config.xsd");
	xmlSchemaPtr schema = xmlSchemaParse(pctxt);
	xmlSchemaFreeParserCtxt(pctxt);

	// Validate against schema
	if(!schema) {
		fprintf(stderr, "Error loading XML schema for validation\n");
		xmlSchemaFree(schema);
	} else {
		xmlSchemaValidCtxtPtr vctxt = xmlSchemaNewValidCtxt(schema);
		xmlSchemaSetValidErrors(vctxt, (xmlSchemaValidityErrorFunc) fprintf,(xmlSchemaValidityWarningFunc) fprintf, stderr);
		rv = xmlSchemaValidateDoc(vctxt, doc);
		xmlSchemaFreeValidCtxt(vctxt);
		xmlSchemaFree(schema);
	}
#endif

	// Check root element
	if (xmlStrcasecmp(cur->name, (const xmlChar *) "TASKS")) {
		ERROR("Error: document of the wrong type, root node != tasks\n");
		xmlFreeDoc(doc);
		return -1;
	}

	/*
	 * Now, walk the tree.
	 */
	/* First level we expect image */
	cur = cur->xmlChildrenNode;
	while(cur) {
		while ( cur && (xmlIsBlankNode(cur) || cur->type == XML_COMMENT_NODE) ) {
			cur = cur -> next;
		}
		if(!cur)
			break;
		if ((xmlStrcasecmp(cur->name, (const xmlChar *) "IMAGE"))) {
			ERROR("Error: document of the wrong type, was '%s', IMAGE expected\n",
					cur->name);
			xmlFreeDoc(doc);
			return -1;
		}

		rv = parse_image(cur);
		if(rv < 0)
			return rv;
		cur = cur->next;
	}

	return 0;
}

// Display command line arguments
void help(char *exe) {
	printf("Usage: %s [OPTION..] <FILE..>\n", exe);
	printf("Sign binary files according to XML script\n\n");
	printf("Options:\n");
	printf("  -q, --quiet        Suppress informational messages\n");
	printf("  -V, --version      Display program version and exit\n");
}

// Main function
int main(int argc, char **argv) {
	int i, rv;

	LIBXML_TEST_VERSION
	xmlKeepBlanksDefault(0);

	memset(hash_len, 0, sizeof(hash_len));
	memset(saved_addr, 0, sizeof(saved_addr));
	quiet = 0;

	if(argc > 1) {
		if(argv[1][0] == '-') {
			if(!strcmp(argv[1], "-V") || !strcmp(argv[1], "--version")) {
				printf("m7i_sign.exe version " VERSION "\n");
				exit(0);

			} else if(!strcmp(argv[1], "-q") || !strcmp(argv[1], "--quiet")) {
				quiet = 1;
				argc--;
				argv++;

			} else {
				help(argv[0]);
				exit(EXIT_FAILURE);
			}
		}
	}

	/* register prng/hash */
	if (register_prng(&sprng_desc) == -1) {
		printf("Error registering sprng");
		return EXIT_FAILURE;
	}
	/* register a math library (in this case TomMath) */
	//ltc_mp = tfm_desc;
	ltc_mp = ltm_desc;
	if (register_hash(&sha1_desc) == -1) {
	   ERROR("Error registering sha1");
	   return EXIT_FAILURE;
	}
	if (register_hash(&sha256_desc) == -1) {
	   ERROR("Error registering sha256");
	   return EXIT_FAILURE;
	}
	hash_idx = find_hash("sha1");
	prng_idx = find_prng("sprng");

	for (i = 1; i < argc ; i++) {
		rv = parse_config(argv[i]);
		if(rv < 0)
			ERROR("Error processing file '%s'\n", argv[i]);

	}

	/* Clean up everything else before quitting. */
	xmlCleanupParser();

	return(0);
}

