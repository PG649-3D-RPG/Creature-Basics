#include "native_plugin.h"

pmp::Scalar angle(const pmp::Point& p1, const pmp::Point& p2)
{
    return std::acos(pmp::dot(p1, p2) / (pmp::norm(p1) * pmp::norm(p2)));
}


NATIVEPLUGIN_API void setMesh(float* vertexBuffer, int vertexCount, int* indexBuffer, int indexCount)
{
    _mesh.clear();

    std::vector<pmp::Vertex> vertices;
    vertices.resize(vertexCount);

    for (int i = 0; i < vertexCount; i++) {
        const auto v = _mesh.add_vertex(pmp::Point(vertexBuffer[i * 3], vertexBuffer[i * 3 + 1], vertexBuffer[i * 3 + 2]));

        vertices[i] = v;
    }

    for (int i = 0; i < indexCount; i += 3) {
        const auto v0 = vertices[indexBuffer[i]];
        const auto v1 = vertices[indexBuffer[i + 1]];
        const auto v2 = vertices[indexBuffer[i + 2]];

        _mesh.add_triangle(v0, v1, v2);
    }
}


NATIVEPLUGIN_API void processMesh()
{
    const pmp::Scalar edge_length(0.5);
    const int iterations = 10;
    pmp::Remeshing rm(_mesh);
    rm.uniform_remeshing(edge_length, iterations);
    
    pmp::Triangulation tri(_mesh);
    tri.triangulate(pmp::Triangulation::Objective::MAX_ANGLE);

    bool isDelaunay = false;
    int passes = 0;
    int flips = 0;
    for (int i = 0; !isDelaunay && i < 10000; i++) {
        isDelaunay = true;
        passes++;
        flips = 0;
        for (auto e : _mesh.edges()) {
            if (_mesh.is_boundary(e) || !_mesh.is_flip_ok(e))
                continue;

            pmp::Halfedge h10 = _mesh.halfedge(e, 0);
            pmp::Halfedge h01 = _mesh.halfedge(e, 1);
            pmp::Halfedge h10p = _mesh.next_halfedge(h10);
            pmp::Halfedge h01p = _mesh.next_halfedge(h01);

            pmp::Vertex v10 = _mesh.to_vertex(h10);
            pmp::Vertex v01 = _mesh.to_vertex(h01);
            pmp::Vertex v10p = _mesh.to_vertex(h10p);
            pmp::Vertex v01p = _mesh.to_vertex(h01p);

            const pmp::Point& v1 = _mesh.position(v10) - _mesh.position(v10p);
            const pmp::Point& v2 = _mesh.position(v01) - _mesh.position(v10p);
            const pmp::Point& v3 = _mesh.position(v10) - _mesh.position(v01p);
            const pmp::Point& v4 = _mesh.position(v01) - _mesh.position(v01p);

            pmp::Scalar alpha = angle(v1, v2);
            pmp::Scalar beta = angle(v3, v4);

            const pmp::Scalar pi(3.14159265358979);
            if (alpha + beta > pi) {
                isDelaunay = false;

                flips++;
                _mesh.flip(e);
            }
        }
    }

    _mesh.garbage_collection();
}

NATIVEPLUGIN_API int numVertices()
{
    return _mesh.n_vertices();
}

NATIVEPLUGIN_API int numIndices()
{
    return _mesh.n_faces() * 3;
}

NATIVEPLUGIN_API void writeMesh(float* vertexBuffer, int* indexBuffer)
{
    std::map<pmp::Vertex, int> vertexMap;
    int vertexIdx = 0;

    int i = 0;
    for (auto vertex : _mesh.vertices()) {
        if (_mesh.is_deleted(vertex))
            continue;

        vertexMap.insert(std::pair<pmp::Vertex, int>(vertex, vertexIdx++));

        const pmp::Point& point = _mesh.position(vertex);
        vertexBuffer[i++] = static_cast<float>(point[0]);
        vertexBuffer[i++] = static_cast<float>(point[1]);
        vertexBuffer[i++] = static_cast<float>(point[2]);
    }

    i = 0;
    for (auto face : _mesh.faces()) {
        if (_mesh.is_deleted(face))
            continue;

        for (auto vertex : _mesh.vertices(face)) {
            if (_mesh.is_deleted(vertex))
                continue;
            indexBuffer[i++] = vertexMap[vertex];
        }
    }
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

