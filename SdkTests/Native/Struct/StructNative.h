#include "StructNativeConstants.h"

#include <stdint.h>

#if UINTPTR_MAX == UINT64_MAX
#    define STRUCT_PLATFORM_64 1
#elif UINTPTR_MAX == UINT32_MAX
#    define STRUCT_PLATFORM_32 1
#else
#    pragma error Unexpected value of UINTPTR_MAX
#endif

#define STRUCTLIB_API __declspec(dllexport)
#define STRUCTLIB_FUNC(RET) extern "C" __declspec(dllexport) RET __stdcall

typedef enum _MF_ATTRIBUTE_TYPE
{
	MF_ATTRIBUTE_UINT32 = 19,
	MF_ATTRIBUTE_UINT64 = 21,
	MF_ATTRIBUTE_DOUBLE = 5,
	MF_ATTRIBUTE_GUID = 72,
	MF_ATTRIBUTE_STRING = 31,
	MF_ATTRIBUTE_BLOB = (0x1000 | 17),
	MF_ATTRIBUTE_IUNKNOWN = 13
} MF_ATTRIBUTE_TYPE;

typedef enum D2D1_DEVICE_CONTEXT_OPTIONS
{
	D2D1_DEVICE_CONTEXT_OPTIONS_NONE = 0,
	D2D1_DEVICE_CONTEXT_OPTIONS_ENABLE_MULTITHREADED_OPTIMIZATIONS = 1,
	D2D1_DEVICE_CONTEXT_OPTIONS_FORCE_DWORD = 0xffffffff
} D2D1_DEVICE_CONTEXT_OPTIONS;

enum CrShutterSpeedSet : unsigned int
{
	CrShutterSpeed_Bulb = 0x00000000,
	CrShutterSpeed_Nothing = 0xFFFFFFFF,
};

enum CrBatteryLevel : unsigned int
{
	CrBatteryLevel_PreEndBattery = 0x00000001,
	CrBatteryLevel_1_4,
	CrBatteryLevel_2_4,
	CrBatteryLevel_3_4,
	CrBatteryLevel_4_4,
	CrBatteryLevel_1_3,
	CrBatteryLevel_2_3,
	CrBatteryLevel_3_3,
	CrBatteryLevel_USBPowerSupply = 0x00010000,
	CrBatteryLevel_PreEnd_PowerSupply,
	CrBatteryLevel_1_4_PowerSupply,
	CrBatteryLevel_2_4_PowerSupply,
	CrBatteryLevel_3_4_PowerSupply,
	CrBatteryLevel_4_4_PowerSupply,
	CrBatteryLevel_Fake = 0xFFFFFFFD,
};

struct SimpleStruct
{
	int i;
	int j;
};

struct StructWithArray
{
	int i[3];
	double j;
};

struct StructWithPointer 
{
	const char* name;
};

struct StructInheritanceA: StructWithPointer
{
	int integer0;
};

struct StructInheritanceB : StructInheritanceA
{
	int integer1;
};

union TestUnion
{
	int integer;
	float decimal;
};

union UnionWithArray
{
	unsigned long long bigInt;
	unsigned int parts[2];
};

struct BitField
{
	int firstBit : 1;
	int lastBits : 31;
};

struct AsciiTest
{
	char SmallString[10];
	char* LargeString;
};

struct Utf16Test
{
	wchar_t SmallString[10];
	wchar_t* LargeString;
};

struct NestedTest
{
	AsciiTest Ascii;
	Utf16Test Utf;
};

struct BitField2
{
	short lowerBits : 4;
	short reservedBits : 8;
	short upperBits: 4;
};

struct BoolToInt
{
	int test;
};

struct BoolToInt2
{
	int test;
};

struct BoolArray
{
	bool elements[3];
};

struct CustomNativeNew {};

struct CustomNativeNewNested
{
	CustomNativeNew Nested;
};

struct Interface
{
	virtual int One() = 0;
};

struct StructWithInterface
{
	Interface* test;
};

struct StructWithDynamicArrayOfInterface
{
	Interface** ppInterfaces;
	int interfaceCount;
};

struct StructWithDynamicArrayOfIntegralType
{
	int* pElements;
	int elementCount;
};

struct StructWithDynamicArrayOfString
{
	const char* const* pElements;
	int  elementCount;
};

struct StructWithDynamicArrayOfPrimitiveStruct
{
	SimpleStruct* pStructs;
	int structCount;
};

struct StructWithDynamicArrayOfMarshaledStruct
{
	StructInheritanceB* pStructs;
	int structCount;
};

struct StructWithDynamicArrayRecursive
{
	StructWithDynamicArrayRecursive* pStructs;
	int structCount;
};

struct StructWithConstArrayOfInterface
{
	Interface* pInterfaces[8];
};

struct StructWithConstArrayOfIntegralType
{
	int elements[8];
};

struct StructWithConstArrayOfPrimitiveStruct
{
	SimpleStruct structs[8];
};

struct StructWithConstArrayOfMarshaledStruct
{
	StructInheritanceB structs[8];
};

struct StructWithPointerIntegralType 
{
	int* pElement;
};

struct StructWithPointerPrimitiveStruct 
{
	SimpleStruct* pElement;
};

struct StructWithPointerMarshaledStruct 
{
	StructInheritanceB* pElement;
};

struct StructInheritanceDiligent
{
	const char* name;
};

struct StructInheritanceDiligentA : StructInheritanceDiligent
{
	int integer0;
};

struct StructInheritanceDiligentB : StructInheritanceDiligentA
{
	int integer1;
};

struct StructWithCallback
{
	void (*ModifyCallback)(StructWithPointerIntegralType& item, void* pUserData);
	void* pUserData;
};

struct StructIntegralTypeDefaultValue
{
	int element = 1;
};

struct StructStringTypeDefaultValue
{
	const char* element = "Hello world";
};

struct StructArrayIntegralTypeDefaultValue0
{
	float elements[3]{ 0.0f, 1.0f, 2.0f };
};

struct StructArrayIntegralTypeDefaultValue1
{
	unsigned int elements[3]{ 0, 1, 2 };
};

struct StructArrayIntegralTypeDefaultValue2
{
	unsigned int elements[3] {};
};

struct StructEnumTypeDefaultValue
{
	D2D1_DEVICE_CONTEXT_OPTIONS element = D2D1_DEVICE_CONTEXT_OPTIONS_ENABLE_MULTITHREADED_OPTIMIZATIONS;
};

struct StructComplexTypeDefaultValue0
{
	StructArrayIntegralTypeDefaultValue0 element{};
};

struct StructComplexTypeDefaultValue1
{
	StructEnumTypeDefaultValue element[8]{};
};

struct StructWithConstantIdentifierDefaultValue
{
	int element = MAX_LAYOUT_ELEMENTS;
};

struct PointerSizeMember
{
	size_t pointerSize;
};

struct PointerSizeMemberExtended
{
	const void* byteCode = nullptr;

	size_t byteCodeSize = 0;
};


struct StructSizeRelation
{
	int cbSize;
	int field1;
	int field2;
	long long field3;
};

struct ReservedRelation
{
	int field1;
	int field2;
	int reserved;
};

struct StructPlatformDependency {
	int element;
#ifdef STRUCT_PLATFORM_32
	int _padding;
#endif
};

typedef struct _GUID {
	unsigned long  Data1;
	unsigned short Data2;
	unsigned short Data3;
	unsigned char  Data4[8];
} GUID;

typedef unsigned __int64 TOPOID;

typedef struct _MFTOPONODE_ATTRIBUTE_UPDATE
{
	TOPOID NodeId;
	GUID guidAttributeKey;
	MF_ATTRIBUTE_TYPE attrType;
	union
	{
		unsigned int u32;
		unsigned __int64 u64;
		double d;
	};
} MFTOPONODE_ATTRIBUTE_UPDATE;

namespace TestNamespace
{
	enum CrShutterSpeedSetX : unsigned int
	{
		CrShutterSpeedX_Bulb = 0x00000000,
		CrShutterSpeedX_Nothing = 0xFFFFFFFF,
	};

	struct StructEnumTypeDefaultValueX
	{
		CrShutterSpeedSetX element = CrShutterSpeedX_Nothing;
	};
}

static_assert(sizeof(wchar_t) == 2, "Wide character isn't wide.");

STRUCTLIB_FUNC(SimpleStruct) GetSimpleStruct();

STRUCTLIB_FUNC(StructWithArray) PassThroughArray(StructWithArray param);

STRUCTLIB_FUNC(TestUnion) PassThroughUnion(TestUnion param);

STRUCTLIB_FUNC(UnionWithArray) PassThroughUnion2(UnionWithArray param);

STRUCTLIB_FUNC(BitField) PassThroughBitfield(BitField param);

STRUCTLIB_FUNC(AsciiTest) PassThroughAscii(AsciiTest param);

STRUCTLIB_FUNC(Utf16Test) PassThroughUtf(Utf16Test param);

STRUCTLIB_FUNC(NestedTest) PassThroughNested(NestedTest param);

STRUCTLIB_FUNC(BoolToInt2) PassThroughBoolToInt(BoolToInt2 param);

STRUCTLIB_FUNC(BoolArray) PassThroughBoolArray(BoolArray param);

STRUCTLIB_FUNC(bool) VerifyReservedBits(BitField2 param);

STRUCTLIB_FUNC(void) CustomNativeNewTest(CustomNativeNew param);

STRUCTLIB_FUNC(StructWithInterface) GetStructWithInterface();

STRUCTLIB_FUNC(StructWithInterface) PassThroughStructWithInterface(StructWithInterface param); 

STRUCTLIB_FUNC(PointerSizeMember) PassThroughPointerSizeMember(PointerSizeMember param);

STRUCTLIB_FUNC(bool) VerifyFlags(D2D1_DEVICE_CONTEXT_OPTIONS param);