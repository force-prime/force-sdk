### How to add SDK to Unity project.

1) Since SDK targets .NET Standard 2.1 Unity 2021.2 or newer is required.

2) Current Unity distribution already contains Newtonsoft.Json assembly,
that's why it excluded from SDK libraries explicitly

3) Run for ChainAbstractions or StacksApi project
    ```
    dotnet publish -c Release
    ```
4) Import *.dll contents of project's subfolder to Unity assets:
    ```
    bin\Release\netstandard2.1\publish\
    ```

5) You might need to turn of 'Assembly Version Validation' in Unity Project Settings (Player section)