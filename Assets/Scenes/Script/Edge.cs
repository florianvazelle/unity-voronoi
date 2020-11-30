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