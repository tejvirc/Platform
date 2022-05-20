
// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN            // Exclude rarely-used stuff from Windows headers
#endif

#include "targetver.h"
#include <Windows.h>
#include <iostream>
#include <tchar.h>

#define SAFE_DELETE(ptr) if( (ptr) != nullptr ) { delete (ptr); (ptr) = nullptr; }
#define SAFE_ARRAY_DELETE(ptr) if( (ptr) != nullptr ) { delete[] (ptr); (ptr) = nullptr; }