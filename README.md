# Z it!

`zit` eases the access to compression/decompression for various formats.

In short, `zit` provides a single executable that integrates functionalities of tar/unzip/zip/zstd for all desktop platforms.

You may avoid remembering `-xzf` or stuffs like that, just use `zit` to handle all the things.

```shell
# compression
> zit -c /path/to/dir -o out.zip
> zit -c /path/to/dir -o out.tar.gz
> zit -c /path/to/dir -o out.tar.zst
> zit -c /path/to/dir -o out.zip.zip.zip.zst # works :)

# decompression
> zit -d out.zip -o /path/to/dir2
> zit -d out.tar.gz -o /path/to/dir3
> zit -d out.tar.zst -o /path/to/dir4
> zit -d out.zip.zip.zip.zst -o /path/to/dir5
# now, we have dir2 = dir3 = dir3 = dir5 = /path/to/dir
```

## Usage

1. Compression (supports `.zip`, `.tar`, `.gz` and `.zst`)

  ```shell
  # chaining
  zit -c -o output.zip.zst /path/to/file_or_dir
  # this is fine: zit -c /path/to/file_or_dir -o output.zip.zst
  zit -c -o output.tar.zst /path/to/file_or_dir
  zit -c -o output.tar.gz /path/to/file_or_dir

  # single format
  zit -c -o output.zip /path/to/file_or_dir
  zit -c -o output.tar /path/to/file_or_dir
  zit -c -o output.gz /path/to/file
  zit -c -o output.zst /path/to/file
  ```

2. Decompression (supports `.zip`, `.tar`, `.gz` and `.zst`)

  ```shell
  # chaning
  zit -d output_dir.tar.gz -o /path/to/output_dir
  # this is fine: zit -d output_dir.tar.gz -o /path/to/output_dir
  zit -d output_dir.tar.zst -o /path/to/output_dir

  # single
  zit -d output_dir.zip -o /path/to/output_dir
  zit -d output_dir.tar -o /path/to/output_dir
  zit -d output_dir.gz -o /path/to/output_file
  zit -d output_dir.zst -o /path/to/output_file
  ```

Besides, this is the CLI help:

```bash
> zit -h
Usage: zit [options] path (version 0.1.2)
Options:
  -h,--help     Print this help message.
  -o <path>     Output path.
  -d            Use decompression.
  -c            Use compression (default).
  -v,--version  Print the version.
```

## Scenarios

`zit` is designed for simplicity and portability, but not for performance and extreme scalability.

`zit` is useful in the following scenarios:

1. build system (`zit` is designed for the [NoMake](https://github.com/thautwarm/nomake) build system)
2. CI/CD system

## Source Build

You need the following componets to proceed the cross compilation.

1. [.NET 7 or above](https://dotnet.microsoft.com/en-us/download)
2. [Deno](https://deno.com/)

Then you run the following commands:

```shell
> deno run -A build.ts dist

> tree -ha dist
[4.0K]  dist
├── [4.0K]  linux-arm64
│   ├── [ 12M]  zit
│   └── [ 14K]  zit.pdb
├── [4.0K]  linux-x64
│   ├── [ 13M]  zit
│   └── [ 14K]  zit.pdb
├── [4.0K]  osx-arm64
│   ├── [ 12M]  zit
│   └── [ 14K]  zit.pdb
├── [4.0K]  osx-x64
│   ├── [ 13M]  zit
│   └── [ 14K]  zit.pdb
├── [4.0K]  win-x64
│   ├── [ 12M]  zit.exe
│   └── [ 14K]  zit.pdb
├── [5.0M]  zit-linux-arm64.zip
├── [5.4M]  zit-linux-x64.zip
├── [4.9M]  zit-osx-arm64.zip
├── [5.3M]  zit-osx-x64.zip
└── [5.2M]  zit-win-x64.zip

5 directories, 15 files
```

# Contributions

`zit` now supports the following formats:

- [x] `.zip`
- [x] `.tar`
- [x] `.gz`
- [x] `.zst`

It's welcome to contribute to `zit` by adding more formats or improving the existing ones (e.g., scalability).

`zit` is well-orgainzed and separated from concerns. You might modify `zit/Ops.cs` for adding new compression/decompression backends.

Remember to add test for your changes. See `test/Test.tar.gz.cs`, `test/Test.zip`, `test/Test.tar.zst` for examples.

# License

`zit` is licnesed under the MIT license. You are free to use it in any way you like.
