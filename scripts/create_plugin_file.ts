import * as path from "https://deno.land/std@0.146.0/path/mod.ts";
import { processPlugin } from "https://raw.githubusercontent.com/dprint/automation/0.7.0/mod.ts";

const currentDirPath = path.dirname(path.fromFileUrl(import.meta.url));
const projectFile = path.join(currentDirPath, "../DprintPluginCsharpier/DprintPluginCsharpier.csproj");

const packageText = await Deno.readTextFile(projectFile);
const version = packageText.match(/\<Version\>(\d+\.\d+\.\d+)<\/Version\>/)?.[1];

if (version == null || !/^\d+\.\d+\.\d+$/.test(version)) {
  throw new Error("Error extracting version.");
}

async function createProcessPlugin({ pluginName, version, platforms, isTest }: {
  pluginName: string;
  version: string;
  platforms: processPlugin.Platform[];
  /** Creates a plugin file with only the current platform using
   * a zip file in the current folder.
   */
  isTest: boolean;
}) {
  const builder = new processPlugin.PluginFileBuilder({
    name: pluginName,
    version: version,
  });

  if (isTest) {
    const platform = processPlugin.getCurrentPlatform();
    const zipFileName = processPlugin.getStandardZipFileName(builder.pluginName, platform);
    await builder.addPlatform({
      platform,
      zipFilePath: zipFileName,
      zipUrl: zipFileName,
    });
  } else {
    for (const platform of platforms) {
      await addPlatform(platform);
    }
  }

  await builder.writeToPath("plugin.json");

  async function addPlatform(platform: processPlugin.Platform) {
    const zipFileName = processPlugin.getStandardZipFileName(builder.pluginName, platform);
    const zipUrl = `https://github.com/Phault/${pluginName}/releases/download/${builder.version}/${zipFileName}`;
    await builder.addPlatform({
      platform,
      zipFilePath: zipFileName,
      zipUrl,
    });
  }
}

await createProcessPlugin({
  pluginName: "dprint-plugin-csharpier",
  version,
  platforms: [
    "darwin-x86_64",
    "darwin-aarch64",
    "linux-x86_64",
    "linux-x86_64-musl",
    "linux-aarch64",
    "linux-aarch64-musl",
    "windows-x86_64",
  ],
  isTest: Deno.args.some(a => a == "--test"),
});
