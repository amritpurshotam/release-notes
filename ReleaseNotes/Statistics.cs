using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ReleaseNotes
{
    public class Statistics
    {
        private IList<IssueId> IssueIds;

        public Statistics(ICommitLog commits)
        {
            IssueIds = new List<IssueId>();
            NumberOfCommits = 0;
            Points = 0;
            LastCommitSha = commits.First().Sha;

            ProcessCommits(commits);
        }

        public int NumberOfCommits { get; private set; }
        public decimal Points { get; private set; }
        public string LastCommitSha { get; private set; }

        public IEnumerable<IssueId> SortedIssueIds
        {
            get { return IssueIds.OrderBy(x => x.Number); }
        }

        public int TicketNumberCount
        {
            get { return IssueIds.Count; }
        }

        public void AddPoints(decimal points)
        {
            Points = Points + points;
        }

        public void RemoveIssueId(IssueId issueId)
        {
            IssueIds.Remove(issueId);
        }

        public override string ToString()
        {
            return string.Format("Commits: {0}. Tickets: {1}. Points: {2}.  \nSha: {3}",
                NumberOfCommits, TicketNumberCount, Points, LastCommitSha);
        }

        private void ProcessCommits(ICommitLog commits)
        {
            foreach (var commit in commits)
            {
                var message = RemovePullRequestSubstring(commit.Message);
                AddTicketNumbersFrom(message);
                IncrementNumberOfCommits();
            }
        }

        private void AddTicketNumbersFrom(string message)
        {
            var matches = Regex.Matches(message, "[A-Z]{2}-\\d+");
            foreach (Match match in matches)
            {
                var projectKey = new IssueId(match.Value);
                AddIssueId(projectKey);
            }
        }

        private void IncrementNumberOfCommits()
        {
            NumberOfCommits = NumberOfCommits + 1;
        }

        private void AddIssueId(IssueId issueId)
        {
            if (IsUnique(issueId))
            {
                IssueIds.Add(issueId);
            }
        }

        private bool IsUnique(IssueId issueId)
        {
            return IssueIds.All(x => x.Number != issueId.Number);
        }

        private string RemovePullRequestSubstring(string message)
        {
            var pullRequestMatches = Regex.Matches(message, "Merge pull request #\\d+");
            foreach (Match pullRequestMatch in pullRequestMatches)
            {
                message = message.Replace(pullRequestMatch.Value, "");
            }
            return message;
        }
    }
}