using System;
using System.Collections.Generic;
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
            var repo = new Repository(@"C:\Development\easy-equities");

            Pull(repo);
            var commits = GetNewCommits(repo);

            var numberOfCommits = 0;
            var ticketNumbers = GetTicketNumbers(commits, ref numberOfCommits);

            repo.Dispose();

            GenerateReleaseNotes(ticketNumbers, numberOfCommits);
            
            Console.ReadLine();
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
            Console.Write("### Release Notes");
            Console.WriteLine();

            var points = 0;
            var client = new RestClient("https://api.assembla.com/v1");
            foreach (var ticketNumber in ticketNumbers)
            {
                var ticket = GetAssemblaTicket(ticketNumber, client);
                if (ticket.ResponseStatus != ResponseStatus.Completed)
                {
                    continue;
                }

                points += ticket.Data.total_estimate;
                Console.WriteLine("[#{1}]({2}{1}) {0}<br/>", ticket.Data.summary, ticketNumber, "https://entelect.assembla.com/spaces/easy-equities/tickets/");
            }

            GenerateStats(numberOfCommits, ticketNumbers.Count, points);
        }

        private static void GenerateStats(int numberOfCommits, int numberOfTickets, int points)
        {
            Console.WriteLine("#### Stats");
            Console.WriteLine();
            Console.WriteLine("Commits : {0}<br/>", numberOfCommits);
            Console.WriteLine("Tickets : {0}<br/>", numberOfTickets);
            Console.WriteLine("Points  : {0}", points);
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