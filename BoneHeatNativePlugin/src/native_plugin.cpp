#include "native_plugin.h"

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

