# Index page of the documentation

> [DocFX](https://dotnet.github.io/docfx/index.html) using DocFX


| Embedding images is possible but unclear where and how |
|:----------------------------------:|
| [![DocFxForUnity documentation manual](https://normanderwan.github.io/DocFxForUnity/resources/ExampleManual.png)](https://normanderwan.github.io/DocFxForUnity/manual/coniunctis.html) |


## Setup your documentation

Nice effect with a block and color coding

    ```diff
      .
      ├── Assets
    + ├── Documentation
      ├── Package
      ├── ProjectSettings
      └── README.md
    ```

3. Something in a textblock inline `Documentation/docfx.json`

    ```javascript
      {
        "build": {
          "globalMetadata": // Edit your documentation website info, see: https://dotnet.github.io/docfx/tutorial/docfx.exe_user_manual.html#322-reserved-metadata
          {
            "_appTitle": "Example Unity documentation",
            "_appFooter": "Example Unity documentation",
            "_enableSearch": true
          },
          "sitemap":
          {
            "baseUrl": "https://normanderwan.github.io/DocFxForUnity" // The URL of your documentation website
          }
      }
    ```
    ```yaml
    ---
    uid: Your.Namespace1
    summary: Description of the Your.Namespace1 namespace.
    ---
    ```


    ```bash
    cp README.md Documentation/index.md
    docfx Documentation/docfx.json --serve
    ```

