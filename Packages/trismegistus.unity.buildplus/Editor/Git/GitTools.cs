using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Trismegistus.BuildPlus.Git
{
    public static class GitTools
    {
        private const float CacheTimeSec = 60;
        private static readonly Dictionary<string, (string value, int cachedTime)> Cache = new Dictionary<string, (string, int)>();
        public static string GetBranch()
        {
            return RunGitCommand("rev-parse --abbrev-ref HEAD");
        }
        
        public static string GetRemote()
        {
            return RunGitCommand("config --get remote.origin.url");
        }
        
        public static string GetCommitHash()
        {
            return RunGitCommand("rev-parse HEAD");
        }
        
        public static string GetCommitHashShort()
        {
            return RunGitCommand("rev-parse --short HEAD");
        }
        
        public static string GetCommitMessage()
        {
            var msg = RunGitCommand("log -1 --pretty=%B");
            return msg.Replace("\n", " ").Replace("\r", " ").ReplaceUnsupportedSymbols();
        }
        
        public static string GetCommitAuthor()
        {
            return RunGitCommand("log -1 --pretty=%an");
        }
        
        public static string GetCommitDate()
        {
            return RunGitCommand("log -1 --pretty=%ad");
        }
        
        public static string GetCommitTime()
        {
            return RunGitCommand("log -1 --pretty=%at");
        }
        
        public static string GetCommitTimeIso()
        {
            return RunGitCommand("log -1 --pretty=%ai");
        }
        
        public static string GetCommitTimeRfc()
        {
            return RunGitCommand("log -1 --pretty=%aI");
        }
        
        public static string GetCommitTimeRelative()
        {
            return RunGitCommand("log -1 --pretty=%ar");
        }
        
        public static string GetCommitTimeShort()
        {
            return RunGitCommand("log -1 --pretty=%cr");
        }
        
        private static string RunGitCommand(string command)
        {
            if (Cache.TryGetValue(command, out var cache))
            {
                if (cache.cachedTime + CacheTimeSec > EditorApplication.timeSinceStartup)
                {
                    return cache.value;
                }
            }
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            // remove ending newline
            result = result.Substring(0, result.Length - 1);
            Cache[command] = (result, (int)EditorApplication.timeSinceStartup);
            return result;
        }
    }
}