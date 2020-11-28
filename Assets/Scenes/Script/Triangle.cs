using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Triangle {
    public List<Vector2> vertices;
    public List<Edge> edges;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3) {
        vertices = new List<Vector2>() {
            p1, p2, p3
        };

        edges = new List<Edge>() {
            new Edge(p1, p2),
            new Edge(p2, p3),
            new Edge(p3, p1)
        };
    }

    public bool isEqual(Triangle triangle) {
        Vector2 p11 = vertices[0];
        Vector2 p21 = vertices[1];
        Vector2 p31 = vertices[2];

        Vector2 p12 = triangle.vertices[0];
        Vector2 p22 = triangle.vertices[1];
        Vector2 p32 = triangle.vertices[2];

        return ((p11 == p12 && p21 == p22 && p31 == p32) ||
                (p11 == p12 && p21 == p32 && p31 == p22) ||
                (p11 == p22 && p21 == p12 && p31 == p32) ||
                (p11 == p22 && p21 == p32 && p31 == p12) || 
                (p11 == p32 && p21 == p22 && p31 == p12) || 
                (p11 == p32 && p21 == p12 && p31 == p22));
    }

    // Test si le triangle possède cette arête
    public bool hasEdge(Edge edge) {
        return edges.Count(e => e.isEqual(edge)) > 0;
    }

    // Retourne le point du triangle qui n'appartient pas a l'arête
    public Vector2 GetOtherPoint(Edge edge) {
        for (var i = 0; i < vertices.Count; i++) {
            if (!edge.Contains(vertices[i])) {
                return vertices[i];
            }
        }
        // Techniquement cela ne passe jamais ici (edge = 2 points et triangle = 3 points)
        return Vector2.zero;
    }

    // Retourne les arêtes du triangle qui ne corresponde pas
    public List<Edge> GetOtherEdge(Edge edge) {
        return edges.Where(e => !e.isEqual(edge)).ToList();
    }

    public Circle CircumscribedCircle() {
        Vector2 a = vertices[0];
        Vector2 b = vertices[1];
        Vector2 c = vertices[2];

        float d = 2 * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
        float ux = ((a.x * a.x + a.y * a.y) * (b.y - c.y) + (b.x * b.x + b.y * b.y) * (c.y - a.y) + (c.x * c.x + c.y * c.y) * (a.y - b.y)) / d;
        float uy = ((a.x * a.x + a.y * a.y) * (c.x - b.x) + (b.x * b.x + b.y * b.y) * (a.x - c.x) + (c.x * c.x + c.y * c.y) * (b.x - a.x)) / d;

        Vector2 center = new Vector2(ux, uy);
        return new Circle() {
            center = center,
            radius = Vector2.Distance(center, b)
        };
    }

    // https://github.com/photonstorm/phaser/blob/master/src/geom/triangle/Contains.js
    public bool Contains(Vector2 point) {
        Vector2 a = vertices[0];
        Vector2 b = vertices[1];
        Vector2 c = vertices[2];

        var v0x = c.x - a.x;
        var v0y = c.y - a.y;

        var v1x = b.x - a.x;
        var v1y = b.y - a.y;

        var v2x = point.x - a.x;
        var v2y = point.y - a.y;

        var dot00 = (v0x * v0x) + (v0y * v0y);
        var dot01 = (v0x * v1x) + (v0y * v1y);
        var dot02 = (v0x * v2x) + (v0y * v2y);
        var dot11 = (v1x * v1x) + (v1y * v1y);
        var dot12 = (v1x * v2x) + (v1y * v2y);

        // Compute barycentric coordinates
        var bar = ((dot00 * dot11) - (dot01 * dot01));
        var inv = (bar == 0) ? 0 : (1 / bar);
        var u = ((dot11 * dot02) - (dot01 * dot12)) * inv;
        var v = ((dot00 * dot12) - (dot01 * dot02)) * inv;

        return (u >= 0 && v >= 0 && (u + v < 1));
    }
}