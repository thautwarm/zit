namespace Zit.Test;

public static class TestTarZst
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
            "test.tar.zst",
        ]);
        Zit.Program.MainResuable([
            "-d",
            "test.tar.zst",
            "-o",
            "test_tar_zst_out",
        ]);
        var data2 = Files.Load(Path.Combine(root, "test_tar_zst_out"));

        Assert.Equal(data.Count, data2.Count);
        foreach (var (k, v) in data)
        {
            Assert.Cond(data2.ContainsKey(k));
            Assert.Equal(v, data2[k]);
        }

        Directory.Delete(Path.Combine(root, "test_tar_zst_out"), true);
        File.Delete(Path.Combine(root, "test.tar.zst"));
    }
}