#pragma once



#include "targetver.h"

// #define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <tlhelp32.h>
#include <shlwapi.h>
#include <ObjIdl.h>
#include <gdiplus.h>
#include <interface.h>
#include <interfaceid.h>
#pragma comment (lib,"Gdiplus.lib")


#ifdef TEST_FOR_MEMORY_LEAKS
    #include <stdlib.h>
    #include <crtdbg.h>
#else

#endif

// C RunTime Header Files
#include <stdlib.h>

#include <malloc.h>
#include <memory.h>


