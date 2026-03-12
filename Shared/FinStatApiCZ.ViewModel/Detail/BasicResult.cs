using System.Text;

namespace FinstatApi
{
    public class BasicResult : AbstractResult
    {
        public string TaxPayer { get; set; }
        public string VatNumber { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("TaxPayer: {0}", TaxPayer));
            dataString.AppendLine(string.Format("VatNumber: {0}", VatNumber));

            return dataString.ToString();
        }
    }
}
