// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <Windows.h>
#include <stdio.h>
#include <tchar.h>

#define SAFE_FREE(ptr) if( (ptr) != 0 ) { free (ptr); (ptr) = 0; }