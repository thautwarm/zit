using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Zit;

public static class Program
{
    enum ZipMode
    {
        Compress,
        Decompress,
    }

    enum ZipFormat
    {
        zst,
        gz,
        zip,
        tar,
    }
    class Options
    {
        public ZipMode Mode = ZipMode.Compress;
        public string Output = "";
        public string? Input;
    }

    public sealed class ZipIOPair
    {
        public string Input;
        public string Output;
        public ZipIOPair(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    public static void Main(string[] args)
    {
        MainResuable(args);
    }

    public static void MainResuable(string[] args)
    {
        SystemX.Cmd.CmdParser<Options> parser = new();
        string helpDoc = "";
        helpDoc += "Usage: zit [options] path (version " + GeneratedVersion.Value + ")\n";
        helpDoc += "Options:\n";
        helpDoc += "  -h,--help     Print this help message.\n";
        helpDoc += "  -o <path>     Output path.\n";
        helpDoc += "  -d            Use decompression.\n";
        helpDoc += "  -c            Use compression (default).\n";
        helpDoc += "  -v,--version  Print the version.\n";
        parser.Help(helpDoc);

        parser.ShortcutFlag("v", "version", (opts) =>
        {
            Console.WriteLine(GeneratedVersion.Value);
            Environment.Exit(0);
        });

        parser.ShortcutFlag("h", "help", (opts) =>
        {
            parser.PrintHelp();
            Environment.Exit(0);
        });

        parser.ShortcutFlag("d", (opts) =>
        {
            opts.Mode = ZipMode.Decompress;
        });
        parser.ShortcutFlag("c", (opts) =>
        {
            opts.Mode = ZipMode.Compress;
        });
        parser.ShortcutOption("o", "Output", (opts, value) =>
        {
            opts.Output = value;
        });
        parser.Positional("Input", (opts, value) =>
        {
            if (opts.Input is not null)
            {
                Console.WriteLine("Warning : zit accepting multiple input files.");
            }
            opts.Input = value;
        });
        var opts = parser.Parse(args);
        var (formats, ioPair) = ParseFormats(opts);
        if (formats.Count == 0)
        {
            switch (opts.Mode)
            {
                case ZipMode.Compress:
                    Console.WriteLine($"Error: No file formats found from the output path: {opts.Output}");
                    break;
                case ZipMode.Decompress:
                    Console.WriteLine($"Error: No file formats found from the input path: {opts.Input}");
                    break;
                default:
                    throw new UnreachableException();
            }
            Environment.Exit(1);
        }
        switch (opts.Mode)
        {
            case ZipMode.Compress:
                {
                    PerformCompression(formats, ioPair);
                    break;
                }
            case ZipMode.Decompress:
                {
                    PerformDecompression(formats, ioPair);
                    break;
                }
        }
    }

    /// <summary>
    /// Parse the file formats from the input and output paths.
    /// The file formats are determined by the file extensions.
    /// The string used to determine the file formats is modified to remove the file extensions.
    /// </summary>
    static (List<ZipFormat>, ZipIOPair) ParseFormats(Options opts)
    {
        if (opts.Input is null)
        {
            Console.WriteLine("Error: No input file specified.");
            Environment.Exit(1);
        }


        string parseSource;

        switch (opts.Mode)
        {
            case ZipMode.Compress:
                {
                    parseSource = opts.Output;
                    break;
                }
            case ZipMode.Decompress:
                {
                    parseSource = opts.Input;
                    break;
                }
            default:
                {
                    throw new NotImplementedException();
                }
        }

        var basename = Path.GetFileName(parseSource);
        var parts = new Stack<string>(basename.Split("."));
        List<ZipFormat> fileFormats = new();

        while (parts.TryPop(out var part))
        {
            switch (part.ToLowerInvariant())
            {
                case "zst":
                    {
                        fileFormats.Add(ZipFormat.zst);
                        break;
                    }
                case "gz":
                    {
                        fileFormats.Add(ZipFormat.gz);
                        break;
                    }
                case "zip":
                    {
                        fileFormats.Add(ZipFormat.zip);
                        break;
                    }
                case "tar":
                    {
                        fileFormats.Add(ZipFormat.tar);
                        break;
                    }
                default:
                    {
                        parts.Push(part);
                        goto formatsFound;
                    }
            }
        }



    formatsFound:
        fileFormats.Reverse();
        return (
            fileFormats,
            new ZipIOPair(Path.GetFullPath(opts.Input!), Path.GetFullPath(opts.Output))
        );
    }


    private static void PerformCompression(List<ZipFormat> formats, ZipIOPair ioPair)
    {
        using var autoClean = new AutoClean();

        for (int i = 0; i < formats.Count; i++)
        {
            var format = formats[i];
            var isLast = i == formats.Count - 1;
            string allocateOutput() => AllocateOutputPath(true, isLast, ioPair.Output, autoClean);
            switch (format)
            {
                case ZipFormat.zip:
                    {
                        string output = allocateOutput();
                        Ops.CompressionOps.Zip(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
                case ZipFormat.tar:
                    {
                        string output = allocateOutput();
                        Ops.CompressionOps.Tar(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
                case ZipFormat.gz:
                    {
                        string output = allocateOutput();
                        Ops.CompressionOps.Gz(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
                case ZipFormat.zst:
                    {
                        string output = allocateOutput();
                        Ops.CompressionOps.Zst(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
            }
        }
    }

    private static void PerformDecompression(List<ZipFormat> formats, ZipIOPair ioPair)
    {
        formats.Reverse();
        using var autoClean = new AutoClean();

        for (int i = 0; i < formats.Count; i++)
        {
            if (ioPair.Input == "") throw new Exception("No input file found.");

            var format = formats[i];
            var isLast = i == formats.Count - 1;
            string allocateOutput(bool file) => AllocateOutputPath(file, isLast, ioPair.Output, autoClean);

            switch (format)
            {
                case ZipFormat.zip:
                    {
                        string output = allocateOutput(file: false);
                        Ops.DecompressionOps.Zip(ioPair.Input, output);
                        if (!isLast)
                        {
                            ioPair.Input = FindFileInDirectory(output) ?? "";
                        }
                        break;
                    }
                case ZipFormat.tar:
                    {
                        string output = allocateOutput(file: false);
                        Ops.DecompressionOps.Tar(ioPair.Input, output);
                        if (!isLast)
                        {
                            ioPair.Input = FindFileInDirectory(output) ?? "";
                        }
                        break;
                    }
                case ZipFormat.gz:
                    {
                        string output = allocateOutput(file: true);
                        Ops.DecompressionOps.Gz(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
                case ZipFormat.zst:
                    {
                        string output = allocateOutput(file: true);
                        Ops.DecompressionOps.Zst(ioPair.Input, output);
                        ioPair.Input = output;
                        break;
                    }
            }
        }
    }

    static string AllocateOutputPath(bool file, bool isLast, string finalOutput, AutoClean autoClean)
    {
        string output;
        if (isLast)
        {
            output = finalOutput;
            var outputDir = Path.GetDirectoryName(output);
            if (outputDir is not null && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            if (!file)
            {
                Directory.CreateDirectory(output);
            }
            else
            {
                if (File.Exists(output)) File.Delete(output);
            }
        }
        else
        {
            var directory = Path.GetDirectoryName(finalOutput) ?? Environment.CurrentDirectory;
            output = file ?
                AllocateTempFile(directory)
                : AllocateTempDirectory(directory);

            autoClean.Add(output);
        }
        return output;
    }

    private static string AllocateTempDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        // for windows 7 compatibility, we do not use System.IO.Path.GetRandomFileName()
        var genDir = Path.Combine(directory, Guid.NewGuid().ToString());
        while (Directory.Exists(genDir))
        {
            genDir = Path.Combine(directory, Guid.NewGuid().ToString());
        }
        Directory.CreateDirectory(genDir);
        return genDir;
    }

    private static string AllocateTempFile(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        // for windows 7 compatibility, we do not use System.IO.Path.GetRandomFileName()
        var genDir = Path.Combine(directory, Guid.NewGuid().ToString());
        while (File.Exists(genDir))
        {
            genDir = Path.Combine(directory, Guid.NewGuid().ToString());
        }
        return genDir;
    }


    static string? FindFileInDirectory(string directory)
    {
        if (!Directory.Exists(directory)) return null;
        var files = Directory.GetFiles(directory);
        if (files.Length == 0) return null;
        if (files.Length > 1)
            Console.WriteLine("Warning: Multiple files found in the directory. Using the first file.");
        return files[0];
    }
}