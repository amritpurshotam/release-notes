using RestSharp;
using RestSharp.Authenticators;

namespace ReleaseNotes
{
    public static class JiraService
    {
        public static Issue GetIssue(string key)
        {
            var client = new RestClient("https://purplegroup3.atlassian.net/rest/api/2");
            client.Authenticator = new HttpBasicAuthenticator(Settings.JiraUsername, Settings.JiraPassword);
            var request = new RestRequest(string.Format("issue/{0}", key));
            var issue = client.Execute<Issue>(request);
            return issue.Data;
        }
    }
}