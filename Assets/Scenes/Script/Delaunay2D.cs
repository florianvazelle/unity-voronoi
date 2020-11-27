using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Delaunay2D {

    public static Triangle GetSuperTriangle(List<Vector2> points){
        // on récupère les bornes
        var left = points.Min(p => p.x);
        var right = points.Max(p => p.x);
        var top = points.Max(p => p.y);
        var bottom = points.Min(p => p.y);
        
        // Cercle circonscrit du rectangle entourant tout les points
        var center = new Vector2((left + right) / 2, (top + bottom) / 2);
        var topleft = new Vector2(left, top);
        var radius = Vector2.Distance(center, topleft);

        // Le triangle du cercle circonscrit
        var x1 = center.x - Mathf.Sqrt(3) * radius;
        var y1 = center.y - radius;
        var p1 = new Vector2(x1, y1);

        var x2 = center.x + Mathf.Sqrt(3) * radius;
        var y2 = center.y - radius;
        var p2 = new Vector2(x2, y2);

        var x3 = center.x;
        var y3 = center.y + 2 * radius;
        var p3 = new Vector2(x3, y3);

        return new Triangle(p1, p2, p3);
    }

    public static void Delaunay(List<Vector2> points, ref List<Triangle> triangles) {
        List<Triangle> tmp_triangles;

        triangles.Add(GetSuperTriangle(points));

        for(var i = 0; i < points.Count; i++) {
            tmp_triangles = new List<Triangle>();

            for(var j = 0; j < triangles.Count; j++) {
                Triangle currentTriangle = triangles[j];
                Circle circumcircle = currentTriangle.CircumscribedCircle();

                if(circumcircle.Contains(points[i])) {

                    for(var k = 0; k < 3; k++) {
                        Triangle tria = new Triangle(
                            currentTriangle.vertices[k],
                            currentTriangle.vertices[(k + 1) % 3],
                            points[i]
                        );

                        int index = tmp_triangles.FindIndex(t => t.Equals(tria));
                        if(index == -1) {
                            tmp_triangles.Add(tria);
                        } else {
                            tmp_triangles.RemoveAt(index);
                        }
                    }

                    triangles.RemoveAt(j);
                }
            }

            for(int j = 0; j < tmp_triangles.Count; j++) {
                triangles.Add(tmp_triangles[j]);
            } 
        }

        for(int i = 0; i < triangles.Count; i++) {
            Debug.Log(triangles[i].vertices[0] + " " + triangles[i].vertices[1] + " " + triangles[i].vertices[2]);
        } 
    }
}
