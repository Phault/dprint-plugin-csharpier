import { generateChangeLog } from "https://raw.githubusercontent.com/dprint/automation/0.9.0/changelog.ts";

const version = Deno.args[0];
const checksum = Deno.args[1];
const changelog = await generateChangeLog({
  versionTo: version,
});
const text = `## Changes

${changelog}

## Install

In a dprint configuration file:

1. Specify the plugin url and checksum in the \`"plugins"\` array or run \`dprint config add csharpier\`.
   \`\`\`jsonc
   {
     // etc...
     "plugins": [
       "https://plugins.dprint.dev/Phault/csharpier-${version}.json@${checksum}"
     ]
   }
   \`\`\`
2. Add a "csharpier" configuration property if desired.
   \`\`\`jsonc
   {
     // ...etc...
     "csharpier": {
       "printWidth": 100
     }
   }
   \`\`\`
`;

console.log(text);
