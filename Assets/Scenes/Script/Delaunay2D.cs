using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            Flip(ref edge_stack, ref triangles);
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


    private static bool Snip(List<Vector2> contour, int u, int v, int w, int n, int[] V) {
        float Ax, Ay, Bx, By, Cx, Cy;

        Ax = contour[V[u]].x;
        Ay = contour[V[u]].y;

        Bx = contour[V[v]].x;
        By = contour[V[v]].y;

        Cx = contour[V[w]].x;
        Cy = contour[V[w]].y;

        Triangle t = new Triangle(contour[V[u]], contour[V[v]], contour[V[w]]);

        if (0.000001 > (((Bx-Ax)*(Cy-Ay)) - ((By-Ay)*(Cx-Ax)))) return false;

        for (int p = 0; p < n; p++) {
            if( (p == u) || (p == v) || (p == w) ) continue;
            if (t.Contains(contour[V[p]])) return false;
        }

        return true;
    }

    static float Area(List<Vector2> contour) {
        int n = contour.Count;

        float A = 0.0f;

        for(int p = n - 1, q = 0; q < n; p = q++) {
            A += contour[p].x * contour[q].y - contour[q].x * contour[p].y;
        }

        return A * 0.5f;
    }

    public static void Regular(List<Vector2> contour, ref List<Triangle> result) {
        Interface2D.SortInClockWise(ref contour);
        /* allocate and initialize list of Vertices in polygon */

        int n = contour.Count;
        if ( n < 3 ) return;

        int[] V = new int[n];

        /* we want a counter-clockwise polygon in V */

        if (0.0f < Area(contour))
            for (int v = 0; v < n; v++) V[v] = v;
        else
            for(int v = 0; v < n; v++) V[v] = (n - 1) - v;

        int nv = n;

        /*  remove nv-2 Vertices, creating 1 triangle every time */
        int count = 2 * nv;   /* error detection */

        for(int m = 0, v = nv - 1; nv > 2; )
        {
            /* if we loop, it is probably a non-simple polygon */
            if (0 >= (count--))
            {
                //** Triangulate: ERROR - probable bad polygon!
                return;
            }

            /* three consecutive vertices in current polygon, <u,v,w> */
            int u = v  ; if (nv <= u) u = 0;  /* previous */
            v = u + 1; if (nv <= v) v = 0;      /* new v    */
            int w = v + 1; if (nv <= w) w = 0;  /* next     */

            if (Snip(contour, u, v, w, nv, V))
            {
                int a,b,c,s,t;

                /* true names of the vertices */
                a = V[u]; b = V[v]; c = V[w];

                /* output Triangle */
                result.Add(new Triangle(
                    contour[a],
                    contour[b],
                    contour[c]
                ));

                m++;

                /* remove v from remaining polygon */
                for(s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t]; nv--;

                /* resest error detection counter */
                count = 2 * nv;
            }
        }
    }

    /**
     * Flip Triangle to Delaunay triangulation
     */
    public static void FlipToDelaunay(ref List<Triangle> triangles) {
        var points = new List<Vector2>();
        foreach (var t in triangles) {
            points.AddRange(t.vertices);
        }
        
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
            }
            
            // ***** Flip *****
            Flip(ref edge_stack, ref triangles);
            
        }
    }

    /**
     * Flip
     */
    public static void Flip(ref IEnumerable<Edge> edge_stack, ref List<Triangle> triangles) {
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
}