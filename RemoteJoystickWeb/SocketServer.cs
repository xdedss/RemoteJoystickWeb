using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using System.Security.Cryptography.X509Certificates;

namespace RemoteJoystickWeb
{
    class SocketServer
    {
        public int port;
        public WebSocketServer server;

        public SocketServer(int port)
        {
            this.port = port;
        }

        public void Start(Action<byte[]> onBinary, Action<string> onMessage)
        {
            X509Certificate2 certificate = null;
            var certPath = Path.Combine(Environment.CurrentDirectory, "certificate.pfx");
            var passPath = Path.Combine(Environment.CurrentDirectory, "password.txt");
            if (File.Exists(certPath))
            {
                if (File.Exists(passPath))
                {
                    var reader = new StreamReader(passPath, Encoding.UTF8);
                    certificate = new X509Certificate2(certPath, reader.ReadToEnd());
                    reader.Close();
                }
                else certificate = new X509Certificate2(certPath);
            }

            Console.WriteLine("Socket Server listening @ " + port);
            FleckLog.Level = LogLevel.Warn;
            var allSockets = new List<IWebSocketConnection>();
            if (certificate == null)
            {
                server = new WebSocketServer("ws://0.0.0.0:" + port);
            }
            else // use wss if possible
            {
                server = new WebSocketServer("wss://0.0.0.0:" + port);
                server.Certificate = certificate;
            }
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Socket Opened : " + socket.ConnectionInfo.ClientIpAddress);
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Socket Closed : " + socket.ConnectionInfo.ClientIpAddress);
                    allSockets.Remove(socket);
                };
                socket.OnBinary = binary =>
                {
                    //binary.ToList().ForEach(b => Console.Write(b + ", "));
                    //Console.WriteLine();
                    onBinary(binary);
                };
                socket.OnMessage = message =>
                {
                    //Console.WriteLine(message);
                    //allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                    onMessage(message);
                };
            });
        }
    }
}
