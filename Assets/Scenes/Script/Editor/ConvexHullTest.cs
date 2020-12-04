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
    public void GrahamScanTest1()
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
            new Vector2(0, 0),
            new Vector2(4, 4),
            new Vector2(3, 6),
            new Vector2(-3, 6),
            new Vector2(-4, 4),
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
    public void GrahamScanTest2()
    {
        // It's the input point cloud
        List<Vector2> input = new List<Vector2> ()
        {
            new Vector2(0.8317833f, -2.561209f),
            new Vector2(-2.259331f, 3.335994f),
            new Vector2(-6.651563f, 2.19939f),
            new Vector2(7.226954f, 5.716131f),
            new Vector2(-5.955015f, 3.887402f),
            new Vector2(2.251951f, -3.263929f),
            new Vector2(9.695553f, -3.278175f),
            new Vector2(1.709259f, -3.960266f),
            new Vector2(-10.44734f, 3.029336f),
            new Vector2(-0.7599373f, -0.6391747f)
        };

        // It's the expected polygon
        // In Graham Scan, the points are sort by angle (normaly clockwise sense and begining by the right most point)
        List<Vector2> expected = new List<Vector2> ()
        {
            new Vector2(1.709259f, -3.960266f),
            new Vector2(9.695553f, -3.278175f),
            new Vector2(7.226954f, 5.716131f),
            new Vector2(-5.955015f, 3.887402f),
            new Vector2(-10.44734f, 3.029336f)
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
    public void GrahamScanTest3()
    {
        // It's the input point cloud
        List<Vector2> input = new List<Vector2> ()
        {
            new Vector2(1.013885f, 2.876855f),
            new Vector2(0.1158292f, 3.346232f),
            new Vector2(-1.707805f, 4.502964f),
            new Vector2(-0.2397112f, 1.057849f),
            new Vector2(0.7197906f, 5.062522f)
        };

        // It's the expected polygon
        // In Graham Scan, the points are sort by angle (normaly clockwise sense and begining by the right most point)
        List<Vector2> expected = new List<Vector2> ()
        {
            new Vector2(-0.2397112f, 1.057849f),
            new Vector2(1.013885f, 2.876855f),
            new Vector2(0.7197906f, 5.062522f),
            new Vector2(-1.707805f, 4.502964f)
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
        Vector2 output = ConvexHull.GetBarycenter(input);
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

        ConvexHull.SortByAngle(ref inoutput, ConvexHull.GetBarycenter(inoutput));

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
