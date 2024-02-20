# dprint-plugin-csharpier

[![CI](https://github.com/Phault/dprint-plugin-csharpier/workflows/CI/badge.svg)](https://github.com/Phault/dprint-plugin-csharpier/actions?query=workflow%3ACI)

Wrapper around [Csharpier](https://csharpier.com) in order to use it as a dprint plugin for C# formatting.

## Install

1. Install [dprint](https://dprint.dev/install/)
2. Follow instructions at https://github.com/Phault/dprint-plugin-csharpier/releases/

## Configuration

Specify a "csharpier" configuration property in _dprint.json_:

```jsonc
{
  // etc...
  "csharpier": {
    "printWidth": 100
  }
}
```

Refer to the [official Csharpier docs](https://csharpier.com/docs/Configuration) for available options.

## Credits

This project was forked from [dprint-plugin-roslyn](https://github.com/dprint/dprint-plugin-roslyn).
