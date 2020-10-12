using UnityEngine;

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
}