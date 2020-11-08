using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ConvexHull3DTest
{
    [Test]
    public void GetRotationAffineMatrix3DTo2DTest()
    {
        List<Vector3> input = new List<Vector3>() {
            new Vector3(1, 1, 1),
            new Vector3(2, 1, 1),
            new Vector3(1, 1, 2),
        };

		Matrix4x4 M = ConvexHull3D.GetRotationAffineMatrix3DTo2D(input[0], input[1], input[2]);
        Assert.IsTrue(M.MultiplyPoint3x4(new Vector3(-2, 1, 7)) == new Vector3(-3, 6, 0));
    }
}
