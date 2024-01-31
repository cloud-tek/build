# CloudTek.Git

CloudTek.Git is a NuGet package consumed via Husky.NET in order to validate conventional commits.

## Usage

- Install Husky using offical [documentation](https://alirezanet.github.io/Husky.Net/guide/getting-started.html#installation)
- Follow the [guide](https://alirezanet.github.io/Husky.Net/guide/csharp-script.html) on using C# with git hooks
- Create a `.csx` script to be executed by a git hook
- Use the following code in the hook:

```csharp
// commit-lint.csx
#r "nuget: CloudTek.Git, 1.0.0"
using CloudTek.Git;

private var msg = File.ReadAllLines(Args[0])[0];

return (CommitMessageAnalyzer.Analyze(msg) == CommitMessageAnalysisResult.Ok)
  ? 0
  : 1;
```