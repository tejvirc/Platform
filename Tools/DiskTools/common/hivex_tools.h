#ifndef __HIVEX_TOOLS_H__
#define __HIVEX_TOOLS_H__

#ifndef HIVEX_H_
#include "hivex.h" /* or else make sure hivex.h is included before this tools */
#endif

typedef void (process_hash_func)(void *ctx, const unsigned char *in, unsigned long inlen);

void hashnode(process_hash_func hasher, void *ctx, hive_h* hive, hive_node_h node);


void hashnode(process_hash_func hasher, void *ctx, hive_h* hive, hive_node_h node)
{
	int i = 0;
	int s = 0;
	int c = 0;

	char* name = hivex_node_name(hive, node);
	hasher(ctx, (unsigned char *)name, hivex_strlen(name));
	hivex_free(name);

	hive_value_h* values = hivex_node_values(hive, node);
	for (i = 0; values[i] != 0; ++i)
	{
		hive_value_h value = values[i];
		char* kname = hivex_value_key(hive, value);
		hive_type type = hive_t_REG_NONE;
		hivex_size_t vlen;
		hivex_value_type(hive, value, &type, &vlen);
		hasher(ctx, (unsigned char *)kname, hivex_strlen(kname));
		hasher(ctx, (unsigned char *)&vlen, sizeof(vlen));
		hivex_free(kname);
		char* str = 0;
		switch (type)
		{
		case hive_t_REG_SZ:
			str = hivex_value_string(hive, value);
			hasher(ctx, (unsigned char *)str, hivex_strlen(str));
			hivex_free(str);
			str = 0;
			break;
		case hive_t_REG_NONE:
			break;
		case hive_t_REG_EXPAND_SZ:
			str = hivex_value_string(hive, value);
			hasher(ctx, (unsigned char *)str, hivex_strlen(str));
			hivex_free(str);
			str = 0;
			break;
		case hive_t_REG_DWORD:
		{
			int val = hivex_value_dword(hive, value);
			hasher(ctx, (unsigned char *)&val, sizeof(val));
		}break;
		case hive_t_REG_DWORD_BIG_ENDIAN:
		{
			int val = hivex_value_dword(hive, value);
			hasher(ctx, (unsigned char *)&val, sizeof(val));
		}	break;

		case hive_t_REG_QWORD:
		{
			hivex_uint64_t val = hivex_value_qword(hive, value);
			hasher(ctx, (unsigned char *)&val, sizeof(val));
		}break;
		case hive_t_REG_LINK:
			break;
		case hive_t_REG_MULTI_SZ:
		{
			char** strings = hivex_value_multiple_strings(hive, value);

			for (s = 0; strings[s] != 0; ++s)
			{
				str = strings[s];
				hasher(ctx, (unsigned char *)str, hivex_strlen(str));
			}
			hivex_free_strings(strings);
			strings = NULL;
		}
		case hive_t_REG_RESOURCE_LIST:
			break;
		case hive_t_REG_BINARY:
		{
			hive_type t2;
			size_t len;
			char* bin = hivex_value_value(hive, value, &t2, &len);
			hasher(ctx, (unsigned char *)bin, len);
			hivex_free(bin);
		} break;
		case hive_t_REG_FULL_RESOURCE_DESCRIPTOR:
		case hive_t_REG_RESOURCE_REQUIREMENTS_LIST:
			// do nothing
			break;


		}
	}
	hivex_free(values);
	values = NULL;
	hive_node_h* children = hivex_node_children(hive, node);
	for (c = 0; children[c] != 0; ++c)
	{
		hashnode(hasher, ctx, hive, children[c]);
	}
	hivex_free(children);
	children = NULL;
}
#endif // __HIVEX_TOOLS_H__
