using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ReleaseNotes
{
    public class Statistics
    {
        private IList<int> TicketNumbers;

        public Statistics(ICommitLog commits)
        {
            TicketNumbers = new List<int>();
            NumberOfCommits = 0;
            Points = 0;
            LastCommitSha = commits.First().Sha;

            ProcessCommits(commits);
        }

        public int NumberOfCommits { get; private set; }
        public int Points { get; private set; }
        public string LastCommitSha { get; private set; }

        public IEnumerable<int> SortedTicketNumbers
        {
            get { return TicketNumbers.OrderBy(x => x); }
        }

        public int TicketNumberCount
        {
            get { return TicketNumbers.Count; }
        }

        public void AddPoints(int points)
        {
            Points = Points + points;
        }

        public void RemoveTicketNumber(int ticketNumber)
        {
            TicketNumbers.Remove(ticketNumber);
        }

        public override string ToString()
        {
            return string.Format("Commits: {0}. Tickets: {1}.  \nSha: {2}.",
                NumberOfCommits, TicketNumberCount, LastCommitSha);
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
            var matches = Regex.Matches(message, "#\\d+");
            foreach (Match match in matches)
            {
                AddTicketNumber(match.Value);
            }
        }

        private void IncrementNumberOfCommits()
        {
            NumberOfCommits = NumberOfCommits + 1;
        }

        private void AddTicketNumber(string ticketNumberString)
        {
            int ticketNumber;
            if (Int32.TryParse(ticketNumberString.Replace("#", ""), out ticketNumber))
            {
                AddTicketNumber(ticketNumber);
            }
        }

        private void AddTicketNumber(int ticketNumber)
        {
            if (IsUnique(ticketNumber))
            {
                TicketNumbers.Add(ticketNumber);
            }
        }

        private bool IsUnique(int ticketNumber)
        {
            return TicketNumbers.All(x => x != ticketNumber);
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