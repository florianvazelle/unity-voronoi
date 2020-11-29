using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;

public static class InterfaceUtils 
{
    public const string MESH_NAME = "Building";
    public const string POINT_NAME = "Point";

    // Remove all prefab points
    static public void ResetScene() {
        GameObject tmp = GameObject.Find(POINT_NAME + "(Clone)");
        while(tmp != null) {
            DestroyImmediate(tmp);
            tmp = GameObject.Find(POINT_NAME + "(Clone)");
        }

        ResetMesh();
    }

    // Remove the Building mesh
    static public void ResetMesh() {
        GameObject thisBuilding = GameObject.Find(MESH_NAME);
        if (thisBuilding != null) {
            DestroyImmediate(thisBuilding);
        }
    }

    // Instanciate gameObject for a list of 3D points
    static public void GeneratePoints(GameObject prefab, List<Vector3> vertices) {
        prefab.name = POINT_NAME;
		for (int i = 0; i < vertices.Count; i++) {
			Instantiate(prefab, vertices[i], Quaternion.identity);
		}
    }

    // Return the list of point position in the 3D scene
    static public List<Vector3> UpdateVertices() {
        List<Vector3> newPoints3D = new List<Vector3>();
        GameObject[] allGOs = FindObjectsOfType<GameObject>();
        foreach(var go in allGOs) {
            if (go.name == "Point(Clone)") {
                newPoints3D.Add(go.transform.position);
            }
        }
        return newPoints3D;
    }

    // Generate random points in a 2D plane
    static public List<Vector3> GenerateRandomVertices(int verticesAmount) {
        Vector3 uperLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(100, 100, 10)); // -10.0f if bugs
        Vector3 lowerRightCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 100, Screen.height - 100, 10)); // -10.0f if bugs
        
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

    // Generate random points in a 3D plane
    static public List<Vector3> GenerateRandom3DVertices(int verticesAmount) {
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

    // Sort in clock wise an array of 2D points
    static public void SortInClockWise(ref List<Vector2> points2D) {
        ConvexHull.SortByAngle(ref points2D, ConvexHull.GetBarycenter(points2D));
        points2D.Reverse();
    }
    
    // Sort in clock wise an array of 3D points
    static public void SortInClockWise(ref List<Vector3> points3D) {
        List<Vector2> points2D = ConvertListVector3ToVector2(points3D);
        SortInClockWise(ref points2D);
        points3D = ConvertListVector2ToVector3(points2D); 
    }

    static public List<Vector3> ConvertListVector2ToVector3(List<Vector2> points2D) {
        List<Vector3> points3D = new List<Vector3>();
        for (int i = 0; i < points2D.Count; i++) {
            points3D.Add(points2D[i]);
        }
        return points3D;
    }

    static public List<Vector2> ConvertListVector3ToVector2(List<Vector3> points3D) {
        List<Vector2> points2D = new List<Vector2>();
        for (int i = 0; i < points3D.Count; i++) {
            points2D.Add(points3D[i]);
        }
        return points2D;
    }

    static public void GenerateMesh(List<Vector3> vertices, List<int> indices) {

        GameObject thisBuilding = GameObject.Find(MESH_NAME);
        if (thisBuilding == null) {
            // Create a building game object
            thisBuilding = new GameObject (MESH_NAME);
        }

        var center = FindCenter(vertices);
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

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        MeshRenderer rend = thisBuilding.GetComponent<MeshRenderer>();
        if (rend == null) {
            rend = thisBuilding.AddComponent<MeshRenderer>();
        }
        rend.material = new Material(Shader.Find("SuperSystems/Wireframe-Shaded-Unlit"));
    }

    public static Vector3 FindCenter(List<Vector3> verts) {
        Vector3 center = Vector3.zero;
        // Only need to check every other spot since the odd indexed vertices are in the air, but have same XZ as previous
        for (int i = 0; i < verts.Count; i++) {
            center += verts [i];
        }
        return center / verts.Count;
    }
}