using Muxarr.Core.Utilities;

namespace Muxarr.Tests;

[TestClass]
public class HardLinkHelperTests
{
    private string _tempDir = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"muxarr_hltest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [TestMethod]
    public void TryCreateHardLink_CreatesLink()
    {
        var source = Path.Combine(_tempDir, "source.txt");
        var link = Path.Combine(_tempDir, "link.txt");
        File.WriteAllText(source, "test");

        Assert.IsTrue(HardLinkHelper.TryCreateHardLink(source, link));
        Assert.IsTrue(File.Exists(link));
        Assert.AreEqual("test", File.ReadAllText(link));
    }

    [TestMethod]
    public void TryCreateHardLink_NonExistentSource_ReturnsFalse()
    {
        var link = Path.Combine(_tempDir, "link.txt");

        Assert.IsFalse(HardLinkHelper.TryCreateHardLink(Path.Combine(_tempDir, "nope.txt"), link));
    }

    [TestMethod]
    public void IsHardlinked_RegularFile_ReturnsFalse()
    {
        var file = Path.Combine(_tempDir, "regular.txt");
        File.WriteAllText(file, "test");

        Assert.IsFalse(HardLinkHelper.IsHardlinked(file));
    }

    [TestMethod]
    public void IsHardlinked_HardlinkedFile_ReturnsTrue()
    {
        var original = Path.Combine(_tempDir, "original.txt");
        var link = Path.Combine(_tempDir, "link.txt");
        File.WriteAllText(original, "test");
        HardLinkHelper.TryCreateHardLink(original, link);

        Assert.IsTrue(HardLinkHelper.IsHardlinked(original));
        Assert.IsTrue(HardLinkHelper.IsHardlinked(link));
    }

    [TestMethod]
    public void IsHardlinked_AfterLinkRemoved_ReturnsFalse()
    {
        var original = Path.Combine(_tempDir, "original.txt");
        var link = Path.Combine(_tempDir, "link.txt");
        File.WriteAllText(original, "test");
        HardLinkHelper.TryCreateHardLink(original, link);

        Assert.IsTrue(HardLinkHelper.IsHardlinked(original));

        File.Delete(link);

        Assert.IsFalse(HardLinkHelper.IsHardlinked(original));
    }

    [TestMethod]
    public void IsHardlinked_NonExistentFile_ReturnsFalse()
    {
        Assert.IsFalse(HardLinkHelper.IsHardlinked(Path.Combine(_tempDir, "nope.txt")));
    }

    [TestMethod]
    public void GetLinkCount_RegularFile_Returns1()
    {
        var file = Path.Combine(_tempDir, "regular.txt");
        File.WriteAllText(file, "test");

        Assert.AreEqual(1u, HardLinkHelper.GetLinkCount(file));
    }

    [TestMethod]
    public void GetLinkCount_TwoLinks_Returns2()
    {
        var original = Path.Combine(_tempDir, "original.txt");
        var link = Path.Combine(_tempDir, "link.txt");
        File.WriteAllText(original, "test");
        HardLinkHelper.TryCreateHardLink(original, link);

        Assert.AreEqual(2u, HardLinkHelper.GetLinkCount(original));
        Assert.AreEqual(2u, HardLinkHelper.GetLinkCount(link));
    }

    [TestMethod]
    public void GetLinkCount_ThreeLinks_Returns3()
    {
        var original = Path.Combine(_tempDir, "original.txt");
        var link1 = Path.Combine(_tempDir, "link1.txt");
        var link2 = Path.Combine(_tempDir, "link2.txt");
        File.WriteAllText(original, "test");
        HardLinkHelper.TryCreateHardLink(original, link1);
        HardLinkHelper.TryCreateHardLink(original, link2);

        Assert.AreEqual(3u, HardLinkHelper.GetLinkCount(original));
    }
}
