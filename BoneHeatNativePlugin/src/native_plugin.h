#if defined(_MSC_VER)
/* We are building this library */
#   define NATIVEPLUGIN_API extern "C" __declspec(dllexport)
#else
    #define NATIVEPLUGIN_API extern "C"
#endif

#include<SparseCore>
#include<SparseCholesky>


NATIVEPLUGIN_API int solveSPDMatrix(int rows, int cols, Eigen::Triplet<float>* triplets, int tripletsLength, float* rhs, int rhsLength, float* result);
