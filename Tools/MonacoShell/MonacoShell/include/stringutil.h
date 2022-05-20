#pragma once
#include <windows.h>
#include <string>
#include <vector>
#include <algorithm>

namespace aristocrat
{
  
    static inline const std::wstring MultiByte2WideCharacterString(const char* value)
    {
        static std::vector<wchar_t> wcs;
        if (value == nullptr)
        {
            wcs = std::vector<wchar_t>();
            return std::wstring();
        }
        wcs.resize(512);
        size_t len = strlen(value) + 1; // include nullterm
        if (len > wcs.size())
           wcs.resize(len);
        std::mbstate_t state = std::mbstate_t();
        size_t num_characters_converted = 0;;
        mbsrtowcs_s(&num_characters_converted, wcs.data(),wcs.size(), &value, len, &state);
        return wcs.data();
    }

    /* not thread safe */
    static inline const std::string WideCharacter2MultiByteString(const wchar_t* value)
    {
        static std::vector<char> mbs;
        if (value == nullptr)
        {
            mbs = std::vector<char>();
            return std::string();
        }
        mbs.reserve(512);
        size_t len = wcslen(value) + 1; // include nullterm
        if (len > mbs.size())
            mbs.resize(len);
        std::mbstate_t state = std::mbstate_t();
        wcsrtombs(mbs.data(), &value, len, &state);
        return mbs.data();
    }

    static inline const std::wstring MultiByte2WideCharacterString(const std::string& value)
    {
        return MultiByte2WideCharacterString((const char*)value.c_str());
    }

    static inline const std::string WideCharacter2MultiByteString(const std::wstring& value)
    {
        return WideCharacter2MultiByteString((const wchar_t*)value.c_str());
    }

    
#ifdef EASTL_STRING_H
    static inline const std::wstring MultiByte2WideCharacterString(const eastl::string& value)
    {
        return MultiByte2WideCharacterString((const char*)value.c_str());
    }
    static inline const std::string WideCharacter2MultiByteString(const eastl::wstring& value)
    {
        return WideCharacter2MultiByteString((const wchar_t*)value.c_str());
    }
#endif

    class StringUtils
    {
    public:

        template<class string_class = std::string>
        static inline std::vector<string_class> StringTokenize(const string_class& str, const string_class& delimiters)
        {
            std::vector<string_class> tokens;

            // skip delimiters at beginning.
            typename string_class::size_type lastPos = str.find_first_not_of(delimiters, 0);

            // find first "non-delimiter".
            typename string_class::size_type pos = str.find_first_of(delimiters, lastPos);

            while (string_class::npos != pos || string_class::npos != lastPos)
            {
                // found a token, add it to the vector.
                tokens.push_back(str.substr(lastPos, pos - lastPos));

                // skip delimiters.  Note the "not_of"
                lastPos = str.find_first_not_of(delimiters, pos);

                // find next "non-delimiter"
                pos = str.find_first_of(delimiters, lastPos);
            }

            return tokens;
        }

        template<class string_class>
        static inline void StringTokenize(const string_class& str, const string_class& delimiters, std::vector<string_class>& tokens)
        {
            // skip delimiters at beginning.
            typename string_class::size_type lastPos = str.find_first_not_of(delimiters, 0);

            // find first "non-delimiter".
            typename string_class::size_type pos = str.find_first_of(delimiters, lastPos);

            while (string_class::npos != pos || string_class::npos != lastPos)
            {
                // found a token, add it to the vector.
                tokens.push_back(str.substr(lastPos, pos - lastPos));

                // skip delimiters.  Note the "not_of"
                lastPos = str.find_first_not_of(delimiters, pos);

                // find next "non-delimiter"
                pos = str.find_first_of(delimiters, lastPos);
            }
        }


        static inline bool is_base64(unsigned char c) {
            return (isalnum(c) || (c == '+') || (c == '/'));
        }



        static std::string base64_encode(const char* bytes_to_encode, unsigned int in_len) {
            std::string ret;
            int i = 0;
            int j = 0;
            unsigned char char_array_3[3];
            unsigned char char_array_4[4];
            const std::string base64_chars =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                "abcdefghijklmnopqrstuvwxyz"
                "0123456789+/";

            while (in_len--) {
                char_array_3[i++] = *(bytes_to_encode++);
                if (i == 3) {
                    char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
                    char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
                    char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
                    char_array_4[3] = char_array_3[2] & 0x3f;

                    for (i = 0; (i < 4); i++)
                        ret += base64_chars[char_array_4[i]];
                    i = 0;
                }
            }

            if (i)
            {
                for (j = i; j < 3; j++)
                    char_array_3[j] = '\0';

                char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
                char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
                char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
                char_array_4[3] = char_array_3[2] & 0x3f;

                for (j = 0; (j < i + 1); j++)
                    ret += base64_chars[char_array_4[j]];

                while ((i++ < 3))
                    ret += '=';

            }

            return ret;

        }

        static std::string base64_encode(const std::vector<char>& bytes_to_encode, unsigned int in_len) {
            return base64_encode(&bytes_to_encode[0], in_len);
        }

        static void base64_decode(const std::string & encoded_string, std::vector<char>& outdata) {
            size_t in_len = encoded_string.size();
            int i = 0;
            int j = 0;
            int in_ = 0;
            byte char_array_4[4], char_array_3[3];
            const std::string base64_chars =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                "abcdefghijklmnopqrstuvwxyz"
                "0123456789+/";


            while (in_len-- && (encoded_string[in_] != '=') && is_base64(encoded_string[in_])) {
                char_array_4[i++] = encoded_string[in_]; in_++;
                if (i == 4) {
                    for (i = 0; i < 4; i++)
                        char_array_4[i] = (byte)base64_chars.find(char_array_4[i]);

                    char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
                    char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
                    char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

                    for (i = 0; (i < 3); i++)
                        outdata.push_back(char_array_3[i]);
                    i = 0;
                }
            }

            if (i) {
                for (j = i; j < 4; j++)
                    char_array_4[j] = 0;

                for (j = 0; j < 4; j++)
                    char_array_4[j] = (byte)base64_chars.find(char_array_4[j]);

                char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
                char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
                char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

                for (j = 0; (j < i - 1); j++) outdata.push_back(char_array_3[j]);
            }
        }
    };

    bool icasecmp(const std::string& l, const std::string& r)
    {
        return l.size() == r.size()
            && std::equal(l.cbegin(), l.cend(), r.cbegin(),
                [](std::string::value_type l1, std::string::value_type r1)
                    { return toupper(l1) == toupper(r1); });
    }

    bool icasecmp(const std::wstring& l, const std::wstring& r)
    {
        return l.size() == r.size()
            && equal(l.cbegin(), l.cend(), r.cbegin(),
                [](std::wstring::value_type l1, std::wstring::value_type r1)
                    { return towupper(l1) == towupper(r1); });
    }
}