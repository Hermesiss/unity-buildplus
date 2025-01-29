using Trismegistus.BuildPlus.Git;

namespace Trismegistus.BuildPlus
{
    [BuildPathProvider]
    public static class StandardGitParameters
    {
        [BuildPath]
        public static PathParameter Branch()
        {
            return new PathParameter("git_branch", "Git", GitTools.GetBranch(), "Current git branch");
        }
        
        [BuildPath]
        public static PathParameter Remote()
        {
            return new PathParameter("git_remote", "Git", GitTools.GetRemote(), "Current git remote URL");
        }
        
        
        [BuildPath]
        public static PathParameter CommitHash()
        {
            return new PathParameter("git_commit_hash", "Git", GitTools.GetCommitHash(), "Current git commit hash");
        }
        
        [BuildPath]
        public static PathParameter CommitHashShort()
        {
            return new PathParameter("git_commit_hash_short", "Git", GitTools.GetCommitHashShort(), "Current git commit hash short");
        }
        
        [BuildPath]
        public static PathParameter CommitMessage()
        {
            return new PathParameter("git_commit_message", "Git", GitTools.GetCommitMessage(), "Current git commit message");
        }
        
        [BuildPath]
        public static PathParameter CommitAuthor()
        {
            return new PathParameter("git_commit_author", "Git", GitTools.GetCommitAuthor(), "Current git commit author");
        }
        
        [BuildPath]
        public static PathParameter CommitDate()
        {
            return new PathParameter("git_commit_date", "Git", GitTools.GetCommitDate(), "Current git commit date");
        }
        
        [BuildPath]
        public static PathParameter CommitTime()
        {
            return new PathParameter("git_commit_time", "Git/Time", GitTools.GetCommitTime(), "Current git commit time");
        }
        
        [BuildPath]
        public static PathParameter CommitTimeIso()
        {
            return new PathParameter("git_commit_time_iso", "Git/Time", GitTools.GetCommitTimeIso(), "Current git commit time iso");
        }
        
        [BuildPath]
        public static PathParameter CommitTimeRfc()
        {
            return new PathParameter("git_commit_time_rfc", "Git/Time", GitTools.GetCommitTimeRfc(), "Current git commit time rfc");
        }
        
        [BuildPath]
        public static PathParameter CommitTimeRelative()
        {
            return new PathParameter("git_commit_time_relative", "Git/Time", GitTools.GetCommitTimeRelative(), "Current git commit time relative");
        }
        
        [BuildPath]
        public static PathParameter CommitTimeShort()
        {
            return new PathParameter("git_commit_time_short", "Git/Time", GitTools.GetCommitTimeShort(), "Current git commit time short");
        }
        
    }
}