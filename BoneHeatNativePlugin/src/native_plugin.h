#if defined(_MSC_VER)
/* We are building this library */
#   define NATIVEPLUGIN_API extern "C" __declspec(dllexport)
#else
    #define NATIVEPLUGIN_API extern "C"
#endif

#include<Eigen/SparseCore>
#include<Eigen/SparseCholesky>

#include <pmp/SurfaceMesh.h>
#include <pmp/algorithms/Triangulation.h>

pmp::Scalar angle(const pmp::Point& p1, const pmp::Point& p2);

NATIVEPLUGIN_API int triangulateMesh(float* vertexBuffer, int vertexCount, int* indexBuffer, int indexCount, int* resultIndexBuffer);

NATIVEPLUGIN_API int solveSPDMatrix(int rows, int cols, Eigen::Triplet<float>* triplets, int tripletsLength, float* rhs, int rhsLength, float* result);
