using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;
using static InterfaceUtils;

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
            GeneratePoints(pointPrefab, pointsCloud3D);
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
}