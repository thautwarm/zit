namespace Zit.Test;

public static class TestTarGz
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
            "test.tar.gz",
        ]);
        Zit.Program.MainResuable([
            "-d",
            "test.tar.gz",
            "-o",
            "test_tar_gz_out",
        ]);
        var data2 = Files.Load(Path.Combine(root, "test_tar_gz_out"));

        Assert.Equal(data.Count, data2.Count);
        foreach (var (k, v) in data)
        {
            Assert.Cond(data2.ContainsKey(k));
            Assert.Equal(v, data2[k]);
        }

        Directory.Delete(Path.Combine(root, "test_tar_gz_out"), true);
        File.Delete(Path.Combine(root, "test.tar.gz"));
    }
}