using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RapidGUI;

public class Interface3D : MonoBehaviour {

	public GameObject pointPrefab;

    private Rect windowRect = new Rect(0, 0, 250, 250);
    private List<Vector3> pointsCloud3D;
    private int verticesAmount = 10;

    private int currentState = 0, oldCurrentState = 0; 

	void Start() {
		pointsCloud3D = new List<Vector3>();
	}

    void Update() {
        List<Vector3> newPointsCloud3D = InterfaceUtils.UpdateVertices();
        
        // Sort array to compare them
        newPointsCloud3D.Sort((x, y) => {
            return (x.x == y.x) ? (x.y == y.y) ? (x.z == y.z) ? 0 : x.z.CompareTo(y.z) : x.y.CompareTo(y.y) : x.x.CompareTo(y.x);
        });
        pointsCloud3D.Sort((x, y) => {
            return (x.x == y.x) ? (x.y == y.y) ? (x.z == y.z) ? 0 : x.z.CompareTo(y.z) : x.y.CompareTo(y.y) : x.x.CompareTo(y.x);
        });
        bool equals = Enumerable.SequenceEqual(newPointsCloud3D, pointsCloud3D);


        if (!equals || currentState != oldCurrentState) {
            oldCurrentState = currentState;
            pointsCloud3D = newPointsCloud3D;

            if (currentState == 1) {
                InterfaceUtils.ResetMesh();

                List<Vector3> mesh3D = new List<Vector3>();
                List<int> tris = new List<int>();

                ConvexHull3D.IncrementalConvexHull(pointsCloud3D, ref mesh3D, ref tris);
                GenerateMesh(mesh3D, tris);
            }
        }
    }

	private void OnGUI() {
        windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DoGUI, "Actions", RGUIStyle.darkWindow);
    }

    public void DoGUI(int windowID) {
        verticesAmount = RGUI.Field(verticesAmount, "Number of Points");

        if (GUILayout.Button("Generate 3D Points Cloud")) {
            currentState = 0;

            InterfaceUtils.ResetScene();
            pointsCloud3D = GenerateRandomVertices(verticesAmount);
            InterfaceUtils.GeneratePoints(pointPrefab, pointsCloud3D);
        }

        if (pointsCloud3D.Count > 0) {
            GUILayout.Label("Convex Hull 3D");

            if (GUILayout.Button("Incremental Convex Hull")) currentState = 1;
        }
    }

	static List<Vector3> GenerateRandomVertices(int verticesAmount) {
		Vector3 uperLeftCorner = new Vector3(-10, -10, 50);
		Vector3 lowerRightCorner = new Vector3(10, 10, 25);
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

	static public void GenerateMesh(List<Vector3> vertices, List<int> indices) {

        GameObject thisBuilding = GameObject.Find("Building");
        if (thisBuilding == null) {
            // Create a building game object
            thisBuilding = new GameObject ("Building");
        }

        var center = findCenter(vertices);
        var normals = new List<Vector3>();

        for (int i = 0; i < vertices.Count; i++) {
            normals.Add((vertices[i] - center).normalized);
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

        // mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        MeshRenderer rend = thisBuilding.GetComponent<MeshRenderer>();
        if (rend == null) {
            rend = thisBuilding.AddComponent<MeshRenderer>();
        }
        rend.material = new Material(Shader.Find("Standard"));
    }

    public static Vector3 findCenter(List<Vector3> verts) {
        Vector3 center = Vector3.zero;
        // Only need to check every other spot since the odd indexed vertices are in the air, but have same XZ as previous
        for (int i = 0; i < verts.Count; i++) {
            center += verts [i];
        }
        return center / verts.Count;
    }
}