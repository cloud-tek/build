#r "nuget: CloudTek.Git"

using CloudTek.Git;

return CommitMessageAnalyzer.AnalyzeCommitsFromLog(args: Args != null && Args.Count() > 0 ? Args[0] : "origin/main..");