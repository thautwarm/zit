import * as NM from "thautwarm/nomake";
export { NM };

export const BuildConfig = {
  version: "0.1.3",
  supportedIdentifiers: [
    "linux-x64",
    "linux-arm64",
    "win-x64",
    "osx-x64",
    "osx-arm64",
  ] as const,
};
