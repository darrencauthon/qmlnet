#ifndef QTNETCOREQML_GLOBAL_H
#define QTNETCOREQML_GLOBAL_H

#include <QtCore/qglobal.h>

enum NetVariantTypeEnum {
    NetVariantTypeEnum_Invalid,
    NetVariantTypeEnum_Bool,
    NetVariantTypeEnum_Char,
    NetVariantTypeEnum_Int,
    NetVariantTypeEnum_UInt,
    NetVariantTypeEnum_Double,
    NetVariantTypeEnum_String,
    NetVariantTypeEnum_DateTime,
    NetVariantTypeEnum_Object
};

#define NetGCHandle void

#if _MSC_VER
    #include <Windows.h>
    #include <stdint.h>
    #include <Tchar.h>

    typedef const wchar_t* BCSTR;
    typedef const char* LPCSTR;
    typedef const wchar_t* LPWCSTR;

    #if UNICODE
        #define LPTSTR(value) L ##value;
        typedef LPWCSTR LPTCSTR;
    #else
        #define LPTSTR(value) value;
        typedef LPCSTR LPTCSTR;
    #endif
#endif

#if !_MSC_VER
    #define __declspec(dllexport)
    #define __stdcall

    typedef char16_t* BSTR;
    typedef const char16_t* BCSTR;

    typedef char* LPSTR;
    typedef const char* LPCSTR;

    typedef char16_t* LPWSTR;
    typedef const char16_t* LPWCSTR;

    #if UNICODE
        #define LPTSTR(value) (LPTSTR)u ##value;
        typedef LPWSTR LPTSTR;
        typedef LPWCSTR LPTCSTR;
    #else
        #define LPTSTR(value) value;
        typedef LPSTR LPTSTR;
        typedef LPCSTR LPTCSTR;
    #endif
#endif

#endif // QTNETCOREQML_GLOBAL_H