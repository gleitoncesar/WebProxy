using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace WebProxy
{
    internal class AllHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/html";
            //context.Response.Write("url: " + context.Request.RawUrl);

            var newReq = CreateRequest(context.Request, Config.TargetSite);

            var res = newReq.GetResponse() as HttpWebResponse;

            context.Response.ContentType = res.ContentType;
            foreach (Cookie c in res.Cookies)
                context.Response.Cookies.Add(new HttpCookie(c.Name, c.Value) { Expires = c.Expires, Secure = c.Secure, Path = c.Path });

            foreach (var headerKey in res.Headers.AllKeys)
                context.Response.Headers.Add(headerKey, res.Headers[headerKey]);

            byte[] buff = new byte[0x1000];


            var ms = new MemoryStream();

            //ms.Write(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 0, 10);

            int cnt = 0;
            using (var resStream = res.GetResponseStream())
            {
                while ((cnt = resStream.Read(buff, 0, buff.Length)) > 0)
                    ms.Write(buff, 0, cnt);
            }

            var base64 = Convert.ToBase64String(ms.ToArray());
            var bytes = Encoding.UTF8.GetBytes(base64);
            var ms2 = new MemoryStream(bytes);
            var bytes2 = Compress(ms2);
            var ms3 = new MemoryStream(bytes2);

            //ms.Position = 0;
            cnt = 0;

            while ((cnt = ms3.Read(buff, 0, buff.Length)) > 0)
                context.Response.OutputStream.Write(buff, 0, cnt);

        }

        private static byte[] Compress(Stream input)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                input.CopyTo(compressor);
                compressor.Close();
                return compressStream.ToArray();
            }
        }

        private HttpWebRequest CreateRequest(HttpRequest source, string targetSite)
        {
            string newUrl = source.Url.Scheme + "://" + targetSite + source.Url.PathAndQuery;
            HttpWebRequest request = HttpWebRequest.Create(newUrl) as HttpWebRequest;

            request.Accept = source.Headers["Accept"];
            request.KeepAlive = source.Headers["Connection"] == "keep-alive";

            request.CookieContainer = new CookieContainer();
            foreach (object c in source.Cookies)
            {
                if (c is string)
                    request.Headers.Add("Cookies", c as string);
                else
                    request.CookieContainer.Add(c as Cookie);
            }

            //request.KeepAlive = source.Headers["KeepAlive"];
            request.Method = source.HttpMethod;
            if (source.UrlReferrer != null)
                request.Referer = source.UrlReferrer.ToString();
            request.UserAgent = source.UserAgent;

            return request;
        }
    }
}