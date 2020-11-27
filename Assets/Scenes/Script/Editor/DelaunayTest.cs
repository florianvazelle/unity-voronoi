using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DelaunayTest
{
    [Test]
    public void GetSuperTriangle()
    {
        List<Vector2> input = new List<Vector2> ()
        {
            new Vector2(0, 0),
            new Vector2(0, 4),
            new Vector2(4, 4),
            new Vector2(1, 4),
            new Vector2(0, 2),
            new Vector2(3, 6),
            new Vector2(-3, 6),
            new Vector2(-4, 4),
            new Vector2(1, 5),
            new Vector2(-1, 3)
        };

        Triangle triangle = Delaunay2D.GetSuperTriangle(input);

        for(int i = 0; i < input.Count; i++) {
            Assert.IsTrue(triangle.Contains(input[i]));
        }
    }
}
