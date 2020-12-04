using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;
using static InterfaceUtils;

public class Interface2D : MonoBehaviour
{
    [Flags]
    private enum FLAGS { NOOP = 1, JARVIS = 2, GRAHAM = 4, DELAUNAY = 8, REGULAR = 16, FLIP = 32, VORONOI = 64 };

    public GameObject pointPrefab;
    public GameObject centerPrefab;
    public Color lineColor;

    private Rect windowRect = new Rect(0, 0, 250, 500);     // Rect window for ImGUI
    private int verticesAmount = 10;                        // Parameter of ImGUI to select the vertex amount
    private List<Vector3> pointsCloud3D;                    // List of points in the scene
    private List<Triangle> tris;                            // Temporaly list with result of triangulation
    private Material lineMat;                               // Voronoi Line material
    private List<Edge> edges;                               // Temporaly list with result of Voronoi
    private FLAGS currentState, oldState;                   // Current state, on which button you click to execute method in update
    private long elapsedMs = -1;                            // To calculate how many time the method execute 

    void Start() {
        tris = new List<Triangle>();
        pointsCloud3D = new List<Vector3>();
        lineMat = new Material(Shader.Find("Unlit/Color"));
        lineMat.color = lineColor;
        edges = new List<Edge>();
        currentState = oldState = FLAGS.NOOP;
    }

    void Update() {

        List<Vector3> newPointsCloud3D = UpdateVertices();
        
        // Sort array to compare them
        newPointsCloud3D.Sort((x, y) => {
            return (x.x == y.x) ? (x.y == y.y) ? (x.z == y.z) ? 0 : x.z.CompareTo(y.z) : x.y.CompareTo(y.y) : x.x.CompareTo(y.x);
        });
        pointsCloud3D.Sort((x, y) => {
            return (x.x == y.x) ? (x.y == y.y) ? (x.z == y.z) ? 0 : x.z.CompareTo(y.z) : x.y.CompareTo(y.y) : x.x.CompareTo(y.x);
        });
        bool equals = Enumerable.SequenceEqual(newPointsCloud3D, pointsCloud3D);

        // Update only if state are change and if a point is drag
        if (!equals || oldState != currentState) {
            oldState = currentState;
            pointsCloud3D = newPointsCloud3D;
            
            // if state is no operation, we skip
            if (currentState == FLAGS.NOOP) return;

            // ***** Convex Hull *****

            if (currentState == FLAGS.JARVIS) {
                ResetMesh();

                List<Vector2> mesh2D = new List<Vector2>();
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ConvexHull.JarvisMarch(pointsCloud2D, ref mesh2D);
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;

                GenerateConvexHullMeshIndirect(mesh2D);
            } else if (currentState == FLAGS.GRAHAM) {
                ResetMesh();

                List<Vector2> mesh2D = new List<Vector2>();
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);

                var watch = System.Diagnostics.Stopwatch.StartNew();
                ConvexHull.GrahamScan(pointsCloud2D, ref mesh2D);
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;

                GenerateConvexHullMeshIndirect(mesh2D);
            }

            // ***** Triangulation & Voronoi *****
            
            // Execute when you click on the delaunay button or you want make voronoi with delaunay
            if (currentState == FLAGS.DELAUNAY || currentState == (FLAGS.VORONOI | FLAGS.DELAUNAY)) {
                // Clear
                ResetMesh();
                ResetData();
                ResetCenter();

                // Convertion
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                // Delaunay
                Delaunay2D.Delaunay(pointsCloud2D, ref tris);

                // Draw
                GenerateMeshIndirect(tris);
            }

            // Execute when you click on the regular button or you want make voronoi with regular
            if (currentState == FLAGS.REGULAR || currentState == FLAGS.FLIP || currentState == (FLAGS.VORONOI | FLAGS.FLIP)) {
                // Clear
                ResetMesh();
                ResetData();
                ResetCenter();

                // Convertion
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                // Regular
                Delaunay2D.Regular(pointsCloud2D, ref tris);
                
                // Draw
                GenerateMeshIndirect(tris);
            }

            // Execute when you click on the flip button or you want make voronoi with flip after regular
            if (currentState == FLAGS.FLIP || currentState == (FLAGS.VORONOI | FLAGS.FLIP)) {
                // Flip
                Delaunay2D.FlipToDelaunay(ref tris);

                // Draw
                GenerateMeshIndirect(tris);
            }
            
            // Execute when you click on the voronoi button 
            if (currentState >= FLAGS.VORONOI) {
                // Get all circumscribed circle center
                List<Vector2> AllCenterPoints = Delaunay2D.AllCenterPoint(tris);
                // And draw it
                GenerateCenter(centerPrefab, AllCenterPoints);

                // Voronoi
                edges = Delaunay2D.Voronoi2D(tris);
                // Draw new edges in the PostRender method
            }
        }
    }

    private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 2D Points Cloud")) {
            currentState = FLAGS.NOOP;
            elapsedMs = -1;
            ResetScene();
            ResetData();
            pointsCloud3D = GenerateRandomVertices(verticesAmount);
            GeneratePoints(pointPrefab, pointsCloud3D);
        }

        if (ValidCloudPoint()) {
            GUILayout.Label("Convex Hull");

            if (GUILayout.Button("Jarvis March")) currentState = FLAGS.JARVIS;
            if (GUILayout.Button("GrahamScan")) currentState = FLAGS.GRAHAM;

            if (elapsedMs != -1) {
                GUILayout.Label("In " + elapsedMs + " milliseconds");
            }
            
            GUILayout.Label("Triangulation");

            if (GUILayout.Button("Direct Delaunay")) currentState = FLAGS.DELAUNAY;
            if (GUILayout.Button("Direct Regular")) currentState = FLAGS.REGULAR;

            if (tris.Count > 0) {
                if (currentState == FLAGS.REGULAR) {
                    if (GUILayout.Button("Flip")) currentState = FLAGS.FLIP;
                }

                // Add current flag with voronoi flag because voronoi, need to make with a triangulation method
                if (GUILayout.Button("Direct Voronoi2D")) currentState |= FLAGS.VORONOI;
            }
        }
    }

    void OnPostRender() {
        foreach (var edge in edges) {
            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a));
            GL.Vertex3(edge.start.x, edge.start.y, 0f);
            GL.Vertex3(edge.end.x, edge.end.y, 0f);
            GL.End();
        }
    }

    bool ValidCloudPoint() {
        return pointsCloud3D.Count > 0;
    }

    public void ResetData() {
        tris.Clear();
        edges.Clear();
    }

    static public void GenerateMeshIndirect(List<Triangle> triangles) {
        triangles.ForEach(tri => SortInClockWise(ref tri.vertices));

        List<Vector2> points2D = new List<Vector2>();
        List<int> indices = new List<int>();
        
        for(int i = 0; i < triangles.Count; i++) {
            for(var j = 0; j < 3; j++) {
                points2D.Add(triangles[i].vertices[j]);
                indices.Add(i * 3 + j);
            }
        }

        GenerateMesh(ConvertListVector2ToVector3(points2D), indices);
    }

    static public void GenerateConvexHullMeshIndirect(List<Vector2> points2D) {
        // Sort in clockwise
        SortInClockWise(ref points2D);

        List<int> indices = new List<int>();
        Vector2 center = ConvexHull.GetBarycenter(points2D);
        points2D.Add(points2D[0]); // to close the mesh
        points2D.Add(center);
        int centerIdx = points2D.Count - 1;
        
        for(int i = 0; i < centerIdx; i++) {
            if (i >= 2) {
                indices.Add(centerIdx);
                indices.Add(i - 1);
            }
            indices.Add(i);
            if (i == 1) {
                indices.Add(centerIdx);
            } 
        }

        GenerateMesh(ConvertListVector2ToVector3(points2D), indices);
    }

    // https://forum.unity.com/threads/building-mesh-from-polygon.484305/
    static public void GenerateMesh(List<Vector3> vertices, List<int> indices) {
        GameObject thisBuilding = GameObject.Find("Building");
        if (thisBuilding == null) {
            // Create a building game object
            thisBuilding = new GameObject ("Building");
        }

        var normals = new List<Vector3>();
        
        for (int i = 0; i < vertices.Count; i++) {
            normals.Add(Vector3.back);
        }

        // Create and apply the mesh
        MeshFilter mf = thisBuilding.GetComponent<MeshFilter>();
        if (mf == null) {
            // Create and apply the mesh
            mf = thisBuilding.AddComponent<MeshFilter>();
        }

        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(indices, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        MeshRenderer rend = thisBuilding.GetComponent<MeshRenderer>();
        if (rend == null) {
            rend = thisBuilding.AddComponent<MeshRenderer>();
        }
        rend.material = new Material(Shader.Find("SuperSystems/Wireframe-Shaded-Unlit"));
    }
}