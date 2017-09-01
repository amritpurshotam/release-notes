using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            var gitRespositoryPath = args[0];
            var assemblaSpaceId = args[1];
            var branch = args[2];

            var repo = new Repository(gitRespositoryPath);
            Pull(repo);

            var numberOfCommits = 0;
            var commits = GetNewCommits(repo, branch);
            var ticketNumbers = GetTicketNumbers(commits, ref numberOfCommits);

            GenerateReleaseNotes(ticketNumbers, assemblaSpaceId, numberOfCommits);
        }

        private static List<int> GetTicketNumbers(ICommitLog commits, ref int numberOfCommits)
        {
            var ticketNumbers = new List<int>();
            foreach (var commit in commits)
            {
                var matches = Regex.Matches(commit.MessageShort, "#\\d+");
                foreach (Match match in matches)
                {
                    int ticketNumber;
                    if (Int32.TryParse(match.Value.Replace("#", ""), out ticketNumber))
                    {
                        ticketNumbers.Add(ticketNumber);
                    }
                }

                numberOfCommits++;
            }

            return ticketNumbers;
        }

        private static void GenerateReleaseNotes(List<int> ticketNumbers, string assemblaSpaceId, int numberOfCommits)
        {
            var points = 0;
            var client = new RestClient("https://api.assembla.com/v1");

            var space = GetAssemblaSpace(assemblaSpaceId, client);
            if (space.ResponseStatus == ResponseStatus.Completed)
            {
                ticketNumbers = ticketNumbers.OrderBy(x => x).ToList();
                foreach (var ticketNumber in ticketNumbers)
                {
                    var ticket = GetAssemblaTicket(ticketNumber, assemblaSpaceId, client);
                    if (ticket.ResponseStatus != ResponseStatus.Completed)
                    {
                        continue;
                    }

                    points += ticket.Data.total_estimate;
                    var ticketSummary = RemoveDoubleQuotesFrom(ticket.Data.summary);

                    const string twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph = "  ";
                    Console.WriteLine("[#{0}](https://{1}.assembla.com/spaces/{2}/tickets/{0}) - {3}{4}",
                        ticketNumber, Settings.AssemblaSubDomain, space.Data.wiki_name,
                        ticketSummary, twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph);
                }
            }

            GenerateStats(numberOfCommits, ticketNumbers.Count, points);
        }

        private static string RemoveDoubleQuotesFrom(string note)
        {
            // powershell cannot convert strings to json with double quotes resulting in the Slack message not being sent
            return note.Replace("\"", "");
        }

        private static void GenerateStats(int numberOfCommits, int numberOfTickets, int points)
        {
            Console.WriteLine();
            Console.WriteLine("Commits: {0}. Tickets: {1}. Points: {2}.", numberOfCommits, numberOfTickets, points);
        }

        private static IRestResponse<Space> GetAssemblaSpace(string assemblaSpaceId, RestClient client)
        {
            var request = new RestRequest(string.Format("spaces/{0}", assemblaSpaceId));
            request.AddHeader("X-Api-Key", Settings.AssemblaApiKey);
            request.AddHeader("X-Api-Secret", Settings.AssemblaApiSecret);

            var space = client.Execute<Space>(request);
            return space;
        }

        private static IRestResponse<Ticket> GetAssemblaTicket(int ticketNumber, string assemblaSpaceId, RestClient client)
        {
            var request = new RestRequest(string.Format("spaces/{0}/tickets/{1}", assemblaSpaceId, ticketNumber));
            request.AddHeader("X-Api-Key", Settings.AssemblaApiKey);
            request.AddHeader("X-Api-Secret", Settings.AssemblaApiSecret);

            var ticket = client.Execute<Ticket>(request);
            return ticket;
        }

        private static ICommitLog GetNewCommits(Repository repo, string branch)
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = repo.Branches[string.Format("origin/{0}", branch)],
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
                                Username = Settings.GitUserName,
                                Password = Settings.GitPassword
                            })
                }
            };
            Commands.Pull(repo, new Signature(Settings.GitUserName, Settings.EmailAddress, new DateTimeOffset(DateTime.Now)), options);
        }
    }
}