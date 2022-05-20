
#ifndef Monaco_Interface_h
#define Monaco_Interface_h

    #include <interfaceid.h>
    #include <cassert>

    #define MONACO_SAFE_RELEASE(p) if ((p) != nullptr) {(p)->Release();p=nullptr;}

    namespace aristocrat
    {
        class Interface
        {
        public:
            //------------------------------------------
            // Constructors/Destructor
            //------------------------------------------
            virtual ~Interface(){};
        
            virtual bool QueryInterface(const InterfaceID& id, Interface** p) = 0;
            virtual bool Implements(const InterfaceID& id) = 0;
            template<typename Type> bool QueryInterface(Type** q)
            {
                Interface* pInterface;
                if (QueryInterface(Type::ID(), &pInterface))
                {
                    *q = dynamic_cast<Type*>(pInterface);
                    return true;
                }
                return false;
            }
            virtual const char* TypeName() = 0;
                        // Optional Interface enumerator, allowing iteration of proxied interfaces
            virtual Interface* Next(const InterfaceID&)
            {
                return nullptr;
            }
        };

        class Referenced 
        {
            public:
            Referenced() : _ref_count(1)
            {

            }
            //------------------------------------------
            // Constructors/Destructor
            //------------------------------------------
            virtual ~Referenced()
            {
                assert(_ref_count == 0);
            };
        public:
            virtual void AddRef()
            {

                ++_ref_count;
            }
            virtual void Release()
            {
                assert(_ref_count != 0);

                _ref_count--;

                if (_ref_count == 0)
                {
                    delete this;
                }
            }

            virtual uint32_t RefCount()
            {
                return _ref_count;
            }
        private:
            volatile uint32_t _ref_count;
        };
        
        class RefInterface : public virtual Interface ,public virtual Referenced        
        {
        protected:
            virtual ~RefInterface() {}
        };


        // Required by each unique plugin interface type
#define MONACO_INTERFACE_ID(name, l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8) \
    typedef struct IID_ ## name \
    { \
        static const char* Name() { return  ## # name;} \
        static const aristocrat::InterfaceID& ID() { static aristocrat::InterfaceID iid(l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8);return iid;} \
    } InterfaceDesc; \
    static const aristocrat::InterfaceID& ID() { return InterfaceDesc::ID(); } 
    
    
    


        // Start plugin interface mapping
#define MONACO_BEGIN_INTERFACE_MAP(cls) \
    const char* TypeName() { return #cls; } \
    static const char* ClassName() { return #cls; } \
    bool Implements(const aristocrat::InterfaceID& iid) { aristocrat::Interface* pTemp = nullptr; return QueryInterface(iid, &pTemp);} \
    bool QueryInterface(const aristocrat::InterfaceID & iid, aristocrat::Interface * *p) \
    { \
        if (p == NULL) { return false;}


        // Addition of an interface
#define MONACO_INTERFACE_ENTRY(itf) \
    if (iid == itf::InterfaceDesc::ID()) \
    { \
        *p = static_cast<itf*>(this); \
        return true; \
    }

        // Addition of a derived class that contains queryable interfaces
#define MONACO_INTERFACE_DERIVED_ENTRY(dclass) \
    if (dclass::QueryInterface(iid, p)) \
    { return true;}

        // Addition of a member variable that implements an interface
#define MONACO_MEMBER_INTERFACE_ENTRY(itf, mem) \
    if (iid == itf::InterfaceDesc::ID()) \
    { \
        *p = static_cast<itf*>(&mem); \
        return true; \
    }

        // Addition of a member variable that implements an interface
#define MONACO_FORWARD_INTERFACE_ENTRY(func) \
    if (func(iid,p)) \
    {                \
        return true; \
    }
        // Addition of a member pointer that implements an interface
#define MONACO_MEMBERPTR_INTERFACE_ENTRY(itf, mptr) \
    if (mptr && (iid == itf::InterfaceDesc::ID()) \
       { \
           *p = static_cast<itf*>(mptr); \
           return true; \
       }

        // End plugin interface mapping
#define MONACO_END_INTERFACE_MAP \
    * p = NULL; \
    return false; \
    };
    

}

#endif // MONACO_Interface_h