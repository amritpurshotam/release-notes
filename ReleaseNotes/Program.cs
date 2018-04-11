using System;
using System.Net;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using RestSharp;
using RestSharp.Authenticators;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var gitRepositoryPath = args[0];
            var branch = args[1];

            var repo = new Repository(gitRepositoryPath);
            Pull(repo);

            var commits = GetNewCommitsFrom(repo, branch);
            var statistics = new Statistics(commits);

            GenerateReleaseNotes(statistics);
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

        private static void GenerateReleaseNotes(Statistics statistics)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new RestClient("https://purplegroup3.atlassian.net/rest/api/2");
            client.Authenticator = new HttpBasicAuthenticator(Settings.JiraUsername, Settings.JiraPassword);
            foreach (var issueId in statistics.SortedIssueIds)
            {
                var request = new RestRequest(string.Format("issue/{0}", issueId));
                var issue = client.Execute<Issue>(request);
                if (issue.StatusCode != HttpStatusCode.OK)
                {
                    statistics.RemoveIssueId(issueId);
                    continue;
                }

                if (issue.Data.IsSubTask)
                {
                    statistics.RemoveIssueId(issueId);
                    continue;
                }

                statistics.AddPoints(issue.Data.fields.customfield_10014);
                var ticketSummary = RemoveDoubleQuotesFrom(issue.Data.fields.summary);

                const string twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph = "  ";
                Console.WriteLine("[{0}]({1}) - {2}{3}",
                    issueId, issue.Data.Url, ticketSummary, twoSpacesNeededForMarkdownToMakeANewLineInSameParagraph);
            }

            Console.WriteLine();
            Console.WriteLine(statistics.ToString());
        }

        private static string RemoveDoubleQuotesFrom(string note)
        {
            // powershell cannot convert strings to json with double quotes resulting in the Slack message not being sent
            return note.Replace("\"", "");
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