using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FinstatApi
{
    public class WebClientWithTimeout : WebClient
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        private readonly X509Certificate2 certificate;


        /// <summary>
        /// Initializes a new instance of the <see cref="WebClientWithTimeout"/> class.
        /// </summary>
        public WebClientWithTimeout() : this(60000, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClientWithTimeout"/> class.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public WebClientWithTimeout(int timeout, X509Certificate2 cert = null)
        {
            this.Timeout = timeout;
            this.certificate = cert;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = this.Timeout;
            }
            if (certificate != null)
            {
                request.ClientCertificates.Add(certificate);
            }

            return request;
        }
    }
}
