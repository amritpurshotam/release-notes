using System.Configuration;

namespace ReleaseNotes
{
    public static class Settings
    {
        public static string GitUserName = ConfigurationManager.AppSettings["GitUserName"];
        public static string GitPassword = ConfigurationManager.AppSettings["GitPassword"];
        public static string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"];
        public static string JiraUsername = ConfigurationManager.AppSettings["JiraUsername"];
        public static string JiraPassword = ConfigurationManager.AppSettings["JiraPassword"];
    }
}