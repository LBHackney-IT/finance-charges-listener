using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface ICustomeHttpClient
    {
        public void AddAuthorization(AuthenticationHeaderValue headerValue);

        public Task<HttpResponseMessage> GetAsync(Uri uri);

        public Task<HttpResponseMessage> PatchAsync(Uri uri, HttpContent content);
    }
}
