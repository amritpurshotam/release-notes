using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new Repository(@"C:\Development\easy-equities");
            var latestTag = repo.Tags.OrderByDescending(x => x.FriendlyName).First();
            var lastCommitOnMaster = repo.Commits.Single(x => x.Sha.Equals(latestTag.Target.Sha));

            var numberOfCommits = 0;

            var ticketNumbers = new List<string>();
            foreach (var commit in repo.Commits)
            {
                if (commit.Sha.Equals(lastCommitOnMaster.Sha))
                {
                    break;
                }

                var matches = Regex.Matches(commit.MessageShort, "#\\d+");
                foreach (Match match in matches)
                {
                    var ticketNumber = match.Value.Replace("#", "");
                    ticketNumbers.Add(ticketNumber);
                }

                numberOfCommits++;
            }

            var lastCommit = repo.Commits.OrderByDescending(x => x.Committer.When).First();
            var changes = repo.Diff.Compare<TreeChanges>(lastCommitOnMaster.Tree, lastCommit.Tree);
            Console.WriteLine("Stats for nerds: ");
            Console.WriteLine("Files changed: " + changes.Count);
            Console.WriteLine("Commits since last release: " + numberOfCommits);
            Console.WriteLine("Tickets completed: " + ticketNumbers.Count);
            // Total points
            repo.Dispose();

            Console.ReadLine();
        }
    }
}