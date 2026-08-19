# Project Scaffolding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a buildable .NET 10 solution with the six approved projects, enforced dependency directions, a working test host, executable examples and benchmark hosts, and documented developer commands.

**Architecture:** Three runtime class libraries form the reusable product surface: Core has no project dependencies, while Algorithms and Experiments each depend only on Core. Examples, Tests, and Benchmarks are development-time consumers that validate the dependency graph without introducing algorithm behavior during this scaffolding increment.

**Tech Stack:** .NET SDK 10.0.400, C# targeting `net10.0`, xUnit.net v3 3.2.2 with Microsoft Testing Platform, BenchmarkDotNet 0.15.8, MSBuild central package management, `.slnx` solution format.

## Global Constraints

- Target `.NET 10` and `net10.0`; use the SDK's default stable C# language version.
- Keep runtime projects cross-platform and free of Windows-specific APIs.
- Enable nullable reference types, implicit usings, deterministic builds, recommended analyzers, and warnings as errors for every project.
- Use root namespaces and assembly names under `Metaheuristics.*`.
- Keep dependencies exactly: `Algorithms → Core`, `Experiments → Core`, `Examples → Core + Algorithms + Experiments`, `Tests → Core + Algorithms + Experiments`, and `Benchmarks → Core + Algorithms`.
- Do not add PSO, constraints, optimizer contracts, experiment execution, or other production behavior in this increment.
- Configuration, generated solution metadata, and executable host files are handled as the explicitly approved TDD exception for scaffolding; all later production behavior follows red-green-refactor.

---

### Task 1: Establish the repository build topology

**Files:**
- Create: `.editorconfig`
- Create: `.gitignore`
- Create: `global.json`
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `Metaheuristics.NET.slnx`
- Create: `src/Metaheuristics.Core/Metaheuristics.Core.csproj`
- Create: `src/Metaheuristics.Algorithms/Metaheuristics.Algorithms.csproj`
- Create: `src/Metaheuristics.Experiments/Metaheuristics.Experiments.csproj`

**Interfaces:**
- Consumes: approved dependency boundaries from `docs/superpowers/specs/2026-08-19-project-foundation-design.md`.
- Produces: a solution containing three buildable runtime projects where Core is independent and Algorithms and Experiments reference only Core.

- [ ] **Step 1: Verify the clean baseline and SDK**

Run:

```powershell
git status --short
dotnet --version
```

Expected: `git status --short` prints nothing and `dotnet --version` prints `10.0.400`.

- [ ] **Step 2: Create the repository-wide configuration**

Create `.editorconfig`:

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,csproj,props,targets}]
indent_style = space
indent_size = 4

[*.{json,md,yml,yaml}]
indent_style = space
indent_size = 2

[*.cs]
dotnet_sort_system_directives_first = true
csharp_new_line_before_open_brace = all
```

Create `.gitignore`:

```gitignore
bin/
obj/
.vs/
.idea/
*.user
*.suo
TestResults/
BenchmarkDotNet.Artifacts/
```

Create `global.json`:

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  },
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

Create `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Deterministic>true</Deterministic>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  </PropertyGroup>
</Project>
```

Create `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="BenchmarkDotNet" Version="0.15.8" />
    <PackageVersion Include="xunit.v3" Version="3.2.2" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Create the runtime project files**

Create `src/Metaheuristics.Core/Metaheuristics.Core.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>Core abstractions and execution primitives for Metaheuristics.NET.</Description>
  </PropertyGroup>
</Project>
```

Create `src/Metaheuristics.Algorithms/Metaheuristics.Algorithms.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>Metaheuristic algorithm implementations for Metaheuristics.NET.</Description>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Metaheuristics.Core\Metaheuristics.Core.csproj" />
  </ItemGroup>
</Project>
```

Create `src/Metaheuristics.Experiments/Metaheuristics.Experiments.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>Reproducible experiment orchestration and reporting for Metaheuristics.NET.</Description>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Metaheuristics.Core\Metaheuristics.Core.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create the solution and register the runtime projects**

Run from the repository root:

```powershell
dotnet new sln --name Metaheuristics.NET --format slnx
dotnet sln Metaheuristics.NET.slnx add src/Metaheuristics.Core/Metaheuristics.Core.csproj --solution-folder src
dotnet sln Metaheuristics.NET.slnx add src/Metaheuristics.Algorithms/Metaheuristics.Algorithms.csproj --solution-folder src
dotnet sln Metaheuristics.NET.slnx add src/Metaheuristics.Experiments/Metaheuristics.Experiments.csproj --solution-folder src
```

Expected: each command reports that the project was added successfully.

- [ ] **Step 5: Restore, build, and inspect runtime references**

Run:

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet list src/Metaheuristics.Algorithms/Metaheuristics.Algorithms.csproj reference
dotnet list src/Metaheuristics.Experiments/Metaheuristics.Experiments.csproj reference
```

Expected:

```text
Build succeeded.
0 Warning(s)
0 Error(s)

Algorithms references: ../Metaheuristics.Core/Metaheuristics.Core.csproj
Experiments references: ../Metaheuristics.Core/Metaheuristics.Core.csproj
```

- [ ] **Step 6: Commit the build topology**

```powershell
git add .editorconfig .gitignore global.json Directory.Build.props Directory.Packages.props Metaheuristics.NET.slnx src
git commit -m "build: scaffold runtime projects"
```

---

### Task 2: Add development consumers and smoke verification

**Files:**
- Create: `examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj`
- Create: `examples/Metaheuristics.Examples/Program.cs`
- Create: `tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj`
- Create: `tests/Metaheuristics.Tests/RuntimeTargetTests.cs`
- Create: `benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj`
- Create: `benchmarks/Metaheuristics.Benchmarks/Program.cs`
- Modify: `Metaheuristics.NET.slnx`
- Modify: `README.md`

**Interfaces:**
- Consumes: the three runtime projects and central package versions created in Task 1.
- Produces: a six-project solution whose test host runs under .NET 10, whose example host executes, and whose development projects enforce the approved reference graph.

- [ ] **Step 1: Create the development project files**

Create `examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <Description>Runnable examples for Metaheuristics.NET.</Description>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Metaheuristics.Core\Metaheuristics.Core.csproj" />
    <ProjectReference Include="..\..\src\Metaheuristics.Algorithms\Metaheuristics.Algorithms.csproj" />
    <ProjectReference Include="..\..\src\Metaheuristics.Experiments\Metaheuristics.Experiments.csproj" />
  </ItemGroup>
</Project>
```

Create `tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Metaheuristics.Core\Metaheuristics.Core.csproj" />
    <ProjectReference Include="..\..\src\Metaheuristics.Algorithms\Metaheuristics.Algorithms.csproj" />
    <ProjectReference Include="..\..\src\Metaheuristics.Experiments\Metaheuristics.Experiments.csproj" />
  </ItemGroup>
</Project>
```

Create `benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <Description>Performance benchmarks for Metaheuristics.NET.</Description>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Metaheuristics.Core\Metaheuristics.Core.csproj" />
    <ProjectReference Include="..\..\src\Metaheuristics.Algorithms\Metaheuristics.Algorithms.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Add the executable hosts and configuration smoke test**

Create `examples/Metaheuristics.Examples/Program.cs`:

```csharp
Console.WriteLine("Metaheuristics.NET examples project is ready.");
```

Create `benchmarks/Metaheuristics.Benchmarks/Program.cs`:

```csharp
Console.WriteLine("Metaheuristics.NET benchmark project is ready.");
```

Create `tests/Metaheuristics.Tests/RuntimeTargetTests.cs`:

```csharp
namespace Metaheuristics.Tests;

public sealed class RuntimeTargetTests
{
    [Xunit.Fact]
    public void TestHostRunsOnNet10()
    {
        Xunit.Assert.Equal(10, Environment.Version.Major);
    }
}
```

The test catches an accidental change that makes the repository test host execute on a runtime major version other than the supported .NET 10 baseline. It uses the real test host and a hand-written literal expectation.

- [ ] **Step 3: Register the development projects in the solution**

Run:

```powershell
dotnet sln Metaheuristics.NET.slnx add examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --solution-folder examples
dotnet sln Metaheuristics.NET.slnx add tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj --solution-folder tests
dotnet sln Metaheuristics.NET.slnx add benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj --solution-folder benchmarks
```

Expected: each command reports that the project was added successfully.

- [ ] **Step 4: Document the developer workflow**

Append this section to `README.md` before `## 项目来源`:

````markdown
## 开发

环境要求：.NET SDK 10.0.400 或满足 `global.json` 滚动策略的兼容补丁版本。

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release --no-build
```

性能基准将在首个算法实现进入仓库时加入；基准宿主项目已预先建立，以固定依赖方向和构建入口。
````

- [ ] **Step 5: Restore, build, test, and run the example host**

Run:

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release --no-build
```

Expected:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
Test run summary: Passed
total: 1
failed: 0
Metaheuristics.NET examples project is ready.
```

- [ ] **Step 6: Verify every development-project reference**

Run:

```powershell
dotnet list examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj reference
dotnet list tests/Metaheuristics.Tests/Metaheuristics.Tests.csproj reference
dotnet list benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj reference
```

Expected:

```text
Examples references: Core, Algorithms, Experiments
Tests references: Core, Algorithms, Experiments
Benchmarks references: Core, Algorithms
```

- [ ] **Step 7: Commit the development consumers**

```powershell
git add README.md Metaheuristics.NET.slnx examples tests benchmarks
git commit -m "build: add development projects"
```

---

## Final Verification

- [ ] Run `dotnet restore Metaheuristics.NET.slnx` and confirm exit code 0.
- [ ] Run `dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore` and confirm 0 warnings and 0 errors.
- [ ] Run `dotnet test Metaheuristics.NET.slnx --configuration Release --no-build` and confirm 1 test passed and 0 failed.
- [ ] Run the Examples project and confirm the expected readiness message.
- [ ] Inspect all five project-reference lists and confirm they exactly match the approved dependency graph.
- [ ] Run `git status --short` and confirm the working tree is clean.
