#ifndef Monaco_InterfaceID_h
#define Monaco_InterfaceID_h

#include <stdint.h>
#include <memory>

namespace aristocrat
{
    class InterfaceID
    {
    public:
        //------------------------------------------
        // Constructors/Destructor
        //------------------------------------------
        InterfaceID() :
            DataA(0),
            DataB(0)
        {
            
        }

        InterfaceID(const InterfaceID& value) :
            DataA(value.DataA),
            DataB(value.DataB)
        {
            
        }

        InterfaceID(uint32_t v1,
                    uint16_t v2,
                    uint16_t v3,
                    uint8_t v4,
                    uint8_t v5,
                    uint8_t v6,
                    uint8_t v7,
                    uint8_t v8,
                    uint8_t v9,
                    uint8_t v10,
                    uint8_t v11) :
            Data1(v1),
            Data2(v2),
            Data3(v3)
        {
            Data4[0] = v4;
            Data4[1] = v5;
            Data4[2] = v6;
            Data4[3] = v7;
            Data4[4] = v8;
            Data4[5] = v9;
            Data4[6] = v10;
            Data4[7] = v11;
        }

        //------------------------------------------
        // Assignment operator
        //------------------------------------------
        inline const InterfaceID& operator=(const InterfaceID& value)
        {
            memcpy(this, &value, sizeof(InterfaceID));
            return (*this);
        }

        //------------------------------------------
        // Comparison operators
        //------------------------------------------
        inline bool operator==(const InterfaceID& value) const
        {
            return (0 == memcmp(this, &value, sizeof(InterfaceID)));
        }

        inline bool operator!=(const InterfaceID& value) const
        {
            return (0 != memcmp(this, &value, sizeof(InterfaceID)));
        }
        
        inline bool operator <(const InterfaceID& rhs) const
        {
            if (DataA < rhs.DataA) return true;
            if (DataA > rhs.DataA) return false;

            if (DataB < rhs.DataB) return true;
            if (DataB > rhs.DataB) return false;

            return false;
        }

    private:

#pragma warning(push)
#pragma warning(disable: 4201) //nonstandard extension used : nameless struct / union
        union
        {
            struct
            {
                uint32_t Data1;
                uint16_t Data2;
                uint16_t Data3;
            };
            uint64_t DataA;
        };
        union
        {
            struct
            {
                uint8_t Data4[8];
            };
            uint64_t DataB;
        };
    };
#pragma warning(pop)
};


#endif // Monaco_InterfaceID_h
