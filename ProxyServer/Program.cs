using Microsoft.Extensions.Logging;
using ProxyServer;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TqkLibrary.Proxy.Handlers;
using TqkLibrary.Proxy.ProxyServers;

TqkLibrary.Proxy.Singleton.LoggerFactory = LoggerFactory.Create(x => x.AddConsole());

Configure? configure = Configure.Load();
if (configure is null) configure = new Configure();
if (configure.ParseArguments(args))
{
    configure.Save();
}

#if API
using ServerApi serverApi = new ServerApi(new Uri(configure.ApiDomain!));
#endif

CustomHttpProxyServerHandler handler = new CustomHttpProxyServerHandler();
using HttpProxyServer httpProxyServer = new HttpProxyServer(IPEndPoint.Parse(configure.ListenEndpoint!), handler);
httpProxyServer.StartListen(true);

var ip = GetIpAddress();
var ips = GetAllLocalIP(NetworkInterfaceType.Wireless80211, AddressFamily.InterNetwork)
    .Concat(GetAllLocalIP(NetworkInterfaceType.Wireless80211, AddressFamily.InterNetworkV6))
    .Concat(GetAllLocalIP(NetworkInterfaceType.Ethernet, AddressFamily.InterNetwork))
    .Concat(GetAllLocalIP(NetworkInterfaceType.Ethernet, AddressFamily.InterNetworkV6))
    .ToArray();

Console.WriteLine($"Address: {ip}");
Console.WriteLine("AddressList:\r\n" + string.Join("\r\n", ips.Select(x => "\t" + x.ToString())));
Console.WriteLine($"Listening port {httpProxyServer.IPEndPoint?.Port}");

while (true)
{
    #if API
    try
    {
        var pingResult = await serverApi.PingAsync(new ServerApi.PingRequest()
        {
            IP = ip,
            Port = httpProxyServer.IPEndPoint!.Port
        });

        handler.Update(pingResult);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex.GetType().FullName}: {ex.Message}, {ex.StackTrace}");
    }
#endif
    await Task.Delay(30000);
}


IEnumerable<string> GetAllLocalIP(NetworkInterfaceType type, AddressFamily addressFamily)
{
    foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()
        .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType == type))
    {
        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
        {
            if (ip.Address.AddressFamily == addressFamily)
            {
                yield return ip.Address.ToString();
            }
        }
    }
}
string? GetIpAddress()
{
    try
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            if (socket.LocalEndPoint is IPEndPoint iPEndPoint)
            {
                return iPEndPoint.Address.ToString();
            }
        }
    }
    catch
    {
    }
    return null;
}