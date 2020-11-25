using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay2D : MonoBehaviour
{
    public int numberOfPoints;
    public Material triangleMaterial;

    private Camera _camera;
    private const int cameraZoffset = 10;

    private List<GameObject> vertices;
    private Triangle superTriangle;
    private List<Triangle> triangles; // err when private

    // Start is called before the first frame update
    void Start()
    {
        InitCamera();
        GenerateRandomPoints();
        GenerateSuperTriangle();
        SplitSuperTriangle();
        MainLoop();
    }

    private void InitCamera()
    {
        _camera = Camera.main;
        _camera.orthographic = true;
    }

    private void GenerateRandomPoints()
    {
        Debug.Log("Generation de " + numberOfPoints + " points");
        Vector3 uperLeftCorner = _camera.ScreenToWorldPoint(new Vector3(
            0,
            0,
            cameraZoffset)); // -10.0f if bugs
        Vector3 lowerRightCorner = _camera.ScreenToWorldPoint(new Vector3(
            Screen.width,
            Screen.height,
            cameraZoffset)); // -10.0f if bugs
        //Debug.Log(" uperLeftCorner : " + uperLeftCorner + " lowerRightCorner : " + lowerRightCorner);

        vertices = new List<GameObject>(numberOfPoints);
        for (int i = 0; i < numberOfPoints; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3(
                Random.Range(uperLeftCorner.x, lowerRightCorner.x),
                Random.Range(uperLeftCorner.y, lowerRightCorner.y),
                0);
            vertices.Add(sphere);
        }
    }
    
    private void GenerateSuperTriangle()
    {
        superTriangle = new Triangle(
            new Vector3(-40, -20, 0),
            new Vector3(0, 40, 0),
            new Vector3(40, -20, 0)
        );
        superTriangle.CreateMesh(triangleMaterial, "superTriangle");
    }

    private void SplitSuperTriangle()
    {
        triangles = new List<Triangle>();

        // select a random point (here the first one)
        Triangle triangle1 = new Triangle(
            superTriangle.vertices[0].transform.position,
            superTriangle.vertices[1].transform.position,
            vertices[0].transform.position
        );
        triangle1.CreateMesh(triangleMaterial, "triangle1");
        triangles.Add(triangle1);

        Triangle triangle2 = new Triangle(
            superTriangle.vertices[1].transform.position,
            superTriangle.vertices[2].transform.position,
            vertices[0].transform.position
        );
        triangle2.CreateMesh(triangleMaterial, "triangle2");
        triangles.Add(triangle2);

        Triangle triangle3 = new Triangle(
            superTriangle.vertices[2].transform.position,
            superTriangle.vertices[0].transform.position,
            vertices[0].transform.position
        );
        triangle3.CreateMesh(triangleMaterial, "triangle3");
        triangles.Add(triangle3);

        // remove used vertice
        vertices.RemoveAt(0);
    }

    private void MainLoop()
    {
        // Pour chaque point non utilisé dans un triangle
        // make list of unused vertices

        Debug.Log(vertices.Count);
        while (vertices.Count > 0)
        {
            // create an array for each triangle the point is in (circumscribedCircle area)
            List<Triangle> circumscribedCircleTriangle = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                // est ce que vertices 0 est dans le cercle circonscrit du triangle
                
                Vector3 p1 = triangle.vertices[0].transform.position;
                Vector3 p2 = triangle.vertices[1].transform.position;
                Vector3 p3 = triangle.vertices[2].transform.position;
                Vector3 v1 = p2 - p1;
                Vector3 v2 = p3 - p1;
            }

            vertices.RemoveAt(0);
            Debug.Log(vertices.Count);
        }


    }
}
