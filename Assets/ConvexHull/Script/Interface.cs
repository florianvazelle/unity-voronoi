using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;

public class Interface: MonoBehaviour
{
    public GameObject pointPrefab;

    private Rect windowRect = new Rect(0, 0, 250, 250);
    private List<Vector3> pointsCloud3D;
    private int verticesAmount = 10;
    private long elapsedMs = -1;

    void Start() {
        pointsCloud3D = new List<Vector3>();
    }

    private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 2D Points Cloud")) {
            elapsedMs = -1;
            ResetScene();
            pointsCloud3D = GenerateRandomVertices(verticesAmount);
            GeneratePoints(pointPrefab, new List<List<Vector3>> () { pointsCloud3D });
        }

        if (ValidCloudPoint()) {
            GUILayout.Label("Convex Hull");
            List<Vector2> mesh2D = new List<Vector2>();

            if (GUILayout.Button("Jarvis March")) {
                pointsCloud3D = UpdateVertices();
                ResetMesh();
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ConvexHull.JarvisMarch(pointsCloud2D, ref mesh2D);
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;

                GenerateMeshIndirect(mesh2D);
            }
            else if (GUILayout.Button("GrahamScan")) {
                pointsCloud3D = UpdateVertices();
                ResetMesh();
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);

                var watch = System.Diagnostics.Stopwatch.StartNew();
                ConvexHull.GrahamScan(pointsCloud2D, ref mesh2D);
                elapsedMs = watch.ElapsedMilliseconds;

                GenerateMeshIndirect(mesh2D);
            }

            if (elapsedMs != -1) {
                GUILayout.Label("In " + elapsedMs + " milliseconds");
            }
        }
    }

    bool ValidCloudPoint() {
        return pointsCloud3D.Count > 0;
    }

    static public void ResetScene() {
        GameObject tmp = GameObject.Find("Point(Clone)");
        while(tmp != null) {
            DestroyImmediate(tmp);
            tmp = GameObject.Find("Point(Clone)");
        }

        ResetMesh();
    }

    static public void ResetMesh() {
        GameObject thisBuilding = GameObject.Find("Building");
        if (thisBuilding != null) {
            DestroyImmediate(thisBuilding);
        }
    }

    static void GeneratePoints(GameObject prefab, List<List<Vector3>> buildingVertices) {
        for (int i = 0; i < buildingVertices.Count; i++) {
            for (int j = 0; j < buildingVertices[i].Count; j++) {
                Instantiate(prefab, buildingVertices[i][j], Quaternion.identity);
            }
        }
    }

    static List<Vector3> UpdateVertices() {
        List<Vector3> newPoints3D = new List<Vector3>();
        GameObject[] allGOs = FindObjectsOfType<GameObject>();
        foreach(var go in allGOs) {
            if (go.name == "Point(Clone)") {
                newPoints3D.Add(go.transform.position);
            }
        }
        return newPoints3D;
    }

    static List<Vector3> GenerateRandomVertices(int verticesAmount) {
        Vector3 uperLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)); // -10.0f if bugs
        Vector3 lowerRightCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10)); // -10.0f if bugs
        
        List<Vector3> points3D = new List<Vector3>();
        for (int i = 0; i < verticesAmount; i++) {
            points3D.Add(new Vector3(
                UnityEngine.Random.Range(uperLeftCorner.x, lowerRightCorner.x),
                UnityEngine.Random.Range(uperLeftCorner.y, lowerRightCorner.y),
                0
            ));
        }

        return points3D;
    }

    static public void SortInClockWise(ref List<Vector2> points2D) {
        ConvexHull.SortByAngle(ref points2D, ConvexHull.GetBarycenter(points2D));
        points2D.Reverse();
    }
        

    static public void SortInClockWise(ref List<Vector3> points3D) {
        List<Vector2> points2D = ConvertListVector3ToVector2(points3D);
        SortInClockWise(ref points2D);
        points3D = ConvertListVector2ToVector3(points2D); 
    }

    static List<Vector3> ConvertListVector2ToVector3(List<Vector2> points2D) {
        List<Vector3> points3D = new List<Vector3>();
        for (int i = 0; i < points2D.Count; i++) {
            points3D.Add(points2D[i]);
        }
        return points3D;
    }

    static List<Vector2> ConvertListVector3ToVector2(List<Vector3> points3D) {
        List<Vector2> points2D = new List<Vector2>();
        for (int i = 0; i < points3D.Count; i++) {
            points2D.Add(points3D[i]);
        }
        return points2D;
    }

    static public void GenerateMeshIndirect(List<Vector2> points2D) {
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
        rend.material = new Material(Shader.Find("Standard"));;
    }
}