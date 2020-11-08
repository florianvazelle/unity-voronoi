using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConvexHull3D {

	// https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates#answer-49771112
	public static Matrix4x4 GetRotationAffineMatrix3DTo2D(Vector3 A, Vector3 B, Vector3 C) {
		Vector3 N = Vector3.Cross(B - A, C - A);
		Vector3 U = Vector3.Normalize(B - A);
		Vector3 uN = Vector3.Normalize(N);
		Vector3 V = -Vector3.Cross(U, uN); 

		Vector3 u = A + U;
		Vector3 v = A + V;
		Vector3 n = A + uN;

		Matrix4x4 S = new Matrix4x4(
			new Vector4(A.x, A.y, A.z, 1),
			new Vector4(u.x, u.y, u.z, 1),
			new Vector4(v.x, v.y, v.z, 1),
			new Vector4(n.x, n.y, n.z, 1)
		);

		Matrix4x4 D = new Matrix4x4(
			new Vector4(0, 0, 0, 1),
			new Vector4(1, 0, 0, 1),
			new Vector4(0, 1, 0, 1),
			new Vector4(0, 0, 1, 1)
		);

		return D * S.inverse;
	}

	public static void SortByAngle3D(List<Vector3> input) {
		Matrix4x4 M = GetRotationAffineMatrix3DTo2D(input[0], input[1], input[2]);

		var input2D = input
			.Select((x, i) => (Vector2)M.MultiplyPoint3x4(input[i]))
			.ToList();

		var Comparer = new ConvexHull.CompareByAngle(ConvexHull.GetBarycenter(input2D));
		
		input = input2D
			.Select((x, i) => new KeyValuePair<int, Vector3>(i, x))
			.OrderBy(x => x.Value, Comparer)
			.Select(x => input[x.Key])
			.ToList();
	}

	/* E[i, j] indicates which (up to two) other points combine with the edge i and
	 * j to make a face in the hull.  Only defined when i < j.
	 */
	struct twoset {
		twoset(int x = -1, int y = -1) { a = x; b = y; }
		public void Insert(int x) { if (a == -1) a = x; else b = x; }
		public void Erase(int x) { if (a == x) a = -1; else b = -1; }
		public bool Size() { return (a != -1) && (b != -1); }
		public int a, b;
	};

	static twoset[,] E;

	public struct face {
		public Vector3 norm;
		public float disc;
		public int[] I;
	};

	/* Compute the half plane {x : c^T norm < disc}
	 * defined by the three points S[i], S[j], S[k] where
	 * S[inside_i] is considered to be on the 'interior' side of the face. */
	static face MakeFace(int i, int j, int k, int inside_i, List<Vector3> S) {
		E[i, j].Insert(k);
		E[i, k].Insert(j);
		E[j, k].Insert(i);

		Vector3 norm = Vector3.Cross(S[j] - S[i], S[k] - S[i]);

		face f = new face{
			I = new int[3] { i, j, k },
			norm = norm,
			disc = Vector3.Dot(norm, S[i])
		};

		if (Vector3.Dot(f.norm, S[inside_i]) > f.disc) {
			f.norm = -f.norm;
			f.disc = -f.disc;
		}
		return f;
	}

	public static void IncrementalConvexHull(List<Vector3> S, ref List<Vector3> P, ref List<int> tris) {
		/* Initially construct the hull as containing only the first four points. */
		face f;
		List<face> faces = new List<face>();
		E = new twoset[1010, 1010];
		for (int i = 0; i < 4; i++)
			for (int j = i + 1; j < 4; j++)
				for (int k = j + 1; k < 4; k++) {
					faces.Add(MakeFace(i, j, k, 6 - i - j - k, S));
				}

		/* Now add a point into the hull one at a time. */
		for (int i = 4; i < S.Count; i++) {
			/* Find and delete all faces with their outside 'illuminated' by this
			 * point. */
			for (int j = 0; j < faces.Count; j++) {
				f = faces[j];
				if (Vector3.Dot(f.norm, S[i]) > f.disc) {
					E[f.I[0], f.I[1]].Erase(f.I[2]);
					E[f.I[0], f.I[2]].Erase(f.I[1]);
					E[f.I[1], f.I[2]].Erase(f.I[0]);
					faces[j--] = faces[faces.Count - 1];
					faces.RemoveAt(faces.Count - 1);
				}
			}
			/* Now for any edge still in the hull that is only part of one face
			 * add another face containing the new point and that edge to the hull. */
			int nfaces = faces.Count;
			for (int j = 0; j < nfaces; j++) {
				f = faces[j];
				for (int a = 0; a < 3; a++)
					for (int b = a + 1; b < 3; b++) {
						int c = 3 - a - b;
						if (E[f.I[a], f.I[b]].Size())
							continue;
						faces.Add(MakeFace(f.I[a], f.I[b], i, f.I[c], S));
					}
			}
		}

		/* Iterate through each point and check if it is on a face of the hull */
		for (int i = 0; i < S.Count; i++) {
			Vector3 v = S[i];

			for (int j = 0; j < faces.Count; j++) {
				float d = faces[j].disc - Vector3.Dot(faces[j].norm, v);
				if (d == 0) {
					P.Add(v);
					break;
				}
			}
		}

		/* Reassign face index for P */
		for (int i = 0; i < P.Count; i++) {
			Vector3 v = P[i];

			for (int j = 0; j < faces.Count; j++) {
				if (S[faces[j].I[0]] == v) faces[j].I[0] = i;
				if (S[faces[j].I[1]] == v) faces[j].I[1] = i;
				if (S[faces[j].I[2]] == v) faces[j].I[2] = i;
			}
		}

		/* Construct indices array */
		var vCenter = Interface3D.findCenter(P);
		for (int j = 0; j < faces.Count; j++) {
			face face = faces[j];

			List<Vector3> input = new List<Vector3>() { 
				P[face.I[0]],
				P[face.I[1]],
				P[face.I[2]]
			};
			
			var idx = input.Select((x, i) => face.I[i]).ToList();

			// verify if face is in clockwise
			var surfaceNormal = Vector3.Cross(input[1] - input[0], input[2] - input[0]).normalized;
			Vector3 center = Interface3D.findCenter(input);
			if (Vector3.Dot(surfaceNormal, center - vCenter) < 0) {
				idx.Reverse();
			}

			tris.AddRange(idx);
		}
	}
}