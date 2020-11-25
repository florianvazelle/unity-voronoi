using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Triangle
{
    public List<GameObject> vertices;
    public GameObject surface;
    public Material material;

    public Triangle( List<GameObject> array ) { 
        vertices = array;
        surface = null;
        material = null;
    }

    public Triangle( List<Vector3> array ) {
        vertices = new List<GameObject>(3);
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].transform.position = array[i]; 
        }
        surface = null;
        material = null;
    }

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        vertices = new List<GameObject>();

        List<Vector3> positions = new List<Vector3>();
        positions.Add(p1);
        positions.Add(p2);
        positions.Add(p3);

        foreach (var p in positions) {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.transform.position = p;
            vertices.Add(gameObject);
        }
        surface = null;
        material = null;
    }

    public void CreateMesh(Material mat, string name = "") {
        // Create a triangle game object
        surface = new GameObject(name);
        //float height = triangles[i].vertices[1].y;

        // Convert vertices to array for mesh

        var vertex = new  List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<int>();
        
        for (int i = 0; i < vertices.Count; i++) {
            vertex.Add(vertices[i].transform.position);
        }

        for (int i = 0; i < vertices.Count; i++) {
            normals.Add(Vector3.back);
        }

        for (int k = 0; k < vertices.Count; k++) {
            indices.Add(k);
        }

        // Create and apply the mesh
        MeshFilter mf = surface.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        mesh.SetVertices(vertex);
        mesh.SetNormals(normals);
        mesh.SetTriangles(indices, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Renderer rend = surface.AddComponent<MeshRenderer>();
        rend.material = mat;
        material = mat;
    }
}