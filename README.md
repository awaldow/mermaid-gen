# mermaid-gen
## Installation
1. Install as a [global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool)
    ```
    dotnet tool install -g mermaid-gen
    ```
2. Verify that the tool was installed correctly
    ```
    mermaid-gen --help
    ```
3. Generate a mermaid ER diagram from a provided assembly
    ```
    mermaid-gen generate -a [assemblyPath] -o [outputDir] -t er
    ```
    Where 
    * [assemblyPath] = path to target assembly
    * [outputDir] = output path (including file name) for mermaid markdown

## Support
Currently this utility is targeted at EF Core libraries using *ONLY* conventions and *NOT* fluent configuration. There are switches for class diagrams and fluent EF Core entity configurations but they do not do anything yet.