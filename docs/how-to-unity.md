### How to add SDK to Unity project.

1) Since SDK targets .NET Standard 2.1 Unity 2021.2 or newer is required.

2) Current Unity distribution may contain Newtonsoft.Json assembly and some others,
don't import them to Unity project or delete (if Unity editor shows conflicting assemblies)

3) Run for ChainAbstractions or StacksApi project to build and collect all necessary assemblies
    ```
    dotnet publish -c Release
    ```
4) Import *.dll contents of project's subfolder to Unity assets:
    ```
    bin\Release\netstandard2.1\publish\
    ```
5) Use bootstrap scripts from "Unity" folder

6) You might need to turn of 'Assembly Version Validation' in Unity Project Settings (Player section)

7) For WebGL builds you need to use Unity web requests and single threaded mode (see ForceSDK.cs)