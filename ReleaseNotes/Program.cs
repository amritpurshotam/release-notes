using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using RestSharp;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new Repository(args[0]);
            Pull(repo);

            var numberOfCommits = 0;
            var commits = GetNewCommits(repo);
            var ticketNumbers = GetTicketNumbers(commits, ref numberOfCommits);

            GenerateReleaseNotes(ticketNumbers, numberOfCommits);
        }

        private static List<string> GetTicketNumbers(ICommitLog commits, ref int numberOfCommits)
        {
            var ticketNumbers = new List<string>();
            foreach (var commit in commits)
            {
                var matches = Regex.Matches(commit.MessageShort, "#\\d+");
                foreach (Match match in matches)
                {
                    var ticketNumber = match.Value.Replace("#", "");
                    ticketNumbers.Add(ticketNumber);
                }

                numberOfCommits++;
            }

            return ticketNumbers;
        }

        private static void GenerateReleaseNotes(List<string> ticketNumbers, int numberOfCommits)
        {
            var points = 0;
            var client = new RestClient("https://api.assembla.com/v1");

            ticketNumbers = ticketNumbers.OrderBy(x => x).ToList();
            foreach (var ticketNumber in ticketNumbers)
            {
                var ticket = GetAssemblaTicket(ticketNumber, client);
                if (ticket.ResponseStatus != ResponseStatus.Completed)
                {
                    continue;
                }

                points += ticket.Data.total_estimate;

                const string twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph = "  ";
                Console.WriteLine("[#{0}](https://purplegroup.assembla.com/spaces/software-development-backlog/tickets/{0}) - {1}{2}", 
                    ticketNumber, ticket.Data.summary, twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph);
            }

            GenerateStats(numberOfCommits, ticketNumbers.Count, points);
        }

        private static void GenerateStats(int numberOfCommits, int numberOfTickets, int points)
        {
            Console.WriteLine();
            Console.WriteLine("Commits: {0}. Tickets: {1}. Points: {2}.", numberOfCommits, numberOfTickets, points);
        }

        private static IRestResponse<Ticket> GetAssemblaTicket(string ticketNumber, RestClient client)
        {
            var request = new RestRequest(string.Format("spaces/{0}/tickets/{1}", Keys.AssemblaSpaceId, ticketNumber));
            request.AddHeader("X-Api-Key", Keys.AssemblaApiKey);
            request.AddHeader("X-Api-Secret", Keys.AssemblaApiSecret);

            var ticket = client.Execute<Ticket>(request);
            return ticket;
        }

        private static ICommitLog GetNewCommits(Repository repo)
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = repo.Branches["origin/develop"],
                ExcludeReachableFrom = repo.Branches["origin/master"]
            };
            var commits = repo.Commits.QueryBy(filter);
            return commits;
        }

        private static void Pull(Repository repo)
        {
            PullOptions options = new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = Keys.GitUserName,
                                Password = Keys.GitPassword
                            })
                }
            };
            Commands.Pull(repo, new Signature(Keys.GitUserName, Keys.EmailAddress, new DateTimeOffset(DateTime.Now)), options);
        }
    }
}