using System.Text;

namespace FinstatApi
{
    public class CommonResult : AbstractResult
    {
        public string Activity { get; set; }
        public bool Warning { get; set; }
        public string WarningUrl { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("Activity: {0}", Activity));
            dataString.AppendLine(string.Format("Warning: {0}", Warning + " " + WarningUrl));

            return dataString.ToString();
        }
    }
}
