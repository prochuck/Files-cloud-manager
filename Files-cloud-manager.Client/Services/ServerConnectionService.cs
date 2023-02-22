using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Files_cloud_manager.Client.Services
{
    internal class ServerConnectionService
    {
        //todo переделать
        private Uri _baseAddress = new Uri("https://localhost:7216");
        private CookieContainer _cookieContainer=new CookieContainer();
        private string _login;
        private string _password;





        public ServerConnectionService()
        {
        }

        public void Login()
        {
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer=_cookieContainer})
            using (HttpClient client = new HttpClient(handler) { BaseAddress = _baseAddress })
            {

              var c=  client.PostAsync(CreateQuery($"/Authentication/Login", new Dictionary<string, string>()
                {
                    {"login","admin" },
                    {"password","123" }
                }), null).Result;
            }
        }

        private string CreateQuery(string requestUri, Dictionary<string, string> keysValuesForQuery)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in keysValuesForQuery)
            {
                query[item.Key] = item.Value;
            }
            return $"{requestUri}?{query.ToString()}";
        }
    }
}
