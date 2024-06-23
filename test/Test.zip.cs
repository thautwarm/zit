namespace Zit.Test;

public static class TestZip
{

    [Assert.Case]
    public static void Test()
    {
        var root = Path.GetDirectoryName(Assert.CallerFilePath())!;
        var data = Files.Load(Path.Combine(root, "test_tar"));
        Environment.CurrentDirectory = root;
        Zit.Program.MainResuable([
            "-c",
            "test_tar",
            "-o",
            "test.zip",
        ]);
        Zit.Program.MainResuable([
            "-d",
            "test.zip",
            "-o",
            "test_zip_out",
        ]);
        var data2 = Files.Load(Path.Combine(root, "test_zip_out"));

        Assert.Equal(data.Count, data2.Count);
        foreach (var (k, v) in data)
        {
            Assert.Cond(data2.ContainsKey(k));
            Assert.Equal(v, data2[k]);
        }

        Directory.Delete(Path.Combine(root, "test_zip_out"), true);
        File.Delete(Path.Combine(root, "test.zip"));
    }

    [Assert.Case]
    public static void TestSingleFile()
    {
        var root = Path.GetDirectoryName(Assert.CallerFilePath())!;
        var fileExample = Path.Combine(root, "_example.txt");
        File.WriteAllText(fileExample, "example");

        Environment.CurrentDirectory = root;
        Zit.Program.MainResuable([
            "-c",
            "_example.txt",
            "-o",
            "_example.zip",
        ]);

        Zit.Program.MainResuable([
            "-d",
            "_example.zip",
            "-o",
            "_example_out",
        ]);

        Assert.Equal("example", File.ReadAllText(Path.Combine(root, "_example_out", "_example.txt")));

        Directory.Delete(Path.Combine(root, "_example_out"), true);
        File.Delete(Path.Combine(root, "_example.zip"));
        File.Delete(fileExample);
    }
}