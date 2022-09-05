#include "native_plugin.h"

NATIVEPLUGIN_API int get_life()
{
    Eigen::SparseMatrix<float> inputMatrix(100, 100);
    inputMatrix.insert(4, 5) = 42;

    Eigen::VectorXf b, x;

    Eigen::SimplicialLLT<Eigen::SparseMatrix<float>, Eigen::Lower> solver;
    solver.compute(inputMatrix);
    if(solver.info() != Eigen::Success) {
        // decomposition failed
    }
    //x = solver.solve(b);
    if(solver.info() != Eigen::Success) {
        // solving failed
    }

    return 42; //le magique
}

