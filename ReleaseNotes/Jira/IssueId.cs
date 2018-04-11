using System;

namespace ReleaseNotes
{
    public class IssueId
    {
        public IssueId(string projectKey)
        {
            var splits = projectKey.Split('-');
            Prefix = splits[0];
            Number = int.Parse(splits[1]);
        }

        public string Prefix { get; private set; }
        public int Number { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Prefix, Number);
        }
    }
}