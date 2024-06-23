using System.IO;
using System.IO.Compression;
using Zit.Ext;

namespace Zit.Ops;
internal sealed class CompressionOps
{
    internal static void Zip(string inputPath, string outputFile)
    {
        using var assuredDir = new AssureDirectory(inputPath);
        ZipFile.CreateFromDirectory(assuredDir.directory, outputFile, CompressionLevel.Optimal, includeBaseDirectory: false);
    }

    internal static void Tar(string inputPath, string outputFile)
    {
        using var assuredDir = new AssureDirectory(inputPath);
        System.Formats.Tar.TarFile.CreateFromDirectory(assuredDir.directory, outputFile, includeBaseDirectory: false);
    }

    internal static void Gz(string inputFile, string outputFile)
    {
        using var input = File.OpenRead(inputFile);
        using var output = File.Create(outputFile);
        using var gz = new GZipStream(output, CompressionMode.Compress);
        input.CopyTo(gz);
    }

    internal static void Zst(string inputFile, string outputFile)
    {
        using var input = File.OpenRead(inputFile);
        using var output = File.Create(outputFile);
        using var zst =
            new ZstdSharp.CompressionStream(output, level: 1);
        input.CopyTo(zst);
    }
}

internal sealed class DecompressionOps
{
    internal static void Zip(string inputPath, string outputDir)
    {
        ZipFile.ExtractToDirectory(inputPath, outputDir, overwriteFiles: true);
    }

    internal static void Tar(string inputPath, string outputDir)
    {
        System.Formats.Tar.TarFile.ExtractToDirectory(inputPath, outputDir, overwriteFiles: true);
    }

    internal static void Gz(string inputPath, string outputFile)
    {
        using var input = File.OpenRead(inputPath);
        using var output = File.Create(outputFile);
        using var gz = new GZipStream(input, CompressionMode.Decompress);
        gz.CopyTo(output);
    }

    internal static void Zst(string inputPath, string outputFile)
    {
        using var input = File.OpenRead(inputPath);
        using var output = File.Create(outputFile);
        using var zst = new ZstdSharp.DecompressionStream(input);
        zst.CopyTo(output);
    }
}

partial struct AssureDirectory : System.IDisposable
{
    private string? dirToDelete;
    public readonly string directory;
    public AssureDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            directory = path;
            return;
        }
        var genDir = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName());
        while (Directory.Exists(genDir))
        {
            genDir = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName());
        }
        Directory.CreateDirectory(genDir);
        if (File.Exists(path))
        {
            File.Copy(path, Path.Combine(genDir, Path.GetFileName(path)));
        }
        directory = genDir;
        dirToDelete = genDir;
    }

    public void Dispose()
    {
        if (dirToDelete is null) return;
        try
        {
            Directory.Delete(dirToDelete, true);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e.ToString());
        }
    }
}