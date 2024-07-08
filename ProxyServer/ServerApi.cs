using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Net;

namespace ProxyServer
{
#if API
    internal class ServerApi : BaseApi
    {
        readonly Uri _endpoint;
        string Host
        {
            get
            {
                return $"{_endpoint.Scheme}://{_endpoint.Authority}";
            }
        }
        public ServerApi(Uri endpoint)
        {
            this._endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }


        public Task<PingResponse> PingAsync(PingRequest pingRequest, CancellationToken cancellationToken = default)
            => Build()
            .WithUrlPostJson(new UrlBuilder(Host, "/api/proxy/update"), pingRequest)
            .ExecuteAsync<PingResponse>(cancellationToken);


        public class PingRequest
        {
            public string? IP { get; set; }
            public int Port { get; set; }
        }
        public class PingResponse
        {
            public List<string>? DomainAllows { get; set; }
            public List<string>? IpAddessAllows { get; set; }
        }
    }
#endif
}
