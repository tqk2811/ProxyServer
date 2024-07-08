using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    internal class Configure
    {
#if DEBUG
        [JsonIgnore]
#endif
        public string? ListenEndpoint { get; set; } = "0.0.0.0:0";
#if DEBUG
        [JsonIgnore]
#endif

#if API
        public string? ApiDomain { get; set; }
#if DEBUG
        = "https://localhost:7257/";
#else
        = "https://localhost:345/";
#endif
#endif



        static readonly string configurePath = Path.Combine(Directory.GetCurrentDirectory(), "Configure.json");
        public void Save()
        {
            File.WriteAllText(configurePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Configure? Load()
        {
            if (File.Exists(configurePath))
                return JsonConvert.DeserializeObject<Configure>(File.ReadAllText(configurePath));
            return null;
        }
        public bool ParseArguments(string[] args)
        {
            bool isChanged = false;
            if (args.Length >= 1 && IPEndPoint.TryParse(args[0], out IPEndPoint? endPoint))
            {
                this.ListenEndpoint = endPoint.ToString();
                isChanged = true;
            }
#if API
            if (args.Length >= 2 && Uri.TryCreate(args[1], UriKind.Absolute, out Uri? wsUri))
            {
                this.ApiDomain = wsUri.ToString();
                isChanged = true;
            }
#endif
            return isChanged;
        }
    }
}
