using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Circle {
    public Vector2 center;
    public float radius;

    public bool Contains(Vector2 point) {
        //  Check if x/y are within the bounds first
        if (radius > 0 && x >= center.x - radius && x <= center.x + radius && y >= center.y + radius && y <= center.y + radius) {
            var dx = (center.x - x) * (center.x - x);
            var dy = (center.y - y) * (center.y - y);

            return (dx + dy) <= (radius * radius);
        } else {
            return false;
        }
    }
}