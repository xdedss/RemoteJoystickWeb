using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteJoystickWeb
{
    public class WebPageServer : HttpServer
    {
        public string wwwroot;
        public Dictionary<string, string> urls = new Dictionary<string, string>
        {
            { "/" , "index.html" },
            { "/app.js" , "app.js" },
            { "/quaternion.min.js" , "quaternion.min.js" },
        };

        public WebPageServer(int port) : base(port)
        {
            wwwroot = Path.Combine(Environment.CurrentDirectory, "www");
        }

        public override void handleGETRequest(HttpProcessor p)
        {
            //Console.WriteLine("request: {0}", p.http_url);
            var pureURL = p.http_url.Split('?')[0].ToLower();
            if (urls.ContainsKey(pureURL))
            {
                p.writeSuccess();
                var streamReader = new StreamReader(Path.Combine(wwwroot, urls[pureURL]));
                int buf;
                while((buf = streamReader.Read()) != -1)
                {
                    p.outputStream.Write((char)buf);
                }
                streamReader.Close();
            }
            else
            {
                p.writeFailure();
            }
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            //Console.WriteLine("POST request: {0}", p.http_url);
            //string data = inputData.ReadToEnd();

            p.writeFailure();
            
        }
    }
}
