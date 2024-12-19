#r "nuget: CloudTek.Git, 1.0.11"

using CloudTek.Git;

return CommitMessageAnalyzer.AnalyzeCommitsFromFile(Args != null && Args.Count() > 0 ? Args[0] : ".git/COMMIT_EDITMSG");