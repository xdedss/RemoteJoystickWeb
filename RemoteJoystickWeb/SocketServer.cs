using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;

namespace RemoteJoystickWeb
{
    class SocketServer
    {
        public int port;
        public SocketServer(int port)
        {
            this.port = port;
        }

        public void Start(Action<byte[]> onBinary, Action<string> onMessage)
        {
            Console.WriteLine("Socket Server listening @ " + port);
            FleckLog.Level = LogLevel.Warn;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:" + port);
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
