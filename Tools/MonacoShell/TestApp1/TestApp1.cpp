// TestApp1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>
#include <Windows.h>
int main()
{
    std::cout << "Hello World!" << std::endl;

    char strBarChunk[4] = { 0 };
    COPYDATASTRUCT cdsChunk = { 0 };
    cdsChunk.dwData = 0;
    cdsChunk.cbData = 4;
    cdsChunk.lpData = strBarChunk;

    char strBarSmooth[4] = { 0 };
    COPYDATASTRUCT cdsSmooth = { 0 };
    cdsSmooth.dwData = 1;
    cdsSmooth.cbData = 4;
    cdsSmooth.lpData = strBarSmooth;

    HWND hwnd = FindWindow("ShellWindow", NULL);
    if (hwnd == NULL)
    {
        std::cout << "FindWindow failed to find window of Class 'ShellWindow'" << std::endl << std::flush;
    }

    // Delay to view that progress bars are invisible before first update
    for (int x = 0; x < 3; ++x)
    {
        std::cout << "Working..." << std::endl << std::flush;
        ::Sleep(200);
    }
        
    sprintf_s(strBarChunk, 4, "0");
    SendMessage(hwnd, WM_COPYDATA, (WPARAM)hwnd, (LPARAM)(LPVOID)&cdsChunk);
    for (int x = 1; x < 5; ++x)
    {
        std::cout << "Loading module #" << x << " of 4..." << std::endl << std::flush;

        sprintf_s(strBarSmooth, 4, "0");
        SendMessage(hwnd, WM_COPYDATA, (WPARAM)0, (LPARAM)(LPVOID)&cdsSmooth);
        for(int i = 1 ; i < 101; ++i)
        {
            sprintf_s(strBarSmooth, 4, "%d", i);
            //cdsSmooth.cbData = DWORD(sizeof(char) * (strlen(strBarSmooth) + 1));
            //std::cout << "Smooth: " << i << "%" << std::endl << std::flush;
            SendMessage(hwnd, WM_COPYDATA, (WPARAM)0, (LPARAM)(LPVOID)&cdsSmooth);
        }

        sprintf_s(strBarChunk, 4, "%d", x * 25);
        //cdsChunk.cbData = DWORD(sizeof(char) * (strlen(strBarChunk) + 1));
        //std::cout << "Chunk: " << x << "%" << std::endl << std::flush;
        SendMessage(hwnd, WM_COPYDATA, (WPARAM)0, (LPARAM)(LPVOID)&cdsChunk);
        ::Sleep(100);
    }
    std::cout << "Module loading complete." << std::endl << std::flush;
}
