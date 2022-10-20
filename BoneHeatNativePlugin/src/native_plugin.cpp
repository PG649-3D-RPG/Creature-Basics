#include "native_plugin.h"


NATIVEPLUGIN_API int calcBoneWeights(float* vertexBuffer, int vertexCount, int* indexBuffer, int indexCount)
{
    pmp::SurfaceMesh mesh;

    std::vector<pmp::Vertex> vertices;
    vertices.resize(vertexCount);

    for (int i = 0; i < vertexCount; i++) {
        const auto v = mesh.add_vertex(pmp::Point(vertexBuffer[i * 3], vertexBuffer[i * 3 + 1], vertexBuffer[i * 3 + 2]));

        vertices.push_back(v);
    }

    for (int i = 0; i < indexCount; i += 3) {
        const auto v0 = vertices[indexBuffer[i]];
        const auto v1 = vertices[indexBuffer[i + 1]];
        const auto v2 = vertices[indexBuffer[i + 2]];

        //mesh.add_triangle(v0, v1, v2);
    }

    return mesh.n_vertices();
}


NATIVEPLUGIN_API int solveSPDMatrix(int rows, int cols, Eigen::Triplet<float>* triplets, int tripletsLength, float* rhs, int rhsLength, float* result)
{
    Eigen::SparseMatrix<float> matrix(rows, cols);

    for (int i = 0; i < tripletsLength; i++) {
        Eigen::Triplet<float> t = triplets[i];
        matrix.insert(t.row(), t.col()) = t.value();
    }

    Eigen::VectorXf b(rhsLength);
    for (int i = 0; i < rhsLength; i++) {
        b[i] = rhs[i];
    }

    Eigen::VectorXf x;
    Eigen::SimplicialLLT<Eigen::SparseMatrix<float>, Eigen::Lower> solver;
    solver.compute(matrix);
    if(solver.info() != Eigen::Success) {
        // decomposition failed
        return 1;
    }
    x = solver.solve(b);
    if(solver.info() != Eigen::Success) {
        // solving failed
        return 2;
    }

    for (int i = 0; i < rhsLength; i++) {
        result[i] = x[i];
    }

    return 0;
}

