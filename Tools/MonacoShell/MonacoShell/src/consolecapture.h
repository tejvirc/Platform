#pragma once

#include <windows.h> 
#include <tchar.h> 
#include <strsafe.h>
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <stdexcept>
#include <sstream>
#include <memory>
#include <iterator>
#include "logger.h"
#include "circularbuffer.h"
#include <functional>

namespace utils
{
    class ConsoleCapture
    {
    public:
        ConsoleCapture(int capture_num_lines_buffer = 100) : _captured_lines(capture_num_lines_buffer)
        {
            SECURITY_ATTRIBUTES saAttr;

            //
            // Set the bInheritHandle flag so pipe handles are inherited. 
            //

            saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
            saAttr.bInheritHandle = TRUE;
            saAttr.lpSecurityDescriptor = NULL;

            std::stringstream ss;

            if (!CreatePipe(&m_hChildStd_OUT_Rd, &m_hChildStd_OUT_Wr, &saAttr, 0))
            {
                ss << "ERROR: CreatePipe failed. File: " << __FILE__ << ", Line: " << __LINE__;

                throw std::runtime_error(ss.str().c_str());
            }

            //
            // Ensure the read handle to the pipe for STDOUT is not inherited.
            //

            if (!SetHandleInformation(m_hChildStd_OUT_Rd, HANDLE_FLAG_INHERIT, 0)) //the functionality of interest
            {
                ss << "ERROR: Stdout SetHandleInformation failed. File: " << __FILE__ << ", Line: " << __LINE__;

                throw std::runtime_error(ss.str().c_str());
            }
        }

        virtual ~ConsoleCapture(void)
        {
            CloseHandle(m_hChildStd_OUT_Rd);
            CloseHandle(m_hChildStd_OUT_Wr);
        }

        void configure(STARTUPINFOW& startupInfo)
        {
            startupInfo.hStdError = m_hChildStd_OUT_Wr;
            startupInfo.hStdOutput = m_hChildStd_OUT_Wr;
            startupInfo.dwFlags |= STARTF_USESTDHANDLES;
        }

        void configure(STARTUPINFOA& startupInfo)
        {
            startupInfo.hStdError = m_hChildStd_OUT_Wr;
            startupInfo.hStdOutput = m_hChildStd_OUT_Wr;
            startupInfo.dwFlags |= STARTF_USESTDHANDLES;
        }

        DWORD Read(char* buffer, DWORD bufferSize, DWORD* numBytesRead)
        {
            //DWORD dwRead;
            DWORD dwTotal{ 0 };
            BOOL bSuccess = FALSE;
            HANDLE hParentStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            DWORD numBytesRemaining = 0;

            PeekNamedPipe(m_hChildStd_OUT_Rd, NULL, 0, NULL, &numBytesRemaining, NULL);

            if (numBytesRemaining == 0 || buffer == nullptr)  
                return numBytesRemaining;
            
            bSuccess = ReadFile(m_hChildStd_OUT_Rd, buffer, bufferSize, numBytesRead, NULL);

            if (!bSuccess || *numBytesRead == 0)
            {
                std::stringstream ss;
                ss << "ERROR: Read from Pipe failed (bytes read so far: " << numBytesRemaining << "). File: " << __FILE__ << ", Line: " << __LINE__;

                throw std::runtime_error(ss.str().c_str());
            }

            BOOL bPeek = PeekNamedPipe(m_hChildStd_OUT_Rd, NULL, 0, NULL, &numBytesRemaining, NULL);
            if (!bPeek)
            {
                DWORD dErr = GetLastError();
                std::cerr << "ERROR: PeekPipe: " << dErr << "\n";
            }

            return numBytesRemaining;
        }

        DWORD ProcessCapture()
        {
            DWORD numbytes  = 0;
            DWORD bytes     = 0;
            numbytes        = Read(NULL,0,NULL);

            if (numbytes <= 0) 
                return 0;

            char buffer[255+1]  = {0};
            Read(buffer,255,&bytes);
            buffer[bytes]       = 0; // always nullterminate

            if (strlen(buffer) > 0)
            {
                if (_callback_stream)
                    _callback_stream(buffer);
            }

            char* pos,* beg;
            beg = pos = buffer;
            char* end = &buffer[bytes];

            int64_t length = 0;

            while(pos != end) // Parse Console ASCII
            {
                length = pos-beg;

                switch(*pos)
                {
                    case '\b':
                    {
                        *pos = 0;

                        if (length > 0)
                            AppendToCaret(beg,length);

                        if (_last_line_caret > 0)
                            _last_line_caret--;

                        beg = pos + 1;
                    } break;
                    case '\r':
                    {
                        *pos = 0;

                        if (length > 0)
                            AppendToCaret(beg,length);

                        _last_line_caret = 0;
                        beg = pos + 1;
                    } break;
                    case '\n':
                    {
                        *pos = 0;

                        if (length >= 0)
                            AppendToCaret(beg,length?length-1:0, true);

                        _last_line_caret = 0;
                        beg = pos + 1;
                    } break;
                }

                ++pos;
            }

            if (length > 0) // residual buffer
            {
               AppendToCaret(beg, length);
            }

            return Read(NULL,0,NULL);
        }

        DWORD bytes_left(void) const
        {
            DWORD numBytesRemaining = 0;
            PeekNamedPipe(m_hChildStd_OUT_Rd, NULL, 0, NULL, &numBytesRemaining, NULL);

            return numBytesRemaining;
        }

        const std::string& last_line()
        {
            static std::string null_line;

            if (_captured_lines.empty())
                return null_line;

            if (!_captured_lines.back().empty())     // last line is still active
                return _captured_lines.back();

            if (_captured_lines.size() > 1)          // last line is empty, second to last was modified
            {
                auto second_to_last = _captured_lines.begin();
                std::advance(second_to_last, _captured_lines.size()-2);

                return *second_to_last;
            }

            return null_line;
        }

    private:
        void AppendToCaret(const char* text, size_t length, bool newLine = false)
        {
            if (length == 0 && newLine == false)
                return;

            if (_captured_lines.empty() || newLine) // Add new line
            {
                _captured_lines.push_back(text);
                _last_line_caret = _captured_lines.back().length();
            }
            else if (_last_line_caret == _captured_lines.back().size()) // append to existing line
            {
                _captured_lines.back().append(text, length);
                _last_line_caret = _captured_lines.back().length();
            }
            else // insert to existing line
            {
                size_t new_length = length + _last_line_caret;
                if (new_length > _captured_lines.back().length())    // need realloc
                {
                    std::string s;
                    s.resize(new_length);
                    s.append(_captured_lines.back());

                    for (int i = 0; i < length;++i)
                        s[_last_line_caret+i] = *(text+i);

                    _captured_lines.back() = s;
                    _last_line_caret += length;
                }
                else  // fits, just insert
                {
                    for (int i = 0; i < length;++i)
                        _captured_lines.back()[_last_line_caret+i] = *(text+i);

                    _last_line_caret += length;
                }
            }

            //
            // callbacks
            // any change
            //

            if (_callback_on_any && strlen(text) > 0) 
                _callback_on_any(last_line().c_str());

            //
            // new line
            //

            if (newLine && _callback_on_new_line)
                _callback_on_new_line(last_line().c_str());    
        }

    public: // events
        //
        // fires callback when a new line is added
        //

        void callback_on_new_line(std::function<void(const char*)> f) { _callback_on_new_line = f;}

        //
        // fires callback when last line is added or updated
        //

        void callback_on_any(std::function<void(const char*)> f) { _callback_on_any = f;}

        //
        // fires callback with all data read from pipe
        //

        void callback_stream(std::function<void(const char*)> f) { _callback_on_any = f;}

    private:
        HANDLE m_hChildStd_OUT_Rd;
        HANDLE m_hChildStd_OUT_Wr;
        std::circular_buffer<std::string> _captured_lines;
        size_t                            _last_line_caret = 0;

        std::function<void(const char*)>  _callback_on_new_line;
        std::function<void(const char*)>  _callback_on_any;
        std::function<void(const char*)>  _callback_stream;        
    };
}