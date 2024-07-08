using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Proxy.Handlers;
using TqkLibrary.Proxy.Interfaces;
using TqkLibrary.Proxy.ProxySources;

namespace ProxyServer
{
    internal class CustomHttpProxyServerHandler : HttpProxyServerHandler
    {
        readonly LocalProxySource _localProxySource = new LocalProxySource();

#if API
        ServerApi.PingResponse? _pingResponse;
        public void Update(ServerApi.PingResponse pingResponse)
        {
            _pingResponse = pingResponse;
        }
#endif


        public override Task<bool> IsAcceptDomainFilterAsync(Uri uri, CancellationToken cancellationToken = default)
        {
#if API
            if (uri is null) return Task.FromResult(false);

            return Task.FromResult(
                    _pingResponse?.DomainAllows?.Any() != true ||//null or empty return true
                    _pingResponse?.DomainAllows?.Any(x => uri.Host.Contains(x, StringComparison.OrdinalIgnoreCase)) == true
                    );
#else
            return base.IsAcceptDomainFilterAsync(uri, cancellationToken);
#endif
        }

        public override Task<bool> IsAcceptClientFilterAsync(TcpClient tcpClient, CancellationToken cancellationToken = default)
        {
#if API
            if (tcpClient?.Client?.RemoteEndPoint is IPEndPoint iPEndPoint)
            {
                string address = iPEndPoint.Address.ToString();
                return Task.FromResult(
                    _pingResponse?.IpAddessAllows?.Any() != true ||//null or empty return true
                    _pingResponse?.IpAddessAllows?.Any(x => x.Equals(address, StringComparison.OrdinalIgnoreCase)) == true
                    );
            }
            return Task.FromResult(false);
#else
            return base.IsAcceptClientFilterAsync(tcpClient, cancellationToken);
#endif
        }

        public override Task<IProxySource> GetProxySourceAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IProxySource>(_localProxySource);
        }
    }
}
