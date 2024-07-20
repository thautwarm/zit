# ChangeLog

## v0.1.3

- Distrbution filenames now include the version, e.g., `zit-linux-x64-0.1.2`, `zit-win-x64-0.1.2.exe`.
- Linux binaries are now built with [bflat](https://github.com/bflattened/bflat) to be compatible to GLIBC 2.17 systems.
- bump `thautwarm/nomake` to `v0.1.10`

## v0.1.2

- Formats: `.zip`, `.tar`, `.gz` and `.zst`.
- Chaining compression/decompression based on path suffixes: `zit -c -o output.zip.zst /path/to/file_or_dir`.
- Platforms: `Linux` (arm64 & x64), `macOS` (arm64 & x64) and `Windows` (x64).
