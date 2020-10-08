using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class EdgeFlipping : MonoBehaviour
{
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] vertices = new Vector3[3];
        Vector2[] uv = new Vector2[3];
        int[] indices = new int[3];

        vertices[0] = new Vector3(0, 1);
        vertices[1] = new Vector3(1, 1);
        vertices[2] = new Vector3(0, 0);
        //vertices[3] = new Vector3(1, 0);

        uv[0] = new Vector2(0, 1);
        uv[1] = new Vector2(1, 1);
        uv[2] = new Vector2(0, 0);
        //uv[3] = new Vector2(1, 0);

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        //indices[3] = 2;
        //indices[4] = 1;
        //indices[5] = 3;

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = indices;

        GameObject gameobject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        gameobject.transform.localScale = new Vector3(1, 1, 1);

        gameobject.GetComponent<MeshFilter>().mesh = mesh;
        gameobject.GetComponent<MeshRenderer>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
