using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ConvexHullTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void JarvisMarchTest()
    {
        // It's the input point cloud
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

        // It's the expected polygon
        // In Jarvis march, the points are sort in anticlockwise, begin by the left most point
        List<Vector2> expected = new List<Vector2> ()
        {
            new Vector2(-4, 4),
            new Vector2(0, 0),
            new Vector2(4, 4),
            new Vector2(3, 6),
            new Vector2(-3, 6)
        };

        // we call JarvisMarch function
        List<Vector2> output = new List<Vector2> ();
        ConvexHull.JarvisMarch(input, ref output);

        // check if all value is same
        Assert.IsTrue(output.Count == expected.Count);
        for (int i = 0; i < output.Count; i++) {
            Assert.IsTrue(expected[i] == output[i]);
        }
    }

    [Test]
    public void GrahamScanTest()
    {
        // It's the input point cloud
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

        // It's the expected polygon
        // In Graham Scan, the points are sort by angle (normaly clockwise sense and begining by the right most point)
        List<Vector2> expected = new List<Vector2> ()
        {
            new Vector2(4, 4),
            new Vector2(3, 6),
            new Vector2(-3, 6),
            new Vector2(-4, 4),
            new Vector2(0, 0),
        };

        // we call GrahamScan function
        List<Vector2> output = new List<Vector2> ();
        ConvexHull.GrahamScan(input, ref output);

        // check if all value is same
        Assert.IsTrue(output.Count == expected.Count);
        for (int i = 0; i < output.Count; i++) {
            Assert.IsTrue(expected[i] == output[i]);
        }
    }

    [Test]
    public void BarycenterTest()
    {
        List<Vector2> input = new List<Vector2> () 
        {
            new Vector2(-6, 6),
            new Vector2(6, -6),
            new Vector2(6, 6)
        };
        Vector2 expected = new Vector2(2, 2);
        Vector2 output = ConvexHull.getBarycenter(input);
        Assert.IsTrue(expected == output);
    }

    [Test]
    public void SortByAngleTest()
    {
        List<Vector2> inoutput = new List<Vector2> () 
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
        
        List<Vector2> expected = new List<Vector2> () 
        {
            new Vector2(4, 4),
            new Vector2(1, 4),
            new Vector2(3, 6),
            new Vector2(1, 5),
            new Vector2(0, 4),
            new Vector2(-3, 6),
            new Vector2(-4, 4),
            new Vector2(-1, 3),
            new Vector2(0, 2),
            new Vector2(0, 0)
        };

        ConvexHull.sortByAngle(ref inoutput, ConvexHull.getBarycenter(inoutput));

        Assert.IsTrue(inoutput.Count == expected.Count);
        for (int i = 0; i < inoutput.Count; i++) {
            Assert.IsTrue(expected[i] == inoutput[i]);
        }
    }

    [Test]
    public void AngleTest()
    {
        float output = ConvexHull.CalculateAngle(new Vector2(2, 2), new Vector2(0, 3));
        float expected = Mathf.PI / 4.0f;
        Assert.IsTrue(Mathf.Abs(expected - output) < 0.00001f);
    }
}
