using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This line enables the testing framework to call internal methods
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor")]

public class ConvexHull {
    
    public static float CalculateAngle(Vector2 s, Vector2 r) {
        float dot = Vector2.Dot(s, r);
        float det = s.magnitude * r.magnitude;
        return Mathf.Acos(dot / det);
    }
    
    /**
    * getLeftmostPoint
    * Return the left most point of a set of point
    */
    public static void getLeftmostPoint(List<Vector2> S, ref Vector2 leftmostPoint) {
        int imin = 0;
        Vector2 pmin = S[imin];
        for (int i = 1; i < S.Count; i++) {
            Vector2 p = S[i];
            if (p.x < pmin.x || (p.x == pmin.x && p.y < pmin.y)) {
                imin = i;
                pmin = p;
            }
        }

        leftmostPoint = S[imin];
    }

    /**
    * isLeft
    * Return true if s is on left of the (r, endpoint) line, else return false
    */
    public static bool isLeft(Vector2 s, Vector2 r, Vector2 end) {
        Vector2 a = new Vector2(end.x - r.x, end.y - r.y);
        Vector2 b = new Vector2(r.x - s.x, r.y - s.y);
        return (a.x * b.y - a.y * b.x > 0);
    }

    /**
    * Jarvis march - One of the simplest planar algorithms.
    *
    * To generate the convex polygon using Jarvis March algorithm
    *
    * S - is the set of points
    * P - will be the set of points which form the convex hull
    */
    public static void JarvisMarch(List<Vector2> S, ref List<Vector2> P) {
        Vector2 pointOnHull = new Vector2();
        Vector2 endpoint = new Vector2();

        // search for the first vertex of the convex hull
        ConvexHull.getLeftmostPoint(S, ref pointOnHull);

        int i = 0;
        do {
            P.Add(pointOnHull);  // add pivot
            endpoint = S[0];

            // search closest point to the left
            for (int j = 1; j < S.Count; j++) {
                if (endpoint == pointOnHull || ConvexHull.isLeft(S[j], P[i], endpoint)) {
                    endpoint = S[j];
                }
            }

            i++;
            pointOnHull = endpoint;  // update pivot
        } while (endpoint != P[0]);
    }

    public static float calc(Vector2 a, Vector2 b, Vector2 c) {
        float BAx = a.x - b.x;
        float BAy = a.y - b.y;
        float BCx = c.x - b.x;
        float BCy = c.y - b.y;
        return (BAx * BCy - BAy * BCx);
    }

    // https://www.tutorialspoint.com/convex-polygon-in-cplusplus
    public static bool isConvex(List<Vector2> points) {
        bool neg = false;
        bool pos = false;
        int n = points.Count;
        for (int i = 0; i < n; i++) {
            int a = i;
            int b = (i + 1) % n;
            int c = (i + 2) % n;
            float crossProduct = calc(points[a], points[b], points[c]);
            if (crossProduct < 0)
                neg = true;
            else if (crossProduct > 0)
                pos = true;
            if (neg && pos) return false;
        }
        return true;
    } 

    public static Vector2 getBarycenter(List<Vector2> S) {
        Vector2 output = new Vector2(0, 0);
        for (int i = 0; i < S.Count; i++) {
            output = output + S[i];
        }
        output = output / S.Count;
        return output;
    }
    
    class CompareAngle : IComparer<Vector2>  {
        Vector2 center;

        public CompareAngle(Vector2 center) {
            this.center = center;
        }

        public int Compare(Vector2 i1, Vector2 i2) { 
            float PI = Mathf.PI;
            float EPSILON = 0.00001f;

            Vector2 tmp1 = i1 - center;
            float a1 = ConvexHull.CalculateAngle(tmp1, new Vector2(1, 0));
            if (tmp1.y < 0) {
                a1 = (PI - a1) + PI;
            }

            Vector2 tmp2 = i2 - center;
            float a2 = ConvexHull.CalculateAngle(tmp2, new Vector2(1, 0));
            if (tmp2.y < 0) {
                a2 = (PI - a2) + PI;
            }

            if (Mathf.Abs(a1 - a2) < EPSILON) {
                return tmp1.magnitude.CompareTo(tmp2.magnitude);
            }
            return a1.CompareTo(a2); 
        } 
    }

    public static void sortByAngle(ref List<Vector2> S, Vector2 center) {
        CompareAngle gg = new CompareAngle(center);
        S.Sort(gg); 
    }

    public static int mod(int a, int b) {
        int r = a % b;
        return r < 0 ? r + b : r;
    }

    class CompareYCoordinates : IComparer<Vector2>  {
        public int Compare(Vector2 lhs, Vector2 rhs) { 
            if (lhs[1] != rhs[1])
                return lhs[1].CompareTo(rhs[1]);

            /* Otherwise, fall back to comparing by x coordinates. */
            return lhs[0].CompareTo(rhs[0]);
        } 
    }

    class CompareByAngle : IComparer<Vector2>  {
        Vector2 origin;

        public CompareByAngle(Vector2 origin) {
            this.origin = origin;
        }

        public int Compare(Vector2 lhs, Vector2 rhs) { 
            Vector2 one = lhs - origin;
            Vector2 two = rhs - origin;

            float normOne = one.magnitude;
            float normTwo = two.magnitude;
            float negCosOne = -one[0] / normOne;
            float negCosTwo = -two[0] / normTwo;

            if (negCosOne != negCosTwo) return negCosOne.CompareTo(negCosTwo);

            return normOne.CompareTo(normTwo);
        } 
    }

    public static void sortByAngle2(ref List<Vector2> S, Vector2 center) {
        CompareByAngle gg = new CompareByAngle(center);
        S.Sort(gg); 
    }

    public static void GrahamScan(List<Vector2> S, ref List<Vector2> P) {
        if (S.Count < 3) {
            P = S;
            return;
        }
        
        List<Vector2> tmp = new List<Vector2>(S);
        tmp.Sort(new CompareYCoordinates()); 
        
        int minY = S.FindIndex(x => x == tmp[0]);
        int next = minY + 1;

        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i < minY; i++)   {
            points.Add(S[i]);
        }   

        for(int i = next; i < S.Count; i++) {
            points.Add(S[i]);
        }        

        points.Sort(new CompareByAngle(S[minY])); 

        points.Add(S[minY]);

        List<Vector2> result = new List<Vector2>();
        result.Add(S[minY]);
        result.Add(points[0]);

        for (int i = 1; i < points.Count; ++i) {
            while (true) {
                Vector2 last = result[result.Count - 1] - result[result.Count - 2];
                Vector2 curr = points[i] - result[result.Count - 1];

                if (last[0] * curr[1] - last[1] * curr[0] >= 0) break;
                result.RemoveAt(result.Count - 1);
            }
            result.Add(points[i]);
        }
        result.RemoveAt(result.Count - 1);
        P = result;
    }

    public static void OldGrahamScan(List<Vector2> S, ref List<Vector2> P) {
        P = S;
        ConvexHull.sortByAngle(ref P, ConvexHull.getBarycenter(S));  // we sort all points compared to barycenter

        // trier S par angles, par rapport au barycentre
        // si meme angle, prendre le plus éloigné
        int Sinit = 0;
        int pivot = Sinit;
        bool avance;

        do {
            // update barycenter, to use it as a fourth point and orient the polygon
            List<Vector2> tmp = new List<Vector2> () {
                P[mod(pivot - 1, P.Count)],
                P[mod(pivot, P.Count)],
                P[mod(pivot + 1, P.Count)],
                ConvexHull.getBarycenter(P),
            };

            if (ConvexHull.isConvex(tmp)) {
                pivot = mod(pivot + 1, P.Count);
                avance = true;
            } else {
                Sinit = mod(pivot - 1, P.Count);
                P.RemoveAt(pivot);
                pivot = Sinit;
                avance = false;
            }

        } while (pivot != Sinit || !avance);
    }
}
