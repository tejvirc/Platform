#pragma once

#include <map>
#include <string>

template <class T, typename ...Args>
class NameClassFactory
{
    class IFactory { public: virtual ~IFactory() {}; virtual T* create(Args...) = 0;};

    template<class F>
    class FactoryCtor : public IFactory
    {
        friend class NameClassFactory;
        virtual ~FactoryCtor()
        {
        }
        template<typename... Args>
        T* create(Args...args)
        {
            return new F(args);
        }
    };

    template<class F>
    class FactoryFunc : public IFactory
    {
        friend class NameClassFactory;
        virtual ~FactoryFunc()
        {
        }
        std::function<T*(Args...)> _f;
        FactoryFunc(std::function<T*(Args...)> f) 
        {
             _f = f;
        }
        
        T* create(Args...args)
        {
            return _f(args...);
        }
        
    };
    
public:
    virtual ~NameClassFactory()
    {
        std::for_each(_factories.begin(),_factories.end(), [](auto f)
        {
            delete f.second;
        });
    }
    static NameClassFactory<T, Args...>& Instance() {
        static NameClassFactory<T, Args...> factory;
        return factory;
    }
    template<class F>
    void Register()
    {
        _factories[F::name()] = new FactoryCtor<F>();
    }

    template<class F>
    void Register(const char * name)
    {
        _factories[name] = new FactoryCtor<F>();
    }


    template<class F>
    void Register(const char * name, std::function<F*(Args...)> f)
    {
        _factories[name] = new FactoryFunc<F>(f);
    }

    template<class F>
    void Register(std::function<F*(Args...)> f)
    {
        _factories[F::name()] = new FactoryFunc<F>(f);
    }

    
    T* create(const char* name,Args...args)
    {
        auto f = _factories.find(name);
        if (f == _factories.end())
            return nullptr;
        return f->second->create(args...);
    }

    std::map<std::string, IFactory*> _factories;
};