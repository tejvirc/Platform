#ifndef __ALI_COMPAT_H__
#define __ALI_COMPAT_H__


#if defined(_WIN32) || defined(_WIN64) 
#define _DD_ "\\"
#define _DDC_ '\\'
#define snprintf _snprintf 
#define vsnprintf _vsnprintf 
#define strcasecmp _stricmp 
#define strncasecmp _strnicmp 
#define STAT _stat64
#define strdup _strdup
#else
#define DD "/"
#define DDC '/'
#define STAT lstat64
#endif


#endif // __ALI_COMPAT_H__