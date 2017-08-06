using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using RestSharp;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new Repository(@"C:\Development\easy-equities");

            var filter = new CommitFilter
            {
                IncludeReachableFrom = repo.Branches["origin/develop"],
                ExcludeReachableFrom = repo.Branches["origin/master"]
            };
            var commits = repo.Commits.QueryBy(filter);

            var numberOfCommits = 0;
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

            var points = 0;
            var client = new RestClient("https://api.assembla.com/v1");
            foreach (var ticketNumber in ticketNumbers)
            {
                var request = new RestRequest(string.Format("spaces/{0}/tickets/{1}", ApiKeys.AssemblaSpaceId, ticketNumber));
                request.AddHeader("X-Api-Key", ApiKeys.AssemblaApiKey);
                request.AddHeader("X-Api-Secret", ApiKeys.AssemblaApiSecret);

                var ticket = client.Execute<Ticket>(request);
                if (ticket.ResponseStatus != ResponseStatus.Completed)
                {
                    continue;
                }

                points += ticket.Data.total_estimate;
                Console.WriteLine("[#{1}]({2}{1}) {0}", ticket.Data.summary, ticketNumber, "https://entelect.assembla.com/spaces/easy-equities/tickets/");
            }

            Console.WriteLine("Stats for nerds: ");
            Console.WriteLine("New commits in release: " + numberOfCommits);
            Console.WriteLine("Tickets completed: " + ticketNumbers.Count);
            Console.WriteLine("Total points: " + points);

            repo.Dispose();

            Console.ReadLine();
        }
    }
}