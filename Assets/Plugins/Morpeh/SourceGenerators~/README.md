# Morpeh component registration

The incremental generator emits one preserved registrar per component assembly, with ordinal-sorted generic registration calls and compile-time cache sizes. Private/protected components produce the MORPEH001 compilation error; components and their containers must be internal or public. Generic components and components inside generic containers produce the MORPEH002 compilation error; move them to non-generic types. Automatic registration runs at SubsystemRegistration.

From the repository root:

```sh
~/.dotnet/dotnet test Assets/Plugins/Morpeh/SourceGenerators~/Tests/Morpeh.SourceGenerators.Tests.csproj
~/.dotnet/dotnet build Assets/Plugins/Morpeh/SourceGenerators~/Morpeh.SourceGenerators.csproj -c Release
```

Roslyn is pinned to 4.3.1 for Unity's compiler. Release builds copy the DLL to `../Analyzers`. After editing generator sources, rebuild and commit the DLL alongside the sources, retaining its RoslynAnalyzer `.meta`, then reimport it in Unity. Unity scopes this analyzer to Morpeh and assemblies referencing Morpeh.
