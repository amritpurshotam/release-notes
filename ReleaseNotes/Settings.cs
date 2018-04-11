using System.Configuration;

namespace ReleaseNotes
{
    public static class Settings
    {
        public static string AssemblaApiKey = ConfigurationManager.AppSettings["AssemblaApiKey"];
        public static string AssemblaApiSecret = ConfigurationManager.AppSettings["AssemblaApiSecret"];
        public static string AssemblaSpaceId = ConfigurationManager.AppSettings["AssemblaSpaceId"];
        public static string AssemblaSpaceName = ConfigurationManager.AppSettings["AssemblaSpaceName"];
        public static string AssemblaSubDomain = ConfigurationManager.AppSettings["AssemblaSubDomain"];
        public static string GitUserName = ConfigurationManager.AppSettings["GitUserName"];
        public static string GitPassword = ConfigurationManager.AppSettings["GitPassword"];
        public static string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"];
        public static string JiraUsername = ConfigurationManager.AppSettings["JiraUsername"];
        public static string JiraPassword = ConfigurationManager.AppSettings["JiraPassword"];
    }
}