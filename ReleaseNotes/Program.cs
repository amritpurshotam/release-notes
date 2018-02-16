using System;
using System.Net;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using RestSharp;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var gitRepositoryPath = "C:\\Development\\easyequities";
            var assemblaSpaceId = "dY1u76DDur54kQdmr6bg7m";
            var branch = "develop";

            var repo = new Repository(gitRepositoryPath);
            Pull(repo);

            var commits = GetNewCommitsFrom(repo, branch);
            var statistics = new Statistics(commits);

            GenerateReleaseNotes(statistics, assemblaSpaceId);

            Console.ReadLine();
        }

        private static ICommitLog GetNewCommitsFrom(Repository repo, string branch)
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = repo.Branches[string.Format("origin/{0}", branch)],
                ExcludeReachableFrom = repo.Branches["origin/master"]
            };
            var commits = repo.Commits.QueryBy(filter);
            return commits;
        }

        private static void GenerateReleaseNotes(Statistics statistics, string assemblaSpaceId)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.assembla.com/v1");

            var space = GetAssemblaSpace(assemblaSpaceId, client);
            if (space.ResponseStatus == ResponseStatus.Completed)
            {
                foreach (var ticketNumber in statistics.SortedTicketNumbers)
                {
                    var ticket = GetAssemblaTicket(ticketNumber, assemblaSpaceId, client);
                    if (ticket.StatusCode != HttpStatusCode.OK)
                    {
                        statistics.RemoveTicketNumber(ticketNumber);
                        continue;
                    }

                    statistics.AddPoints(ticket.Data.total_estimate);
                    var ticketSummary = RemoveDoubleQuotesFrom(ticket.Data.summary);

                    const string twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph = "  ";
                    Console.WriteLine("[#{0}](https://{1}.assembla.com/spaces/{2}/tickets/{0}) - {3}{4}",
                        ticketNumber, Settings.AssemblaSubDomain, space.Data.wiki_name,
                        ticketSummary, twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph);
                }
            }

            Console.WriteLine();
            Console.WriteLine(statistics.ToString());
        }

        private static string RemoveDoubleQuotesFrom(string note)
        {
            // powershell cannot convert strings to json with double quotes resulting in the Slack message not being sent
            return note.Replace("\"", "");
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