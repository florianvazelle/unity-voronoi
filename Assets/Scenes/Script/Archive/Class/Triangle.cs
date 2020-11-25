using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace archive
{
    public class Triangle
    {
        public Vector3[] vertices; // fixed size of 3

        public Triangle() { vertices = new Vector3[3]; }

        public Triangle(Vector3[] v) { vertices = v; }
        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            vertices = new Vector3[3];
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
        }

        public void createMesh(string name, Material mat)
        {
            // Create a triangle game object
            GameObject thisTriangle = new GameObject(name);
            //float height = triangles[i].vertices[1].y;

            // Convert vertices to array for mesh
            var normals = new List<Vector3>();
            var indices = new List<int>();

            for (int i = 0; i < vertices.Length; i++)
            {
                normals.Add(Vector3.back);
            }

            for (int k = 0; k < vertices.Length; k++)
            {
                indices.Add(k);
            }

            // Create and apply the mesh
            MeshFilter mf = thisTriangle.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            mf.mesh = mesh;

            mesh.SetVertices(vertices.ToList());
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            Renderer rend = thisTriangle.AddComponent<MeshRenderer>();
            rend.material = mat;
        }
    }
}
