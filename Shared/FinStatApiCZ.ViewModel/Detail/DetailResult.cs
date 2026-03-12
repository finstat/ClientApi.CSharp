using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace FinstatApi
{
    public class DetailResult : CommonResult
    {
        public string CzNaceCode { get; set; }
        public string CzNaceText { get; set; }
        public string CzNaceDivision { get; set; }
        public string CzNaceGroup { get; set; }

        public string LegalForm { get; set; }
        public string OwnershipType { get; set; }
        public string EmployeeCount { get; set; }

        public override string ToString()
        {
            StringBuilder dataString = new StringBuilder();
            dataString.AppendLine(base.ToString());
            dataString.AppendLine(string.Format("CzNace: {0} {1}", CzNaceCode, CzNaceText));
            dataString.AppendLine(string.Format("CzNaceDivision: {0}", CzNaceDivision));
            dataString.AppendLine(string.Format("CzNaceGroup: {0}", CzNaceGroup));
            dataString.AppendLine(string.Format("LegalForm: {0}", LegalForm));
            dataString.AppendLine(string.Format("OwnershipType: {0}", OwnershipType));
            dataString.AppendLine(string.Format("EmployeeCount: {0}", EmployeeCount));
            return dataString.ToString();
        }
    }
}