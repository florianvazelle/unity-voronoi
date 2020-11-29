using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;
using static InterfaceUtils;

public class Interface2D : MonoBehaviour
{
    public GameObject pointPrefab;

    private Rect windowRect = new Rect(0, 0, 250, 250);
    private List<Triangle> tris;
    private List<Vector3> pointsCloud3D;
    private int verticesAmount = 10;

    void Start() {
        tris = new List<Triangle>();
        pointsCloud3D = new List<Vector3>();
    }

    private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 2D Points Cloud")) {
            ResetScene();
            pointsCloud3D = GenerateRandomVertices(verticesAmount);
            GeneratePoints(pointPrefab, pointsCloud3D);
        }

        if (ValidCloudPoint()) {
            GUILayout.Label("Triangulation");

            if (GUILayout.Button("Direct Delaunay")) {
                pointsCloud3D = UpdateVertices();
                ResetMesh();
                tris.Clear();

                // Convertion
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                Delaunay2D.Delaunay(pointsCloud2D, ref tris);

                GenerateMeshIndirect(tris);
            }

            if (GUILayout.Button("Direct Regular")) {
                pointsCloud3D = UpdateVertices();
                ResetMesh();
                tris.Clear();

                // Convertion
                List<Vector2> pointsCloud2D = ConvertListVector3ToVector2(pointsCloud3D);
                
                Delaunay2D.Regular(pointsCloud2D, ref tris);

                GenerateMeshIndirect(tris);
            }
        }
    }

    bool ValidCloudPoint() {
        return pointsCloud3D.Count > 0;
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
}