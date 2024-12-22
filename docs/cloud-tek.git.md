# CloudTek.Git

CloudTek.Git is a NuGet package consumed via Husky.NET in order to validate conventional commits.

## Usage

- Install Husky using offical [documentation](https://alirezanet.github.io/Husky.Net/guide/getting-started.html#installation)
- Follow the [guide](https://alirezanet.github.io/Husky.Net/guide/csharp-script.html) on using C# with git hooks
- Create a `.csx` script to be executed by a git hook
- Use the following code in the hooks:

```csharp
// commit-lint.csx
#r "nuget: CloudTek.Git, 1.0.11"

using CloudTek.Git;

return CommitMessageAnalyzer.AnalyzeCommitsFromFile(Args != null && Args.Count() > 0 ? Args[0] : ".git/COMMIT_EDITMSG");
```

```csharp
// commit-lint-ci.csx
#r "nuget: CloudTek.Git, 1.0.11"

using CloudTek.Git;

return CommitMessageAnalyzer.AnalyzeCommitsFromLog(args: Args != null && Args.Count() > 0 ? Args[0] : "origin/main..");
```

- Ensure the `task-runner.json` uses `dotnet-script` in order to properly invoke the hooks, due to them referencing external nuget packages.

```json
{
  "tasks": [
    {
      "name": "commit-message-linter",
      "group": "commit-msg",
      "command": "dotnet",
      "args": ["script", ".husky/csx/commit-lint.csx", "--", "${args}"]
    },
    {
      "name": "commit-message-linter-ci",
      "group": "commit-msg",
      "command": "dotnet",
      "args": ["script", ".husky/csx/commit-lint-ci.csx", "--", "${args}"]
    }
  ]
}
```