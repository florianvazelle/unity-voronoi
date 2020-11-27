using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circle {
    public Vector2 center;
    public float radius;

    // https://github.com/photonstorm/phaser/blob/master/src/geom/circle/Contains.js
    public bool Contains(Vector2 point) {
        //  Check if x/y are within the bounds first
        if (radius > 0 && point.x >= center.x - radius && point.x <= center.x + radius && point.y >= center.y - radius && point.y <= center.y + radius) {
            var dx = (center.x - point.x) * (center.x - point.x);
            var dy = (center.y - point.y) * (center.y - point.y);

            return (dx + dy) <= (radius * radius);
        } else {
            return false;
        }
    }
}