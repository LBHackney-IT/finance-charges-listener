using AutoFixture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceChargesListener.Tests.E2ETests.Fixtures
{
    public abstract class BaseApiFixture<T> : IDisposable where T : class
    {
        protected readonly Fixture Fixture = new Fixture();
        private readonly JsonSerializerOptions _jsonOptions;
        private HttpListener _httpListener;

        protected string Route;
        protected string Token;

        public T ResponseObject { get; protected set; }

        protected BaseApiFixture()
        {
            _jsonOptions = global::FinanceChargesListener.Infrastructure.JsonOptions.CreateJsonOptions();
            StartApiStub();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || Disposed) return;
            if (_httpListener.IsListening)
                _httpListener.Stop();

            Disposed = true;
        }

        protected virtual void StartApiStub()
        {
            Task.Run(() =>
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(Route);
                _httpListener.Start();

                // GetContext method blocks while waiting for a request. 
                var context = _httpListener.GetContext();
                var response = context.Response;

                if (context.Request.Headers["Authorization"] != $"Bearer {Token}")
                {
                    response.StatusCode = (int) HttpStatusCode.Unauthorized;
                }
                else
                {
                    response.StatusCode = (int) ((ResponseObject is null) ? HttpStatusCode.NotFound : HttpStatusCode.OK);
                    var responseBody = ResponseObject is null ? context.Request.Url.Segments.Last() : JsonSerializer.Serialize(ResponseObject, _jsonOptions);
                    var stream = response.OutputStream;
                    using var writer = new StreamWriter(stream);
                    writer.Write(responseBody);
                    writer.Close();
                }
            });
        }
    }
}
