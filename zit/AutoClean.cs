using System;
using System.IO;
using System.Collections.Generic;

namespace Zit;

public sealed partial class AutoClean : IDisposable
{
    public readonly List<string> Paths = new();

    public AutoClean(params string[] paths)
    {
        Paths.AddRange(paths);
    }

    public void Add(string path)
    {
        Paths.Add(path);
    }

    public void Dispose()
    {
        foreach (var path in Paths)
        {
            try
            {

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            // exceptions here are tolerated
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}