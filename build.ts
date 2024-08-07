import { BuildConfig, NM } from "./build.config.ts";

async function compileProject(
  identifier: (typeof BuildConfig.supportedIdentifiers)[number],
  sources: string[],
  target: string,
): Promise<"bflat" | "dotnet"> {
  if (identifier === "linux-x64" || identifier === "linux-arm64") {
    const build = new NM.Bflat.Build();
    build.os = "linux";
    build.mode = "exe";
    build.optimize = { speed: true };
    build.referencedILAssemblies = ["deps/ZstdSharp.dll"];
    build.separateSymbols = true;
    switch (identifier) {
      case "linux-arm64":
        build.arch = "arm64";
        break;
      case "linux-x64":
        build.arch = "x64";
        break;
    }
    build.sourceFiles = Array.from(new Set(sources));
    await build.run(target);
    return "bflat";
  }
  await NM.Shell.runChecked(
    NM.Shell.split(
      `dotnet publish -r ${identifier} -c Release zit/zit.csproj -o dist/${identifier}`,
    ),
    {
      logError: true,
      printCmd: true,
    },
  );
  return "dotnet";
}

class AsyncLock {
  private _locked = false;
  async lock() {
    while (this._locked) {
      await new Promise((resolve) => setTimeout(resolve, 100));
    }
    this._locked = true;
  }
  unlock() {
    this._locked = false;
  }
}

const versionTemplate = (x: string) =>
  `namespace Zit;

public static class GeneratedVersion
{
    public const string Value = "${x}";
}
`;

NM.target(
  {
    name: "zit/GeneratedVersion.cs",
    deps: ["build.config.ts"],
    async build({ target }) {
      await new NM.Path(target).writeText(versionTemplate(BuildConfig.version));
    },
  },
);

// 'dotnet' cmd does not support being invoked concurrently.
const dotnetLock = new AsyncLock();

const currentIdentifier = (() => {
  const { os, arch } = NM.Platform.current;
  if (os == "windows" && arch == "x64") return "win-x64";
  if (os == "linux" && arch == "x64") return "linux-x64";
  if (os == "linux" && arch == "arm64") return "linux-arm64";
  if (os == "macos" && arch == "x64") return "osx-x64";
  if (os == "macos" && arch == "arm64") return "osx-arm64";
  throw new Error(`Unsupported platform: ${JSON.stringify({ os, arch })}`);
})();

const hostZitBuiltPath = fixExe(
  `dist/${currentIdentifier}/zit`,
  currentIdentifier,
);

for (const identifier of BuildConfig.supportedIdentifiers) {
  NM.target(
    {
      name: distFile(identifier),
      deps: {
        regular: NM.Path.glob("zit/**/*.cs", {
          exclude: ["**/bin/**/*", "**/obj/**/*"],
        }),
        config: NM.Path.glob("zit/**/*.csproj"),
        version: "zit/GeneratedVersion.cs",
      },
      async build({ target, deps }) {
        await new NM.Path("dist").join(identifier).mkdir({
          parents: true,
          onError: "ignore",
        });
        await dotnetLock.lock();
        let buildSystem: "bflat" | "dotnet";
        try {
          buildSystem = await compileProject(
            identifier,
            [...deps.regular, deps.version],
            target,
          );
        } finally {
          dotnetLock.unlock();
        }

        if (buildSystem == "dotnet") {
          const exePath = await findExecutable(identifier);
          exePath.copyTo(target);
        }

        NM.Log.ok(`Built ${identifier}`);
      },
    },
  );
}

NM.target(
  {
    name: "dist",
    virtual: true,
    doc: `Build all the binaries`,
    deps: BuildConfig.supportedIdentifiers.map(distFile),
    build() {
      NM.Log.ok("All binaries are built!");
    },
  },
);

await NM.makefile();

function fixExe(p: string, identifier: string) {
  if (identifier.startsWith("win")) {
    return p + ".exe";
  }
  return p;
}

function distFile(identifier: string) {
  return fixExe(`dist/zit-${identifier}-${BuildConfig.version}`, identifier);
}

async function findExecutable(identifier: string) {
  const p = new NM.Path(fixExe(`dist/${identifier}/zit`, identifier));
  if (!await p.exists()) {
    NM.fail(`Executable not found: ${p.asOsPath()}`);
  }
  return p;
}
