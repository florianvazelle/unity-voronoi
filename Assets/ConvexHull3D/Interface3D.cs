using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;
using static InterfaceUtils;

public class Interface3D : MonoBehaviour {

	public GameObject pointPrefab;

    private Rect windowRect = new Rect(0, 0, 250, 250);
    private List<int> indices;
    private List<Vector3> pointsCloud3D;
    private int verticesAmount = 10;

	void Start() {
		pointsCloud3D = new List<Vector3>();
        indices = new List<int>();

	}

	private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 3D Points Cloud")) {
            ResetScene();
            pointsCloud3D = GenerateRandom3DVertices(verticesAmount);
            GeneratePoints(pointPrefab, pointsCloud3D);
        }

        if (pointsCloud3D.Count > 0) {
            GUILayout.Label("Convex Hull 3D");
            List<Vector3> mesh3D = new List<Vector3>();

            if (GUILayout.Button("Incremental Convex Hull")) {
                pointsCloud3D = UpdateVertices();
                ResetMesh();
                indices.Clear();

                ConvexHull3D.IncrementalConvexHull(pointsCloud3D, ref mesh3D, ref indices);
                GenerateMesh(mesh3D, indices);
            }
        }
    }

    // void Update() {
    //     GameObject thisBuilding = GameObject.Find("Building");
    //     if (thisBuilding != null) {
    //         MeshFilter mf = thisBuilding.GetComponent<MeshFilter>();
    //         if (mf != null) {
    //             for (int i = 0; i < mf.mesh.vertices.Length; i++) {
    //                 Debug.DrawLine(mf.mesh.vertices[i], mf.mesh.vertices[i] + mf.mesh.normals[i], Color.red);
    //             }

    //             for (int i = 0; i < mf.mesh.triangles.Length; i += 3) {
    //                 Vector3 A = mf.mesh.vertices[mf.mesh.triangles[i]];
    //                 Vector3 B = mf.mesh.vertices[mf.mesh.triangles[i + 1]];
    //                 Vector3 C = mf.mesh.vertices[mf.mesh.triangles[i + 2]];

    //                 Vector3 center = findCenter(new List<Vector3>() { A, B, C });

    //                 var surfaceNormal = Vector3.Cross(B - A, C - A).normalized;
    //                 Debug.DrawLine(center, center + surfaceNormal, Color.blue);
    //             }
    //         } 
    //     }
    // }
}