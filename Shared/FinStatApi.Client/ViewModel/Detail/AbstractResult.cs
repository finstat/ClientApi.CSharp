using System;
using System.Text;

namespace FinstatApi
{
    public class AbstractResult : FullAddress
    {
        public string Ico { get; set; }
        public string Url { get; set; }

        public DateTime? Created { get; set; }
        public DateTime? Cancelled { get; set; }
        public bool SuspendedAsPerson { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(string.Format("ICO: {0}", Ico));
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("SuspendedAsPerson: {0}", SuspendedAsPerson ? "yes" : "no"));
            dataString.AppendLine(string.Format("Created: {0}", Created));
            dataString.AppendLine(string.Format("Canceled: {0}", Cancelled));
            dataString.AppendLine(string.Format("URL: {0}", Url));

            return dataString.ToString();
        }
    }
}
