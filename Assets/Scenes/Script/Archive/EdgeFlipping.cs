using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace archive
{
    public class EdgeFlipping : MonoBehaviour
    {
        public Material material;
        public List<Vector3> randomVertices;
        //public List<List<Vector3>> points;
        public List<Triangle> triangles;

        public const int verticesAmount = 10;

        private Camera _camera;
        private const int cameraZoffset = 10;
        private Event _event;

        void Start()
        {
            _camera = Camera.main;
            _camera.orthographic = true;
            _event = new Event();

            Triangle superTriangle = new Triangle(
                new Vector3(-40, -20, 0),
                new Vector3(0, 40, 0),
                new Vector3(40, -20, 0));

            superTriangle.createMesh("Super Triangle", material);

            generateRandomVertices();
            drawSomeTriangles();
        }

        // Pour ajout dynamique des points
        // event qui recupere la position de la souris dans le monde
        private void OnGUI()
        {
            _event = Event.current;

            if (_event.button == 0 && _event.isMouse)
            {
                Vector3 uperLeftCorner = new Vector3();
                uperLeftCorner = _camera.ScreenToWorldPoint(new Vector3(
                    _event.mousePosition.x,
                    _event.mousePosition.y,
                    cameraZoffset) // -10.0f if bugs
                );
                Debug.Log(uperLeftCorner);
            }

        }

        void generateRandomVertices()
        {
            Vector3 uperLeftCorner = _camera.ScreenToWorldPoint(new Vector3(
                0,
                0,
                cameraZoffset)); // -10.0f if bugs
            Vector3 lowerRightCorner = _camera.ScreenToWorldPoint(new Vector3(
                Screen.width,
                Screen.height,
                cameraZoffset)); // -10.0f if bugs
            Debug.Log(" uperLeftCorner : " + uperLeftCorner + " lowerRightCorner : " + lowerRightCorner);

            for (int i = 0; i < verticesAmount; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(
                    Random.Range(uperLeftCorner.x, lowerRightCorner.x),
                    Random.Range(uperLeftCorner.y, lowerRightCorner.y),
                    0);
            }
        }

        void drawSomeTriangles()
        {
            // List of List of points in clockwise required (TODO : not required)
            triangles = new List<Triangle>();

            Triangle tri1 = new Triangle(
                new Vector3(-4, 4, 0),
                new Vector3(-3, 6, 0),
                new Vector3(0, 0, 0));

            Triangle tri2 = new Triangle(
                new Vector3(0, 0, 0),
                new Vector3(-3, 6, 0),
                new Vector3(3, 6, 0));

            Triangle tri3 = new Triangle(
                new Vector3(0, 0, 0),
                new Vector3(3, 6, 0),
                new Vector3(4, 4, 0));

            triangles.Add(tri1);
            triangles.Add(tri2);
            triangles.Add(tri3);


            GenerateMesh(triangles);
        }
        // adapted function to generate one mesh per triangle without other vertices
        void GenerateMesh(List<Triangle> triangles)
        {
            Debug.Log(triangles.Count);

            for (int i = 0; i < triangles.Count; i++)
            {
                // Create a triangle game object
                GameObject thisTriangle = new GameObject("Triangle " + i);
                //float height = triangles[i].vertices[1].y;

                // Convert vertices to array for mesh
                var triangle = triangles[i];
                var normals = new List<Vector3>();
                var indices = new List<int>();

                for (int j = 0; j < triangle.vertices.Length; j++)
                {
                    normals.Add(Vector3.back);
                }

                for (int k = 0; k < triangle.vertices.Length; k++)
                {
                    indices.Add(k);
                }

                // Create and apply the mesh
                MeshFilter mf = thisTriangle.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                mf.mesh = mesh;

                mesh.SetVertices(triangle.vertices.ToList());
                mesh.SetNormals(normals);
                mesh.SetTriangles(indices, 0);

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                Renderer rend = thisTriangle.AddComponent<MeshRenderer>();
                rend.material = material;
            }
        }

        // https://forum.unity.com/threads/building-mesh-from-polygon.484305/
        void GenerateMesh(List<List<Vector3>> buildingVertices)
        {
            Debug.Log(buildingVertices.Count);

            for (int i = 0; i < buildingVertices.Count; i++)
            {

                // Create a building game object
                GameObject thisBuilding = new GameObject("Building " + i);

                float height = buildingVertices[i][1].y;

                // Compute the center point of the polygon both on the ground, and at height
                // Add center vertices to end of list
                Vector3 center = findCenter(buildingVertices[i]);
                buildingVertices[i].Add(center);

                Vector3 raisedCenter = center;
                raisedCenter.y += height;
                buildingVertices[i].Add(raisedCenter);


                // Convert vertices to array for mesh
                var vertices = buildingVertices[i];
                var normals = new List<Vector3>();
                var triangles = new List<int>();

                for (int j = 0; j < vertices.Count; j++)
                {
                    normals.Add(Vector3.up);
                }

                // Do the triangles for the roof and the floor of the building
                // Roof points are at odd indeces
                for (int j = vertices.Count - 3; j >= 0; j--)
                {
                    // Add the point
                    triangles.Add(j);
                    // Check for wrap around
                    if (j - 2 >= 0)
                    {
                        triangles.Add(j - 2);
                    }
                    else
                    {
                        // If wrap around, add the first vertex
                        int diff = j - 2;
                        triangles.Add(vertices.Count - 2 + diff);
                    }
                    // Check if its at ground or building height level, choose proper center point
                    if (j % 2 == 0)
                    {
                        triangles.Add(vertices.Count - 2);
                    }
                    else
                    {
                        triangles.Add(vertices.Count - 1);
                    }
                }

                // Do triangles which connect roof to ground
                for (int j = vertices.Count - 3; j >= 2; j--)
                {
                    if (j % 2 == 1)
                    {
                        triangles.Add(j);
                        triangles.Add(j - 1);
                        triangles.Add(j - 2);
                    }
                    else
                    {
                        triangles.Add(j);
                        triangles.Add(j - 2);
                        triangles.Add(j - 1);
                    }
                }

                // Create and apply the mesh
                MeshFilter mf = thisBuilding.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                mf.mesh = mesh;

                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(triangles, 0);

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                Renderer rend = thisBuilding.AddComponent<MeshRenderer>();
                rend.material = material;
            }
        }

        // TODO : replace by ConvexHull.getBarycenter
        // Find the center X-Z position of the polygon.
        Vector3 findCenter(List<Vector3> verts)
        {
            Vector3 center = Vector3.zero;
            // Only need to check every other spot since the odd indexed vertices are in the air, but have same XZ as previous
            for (int i = 0; i < verts.Count; i += 2)
            {
                center += verts[i];
            }
            return center / (verts.Count / 2);
        }
    }
}

