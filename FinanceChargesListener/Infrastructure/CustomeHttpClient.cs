using FinanceChargesListener.Infrastructure.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FinanceChargesListener.Infrastructure
{
    public class CustomeHttpClient : HttpClient, ICustomeHttpClient
    {
        public CustomeHttpClient()
        {
            Timeout = TimeSpan.FromMinutes(3);
        }

        public void AddAuthorization(AuthenticationHeaderValue headerValue)
        {
            if (headerValue != null)
            {
                DefaultRequestHeaders.Authorization = headerValue;
            }
        }
    }
}
