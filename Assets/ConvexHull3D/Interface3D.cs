using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RapidGUI;

public class Interface3D : MonoBehaviour {

	public GameObject pointPrefab;

    private Rect windowRect = new Rect(0, 0, 250, 250);
    private List<Vector3> pointsCloud3D;
    private int verticesAmount = 10;

	void Start() {
		pointsCloud3D = new List<Vector3>();
	}

	private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 3D Points Cloud")) {
            Interface.ResetScene();
            pointsCloud3D = GenerateRandomVertices(verticesAmount);
            Interface.GeneratePoints(pointPrefab, pointsCloud3D);
        }

        if (pointsCloud3D.Count > 0) {
            GUILayout.Label("Convex Hull 3D");
            List<Vector3> mesh3D = new List<Vector3>();

            if (GUILayout.Button("Incremental Convex Hull")) {
                pointsCloud3D = Interface.UpdateVertices();
                Interface.ResetMesh();
                ConvexHull3D.IncrementalConvexHull(pointsCloud3D, ref mesh3D);
                GenerateMesh(mesh3D);
            }
        }
    }

	static List<Vector3> GenerateRandomVertices(int verticesAmount) {
		Vector3 uperLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(-100, -100, 10));
		Vector3 lowerRightCorner = Camera.main.ScreenToWorldPoint(new Vector3(100, 100, 100));
		List <Vector3> points3D = new List < Vector3 > ();
		for (int i = 0; i < verticesAmount; i++) {
			points3D.Add(new Vector3 {
				x = UnityEngine.Random.Range(uperLeftCorner.x, lowerRightCorner.x),
				y = UnityEngine.Random.Range(uperLeftCorner.y, lowerRightCorner.y),
				z = UnityEngine.Random.Range(uperLeftCorner.z, lowerRightCorner.z)
			});
		}

		return points3D;
	}

	static public void GenerateMesh(List<Vector3> vertices) {

        GameObject thisBuilding = GameObject.Find("Building");
        if (thisBuilding == null) {
            // Create a building game object
            thisBuilding = new GameObject ("Building");
        }

        var normals = new List<Vector3>();
        var indices = new List<int>();

        for (int i = 0; i < vertices.Count; i++) {
            normals.Add(Vector3.back);
        }

        // TODO : Remove this brute force mesh creation
		for (int i = 0; i < vertices.Count; i++) {
			for (int j = 0; j < vertices.Count; j++) {
				if (i == j) continue;
				for (int k = 0; k < vertices.Count; k++) {
					if (i == k || j == k) continue;
					indices.Add(i);
					indices.Add(j);
					indices.Add(k);
				}
			}
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
        rend.material = new Material(Shader.Find("Standard"));
    }

}