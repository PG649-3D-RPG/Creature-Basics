#if defined(_MSC_VER)
#ifdef NativePlugin_EXPORTS
/* We are building this library */
#   define NATIVEPLUGIN_API extern "C" __declspec(dllexport)
#else
/* We are using this library */
#   define NATIVEPLUGIN_API extern "C" __declspec(dllimport)
#endif
#else
    #define NATIVEPLUGIN_API extern "C"
#endif

#include<iostream>
#include<Dense>
 
using Eigen::MatrixXd;
using Eigen::VectorXd;

NATIVEPLUGIN_API int get_life();
