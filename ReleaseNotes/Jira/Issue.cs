using System.Security.Policy;

namespace ReleaseNotes
{
    public class Issue
    {
        public string key { get; set; }
        
        public Fields fields { get; set; }

        public string Url
        {
            get { return string.Format("https://purplegroup3.atlassian.net/browse/{0}", key); }
        }
    }
}