using Zit.Ext;

namespace Zit.Test;

public static class Files
{
    public static string[] files = [
        "dir/a.txt",
        "dir/bb/c.txt",
        "file.json",
    ];

    public static Dictionary<string, string> Load(string root)
    {
        return files.Select(x => Path.Combine(root, x).Pipe(x => File.ReadAllText(x))).ToDictionary(x => x, x => x);
    }
}