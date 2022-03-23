using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TravianTools.Properties;

namespace TravianTools.TravianUtils
{
    public class Http
    {
        public string Post(string cookies, JObject json)
        {
            var req = (HttpWebRequest)WebRequest.Create(
                                                         $"https://{g.Settings.Server}.{g.Settings.Domain}/api/?c={(json as dynamic).controller}&a={(json as dynamic).action}&t{GetTimeStamp()}");
            var data = Rem(json.ToString());
            var buffer = Encoding.UTF8.GetBytes(data);

            req.Method      = "POST";
            req.Accept      = "application/json, text/plain, */*";
            req.ContentType = "application/json;charset=UTF-8";
            req.Host        = $"{g.Settings.Server}.{g.Settings.Domain}";
            req.Referer     = $"https://{g.Settings.Server}.{g.Settings.Domain}/";
            req.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.50";
            req.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            req.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            req.Headers.Add("Origin", $"https://{g.Settings.Server}.{g.Settings.Domain}");

            req.Headers.Add("Cookie", cookies);

            req.ContentLength = buffer.Length;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var reqStream = req.GetRequestStream();
            reqStream.Write(buffer, 0, data.Length);
            reqStream.Close();
            var resp = (HttpWebResponse)req.GetResponse();
            var strReader = new StreamReader(resp.GetResponseStream());
            var workingPage = strReader.ReadToEnd();
            resp.Close();
            return workingPage;
        }

        public string Rem(string str) => str.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        private string GetTimeStamp() => ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
    }
}
