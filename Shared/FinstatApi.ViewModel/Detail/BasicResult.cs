using System.Text;

namespace FinstatApi
{
    public class BasicResult : AbstractResult
    {
        public string IcDPH { get; set; }
        public string Paragraph { get; set; }
        public string Dic { get; set; }
        public bool Anonymized { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();

            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("Dic: {0}", Dic));
            dataString.AppendLine(string.Format("IcDPH: {0} {1}", IcDPH, Paragraph));
            dataString.AppendLine(string.Format("Anonymized: {0}", Anonymized));
            return dataString.ToString();
        }
    }
}
