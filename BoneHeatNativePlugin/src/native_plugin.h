#if defined(_MSC_VER)
/* We are building this library */
#   define NATIVEPLUGIN_API extern "C" __declspec(dllexport)
#else
    #define NATIVEPLUGIN_API extern "C"
#endif

#include<map>

#include<Eigen/SparseCore>
#include<Eigen/SparseCholesky>
#include<Eigen/SparseLU>

#include <pmp/SurfaceMesh.h>
#include <pmp/algorithms/Remeshing.h>
#include <pmp/algorithms/Triangulation.h>

pmp::SurfaceMesh _mesh;

pmp::Scalar angle(const pmp::Point& p1, const pmp::Point& p2);

NATIVEPLUGIN_API void preprocessMesh(float min_e, float max_e, float approx_error);
NATIVEPLUGIN_API void setMesh(float* vertexBuffer, int vertexCount, int* indexBuffer, int indexCount);

NATIVEPLUGIN_API int numVertices();
NATIVEPLUGIN_API int numIndices();

NATIVEPLUGIN_API void writeMesh(float* vertexBuffer, int* indexBuffer);

NATIVEPLUGIN_API int solveSPDMatrix(int rows, int cols, Eigen::Triplet<float>* triplets, int tripletsLength, float* rhs, int rhsLength, float* result);
