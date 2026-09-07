using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FinstatApi
{
    [Serializable]
    public class FinstatApiException : Exception
    {
        public enum FailTypeEnum
        {
            UrlNotFound,
            NotFound,
            NotValidCustomerKey,
            OtherCommunicationFail,
            Timeout,
            LimitExceed,
            TooShort,
            Unknown,
            LicenseExpired,
            InsufficientAccess,
            AccessDisabled,
            InvalidHash,
            BadRequest,
            Unauthorized,
            GdprRestriction,
            RateLimitExceeded,
            RegisterUnavailable
        }

        public FailTypeEnum FailType { get; set; }

        public FinstatApiException()
        {
        }

        public FinstatApiException(FailTypeEnum failType, string message)
            : base(message)
        {
            FailType = failType;
        }

        public FinstatApiException(FailTypeEnum failType, string message, Exception inner)
            : base(message, inner)
        {
            FailType = failType;
        }

        protected FinstatApiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            FailType = (FailTypeEnum)info.GetValue("FailType", typeof(FailTypeEnum));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("FailType", FailType);
            base.GetObjectData(info, context);
        }
    }
}
