using System.Numerics;
using Plugin.Core;
using Utility.Mathematics.Extensions;
using Wpf.Common;

namespace CoreTests;

public class ExtensionTests
{
    [Fact]
    public void Test1()
    {
        var v = new System.Numerics.Vector3(1.0f, 2.0f, 3.0f);

        var point3D = v.ToPoint3D();

        Assert.Equal(1.0, point3D.X);
        Assert.Equal(2.0, point3D.Y);
        Assert.Equal(3.0, point3D.Z);
    }

    [Fact]
    public void ToVector3D_ConvertsCorrectly()
    {
        // Arrange
        var vector3 = new Vector3(1.0f, 2.0f, 3.0f);

        // Act
        var result = vector3.ToVector3D();

        // Assert
        Assert.Equal(1.0, result.X);
        Assert.Equal(2.0, result.Y);
        Assert.Equal(3.0, result.Z);
    }

    [Fact]
    public void GetMinMax_ReturnsCorrectValues()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new(1.0f, 2.0f, 3.0f),
            new(4.0f, 5.0f, 6.0f),
            new(-1.0f, -2.0f, -3.0f),
        };

        // Act
        var (min, max) = vertices.GetMinMax();

        // Assert
        Assert.Equal(-1.0f, min.X);
        Assert.Equal(-2.0f, min.Y);
        Assert.Equal(6.0f, min.Z);
        Assert.Equal(4.0f, max.X);
        Assert.Equal(5.0f, max.Y);
        Assert.Equal(-3.0f, max.Z);
    }

    [Fact]
    public void GetLongs_ReturnsCorrectValues()
    {
        // Arrange
        var testString = string.Join(Environment.NewLine, Enumerable.Range(1, 09)) + "- dog";

        // Act
        var result = StringHelper.GetLongs(testString);

        // Assert
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
        Assert.Equal(4, result[3]);
        Assert.Equal(5, result[4]);
        Assert.Equal(6, result[5]);
        Assert.Equal(7, result[6]);
        Assert.Equal(8, result[7]);
        Assert.Equal(9, result[8]);
    }
}