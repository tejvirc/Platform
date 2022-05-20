#pragma once
#include <platform.h>
#include <map>
#include "circularbuffer.h"
#include <atomic>

class StreamManager
{
private:
    static std::atomic<int>    stream_ref_count;                 // cpp headeronly hack
    StreamManager()
    {
        if (stream_ref_count.fetch_add(1,std::memory_order_relaxed) == 0)
        {
            _singleton = this;
        }
    }

    virtual ~StreamManager()
    {
        if (stream_ref_count.fetch_sub(1, std::memory_order_relaxed) == 1)
        {
            _singleton = nullptr;
        }
    }
public:
    typedef std::circular_buffer<std::string> StreamBuffer;
    typedef std::map<std::string, StreamBuffer> StreamMap;

    static StreamManager* Create()
    {
        
        StreamManager* sm = new StreamManager();
        
        return sm;
    }
    void Release()
    {
        delete this;
    }
    void Add(const char* name, const char* text)
    {
        _streams[name].push_back(text);
        _is_dirty = true;
    }

    void ReplaceLast(const char* name, const char* text)
    {
        if (_streams[name].size() > 0)
            _streams[name].back() = text;
        else
            _streams[name].push_back(text);

        _is_dirty = true;
    }
    const std::string& Last(const char* name)
    {
        static std::string null_string;
        if (_streams[name].size() > 0)
            return _streams[name].back();
        
        return null_string;
    }
    
    void Clear(std::string name)
    {
         _streams[name].clear();
    }

    void AddStream(std::string name, int capacity = 100)
    {
        if (_streams.find(name) == _streams.end())
            _streams[name] = std::circular_buffer<std::string>(capacity);
    }
    
    StreamMap::const_iterator begin() { return _streams.begin(); }
    StreamMap::const_iterator end() { return _streams.end(); }
    size_t size() { return _streams.size(); }

    StreamBuffer::iterator begin(const char* name) { return _streams[name].begin(); }
    StreamBuffer::iterator end(const char* name) { return _streams[name].end(); }
    size_t size(const char* name) { return _streams[name].size(); }

    static StreamManager* Instance() { return _singleton; }
    bool        isDirty() {  if (_is_dirty) { _is_dirty = false; return true;} return false;}
private:
    bool _is_dirty = false;
    StreamMap _streams;
    static StreamManager* _singleton;
};

StreamManager* StreamManager::_singleton = nullptr;
std::atomic<int>    StreamManager::stream_ref_count = 0;