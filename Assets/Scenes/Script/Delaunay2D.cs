using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Edge {
    public Vector2 start, end;

    public Edge(Vector2 s, Vector2 e) {
        start = s;
        end = e;
    }

    public bool Contains(Vector2 point) {
        return (start == point || end == point);
    }

    public bool isEqual(Edge edge) {
        return ((start == edge.start && end == edge.end) ||
                (start == edge.end && end == edge.start));
    }
}

/*
0) créer un nuage de point Pi
1) créer un super triangle qui englobe tout les points
2) selection d'un point p
3) relier P à tout les points du super triangle (decomposition en 3 triangles)
4) Pour chaque point P sans triangle :
	Pour chaque triangle T :
		Si ( P est dans le cercle circonscrit de T)
			"On a 2 triangles à delete"
			flag le triangle
	Si(deux triangles ou plus sont flag) {
		Supprime les arretes commune des triangles flaggés
	}  	
	Relie P à tous les points des triangles flagué ( meme si 1 seul )
*/

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
        // On crée un super triangle qui englobe tout les points
        Triangle superTriangle = GetSuperTriangle(points);
        triangles.Add(superTriangle);

        foreach (var p in points) {
            // ***** Split *****

            // Find all the triangles where the point is in their circumscribed circle
            var hit_triangles = triangles.Where(t => {
                Circle c = t.CircumscribedCircle();
                return c.Contains(p);
            }).ToList();

            IEnumerable<Edge> edge_stack = Enumerable.Empty<Edge>();
            foreach (var ht in hit_triangles) {
                // On stock les arêtes
                edge_stack = edge_stack.Concat(ht.edges);

                // On supprime le triangle
                int index = triangles.FindIndex(t => t.isEqual(ht));
                triangles.RemoveAt(index);

                // On le split en trois
                for(var k = 0; k < 3; k++) {
                    Triangle t = new Triangle(
                        ht.vertices[k],
                        ht.vertices[(k + 1) % 3],
                        p
                    );

                    triangles.Add(t);
                }
            }
            
            // ***** Flip *****

            // Soit ABC et ABD deux triangles contenant un côté commun (appelons-le côté AB).
            // Si le point D est dans le cercle circonscrit du triangle ABC, on flip l'arête commune et on ajoute les arêtes AD / DB / BC / CA dans la pile.
            // Autrement dit, on supprime les triangles qui ont l'arête commune et on met une nouvelle liste d'arête dans la pile
            // De plus, on ajoute les triangles nouvellement créé à la liste
            foreach (var edge in edge_stack) {

                // On trouve les triangles qui partage le coté courant
                var common_edge_triangles = triangles.Where(t => t.hasEdge(edge)).ToList();

                // On ignorer s'il n'y a pas au minimum deux triangles
                if (common_edge_triangles.Count < 2) {
                    continue;
                }

                var triangle_ABC = common_edge_triangles[0];
                var triangle_ABD = common_edge_triangles[1];

                // Si les triangles sélectionnés sont identiques, on les supprime et on passe au coté suivant
                if (triangle_ABC.isEqual(triangle_ABD)) {
                    int index_ABC = triangles.FindIndex(t => t.isEqual(triangle_ABC));
                    triangles.RemoveAt(index_ABC);
                    int index_ABD = triangles.FindIndex(t => t.isEqual(triangle_ABD));
                    triangles.RemoveAt(index_ABD);
                    continue;
                }

                var point_A = edge.start;
                var point_B = edge.end;

                // On récupère le points qui n'est pas sur l'arête, parmi les sommets du triangle ABC (le point C)
                var point_C = triangle_ABC.GetOtherPoint(edge);

                // Pareil pour le triangle ABD (le point D)
                var point_D = triangle_ABD.GetOtherPoint(edge);

                // On récupère le cercle circonscrit du triangle ABC
                Circle external_circle = triangle_ABC.CircumscribedCircle();

                // Si D est dans le cercle
                if (external_circle.Contains(point_D)) {

                    // On supprime les triangles de la liste des triangles
                    int index_ABC = triangles.FindIndex(t => t.isEqual(triangle_ABC));
                    triangles.RemoveAt(index_ABC);
                    int index_ABD = triangles.FindIndex(t => t.isEqual(triangle_ABD));
                    triangles.RemoveAt(index_ABD);

                    // On crée les deux triangles en flipant l'arête
                    var triangle_ACD = new Triangle(point_A, point_C, point_D);
                    var triangle_BCD = new Triangle(point_B, point_C, point_D);

                    // On les ajout à la liste de triangle
                    triangles.Add(triangle_ACD);
                    triangles.Add(triangle_BCD);

                    // Et on ajoute les côtés des triangle ABC et ABD, à la pile d'arêtes
                    var other_edge1 = triangle_ABC.GetOtherEdge(edge);
                    var other_edge2 = triangle_ABD.GetOtherEdge(edge);

                    edge_stack = edge_stack.Concat(other_edge1);
                    edge_stack = edge_stack.Concat(other_edge2);
                }
            }
        }

        // Enfin, on supprime tous les triangles qui ont 1 point commun avec le super triangle
        triangles = triangles.Where(t => {
            for(var i = 0; i < 3; i++) {
                for(var j = 0; j < 3; j++) {
                    if (t.vertices[i] == superTriangle.vertices[j]) {
                        return false;
                    }
                }
            }
            return true;
        }).ToList();
    }
}