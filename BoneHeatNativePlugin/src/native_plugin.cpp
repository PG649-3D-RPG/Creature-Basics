#include "native_plugin.h"


NATIVEPLUGIN_API int triangulateMesh(float* vertexBuffer, int vertexCount, int* indexBuffer, int indexCount, int* resultIndexBuffer)
{
    pmp::SurfaceMesh mesh;

    std::vector<pmp::Vertex> vertices;
    vertices.resize(vertexCount);

    for (int i = 0; i < vertexCount; i++) {
        const auto v = mesh.add_vertex(pmp::Point(vertexBuffer[i * 3], vertexBuffer[i * 3 + 1], vertexBuffer[i * 3 + 2]));

        vertices[i] = v;
    }

    for (int i = 0; i < indexCount; i += 3) {
        const auto v0 = vertices[indexBuffer[i]];
        const auto v1 = vertices[indexBuffer[i + 1]];
        const auto v2 = vertices[indexBuffer[i + 2]];

        mesh.add_triangle(v0, v1, v2);
    }

    //pmp::Triangulation triangulation(mesh);
    //triangulation.triangulate(pmp::Triangulation::Objective::MAX_ANGLE);
    
    bool isDelaunay = false;
    int passes = 0;
    int flips = 0;
    for (int i = 0; !isDelaunay && i < 10000; i++) {
        isDelaunay = true;
        passes++;
        flips = 0;
        for (auto e : mesh.edges()) {
            if (mesh.is_boundary(e) || !mesh.is_flip_ok(e))
                continue;

            pmp::Halfedge h10 = mesh.halfedge(e, 0);
            pmp::Halfedge h01 = mesh.halfedge(e, 1);
            pmp::Halfedge h10p = mesh.next_halfedge(h10);
            pmp::Halfedge h01p = mesh.next_halfedge(h01);

            pmp::Vertex v10 = mesh.to_vertex(h10);
            pmp::Vertex v01 = mesh.to_vertex(h01);
            pmp::Vertex v10p = mesh.to_vertex(h10p);
            pmp::Vertex v01p = mesh.to_vertex(h01p);

            const pmp::Point& v1 = mesh.position(v10) - mesh.position(v10p);
            const pmp::Point& v2 = mesh.position(v01) - mesh.position(v10p);
            const pmp::Point& v3 = mesh.position(v10) - mesh.position(v01p);
            const pmp::Point& v4 = mesh.position(v01) - mesh.position(v01p);

            pmp::Scalar alpha = angle(v1, v2);
            pmp::Scalar beta = angle(v3, v4);

            if (alpha + beta > pmp::Scalar(3.141)) {
                isDelaunay = false;

                flips++;
                mesh.flip(e);
            }
        }
    }

    int i = 0;
    for (auto face : mesh.faces()) {
        for (auto vertex : mesh.vertices(face)) {
            resultIndexBuffer[i++] = static_cast<int>(vertex.idx());
        }
    }

    return flips;
}

pmp::Scalar angle(const pmp::Point& p1, const pmp::Point& p2)
{
    return std::acos(pmp::dot(p1, p2) / (pmp::norm(p1) * pmp::norm(p2)));
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

