#pragma once

#include <vector>
#include <algorithm>
#include <json/json.h>
#include <fstream>


class ReferenceMap
{
    static std::atomic<int>    ref_count;                 // cpp headeronly hack
    ReferenceMap()
    {
        if (ref_count.fetch_add(1,std::memory_order_relaxed) == 0)
        {
            _singleton = this;
        }
    }

    virtual ~ReferenceMap()
    {
        if (ref_count.fetch_sub(1, std::memory_order_relaxed) == 1)
        {
            _singleton = nullptr;
        }
    }

public:

    void Release()
    {
        delete this;
    }

    static ReferenceMap* from_json(const Json::Value& config)
    {
        Destroy();
        ReferenceMap* ptr = new ReferenceMap();

        // applications node
        auto references       = config["references"];

        for (auto r : references)
        {
            auto refitem = r.begin();
            auto val = refitem.key();
            ptr->_refmap[val] = r[refitem.name()];
        }
        
        _singleton = ptr;
        return ptr;
    }

    static  Json::Value Get(const Json::Value & val)
    {
        static Json::Value _empty;
        return _singleton->GetVal(val,_empty);
    }

    static  Json::Value Get(const char* name)
    {
        static Json::Value _empty;
        Json::Value  val = name;
        return _singleton->GetVal(val,_empty);
    }

    static Json::Value Get(const Json::Value & parent, const char* name, const Json::Value & defaultValue)
    {
        const Json::Value fallback = parent.get(name, defaultValue);
        if (fallback == defaultValue) return defaultValue;
        return _singleton->GetVal(fallback, fallback);
    }
    static void Destroy()
    {
        if (_singleton)
        {
            _singleton->Release();
            _singleton = nullptr;
        }
    }
private:
     Json::Value GetVal(const Json::Value & lookupValue, const Json::Value & defaultValue)
    {
        
        auto it = _refmap.find(lookupValue);
        if (it != _refmap.end())
            return it->second;
        return defaultValue;
    }

private:
    typedef std::map<Json::Value, Json::Value> RefMap;
    RefMap _refmap;
    static ReferenceMap*        _singleton; // yes i know.. 

    
};

ReferenceMap* ReferenceMap::_singleton = nullptr;
std::atomic<int>    ReferenceMap::ref_count = 0;