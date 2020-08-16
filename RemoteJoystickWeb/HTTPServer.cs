using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteJoystickWeb
{
    public abstract class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void Listen()
        {
            listener = new TcpListener(IPAddress.Any ,port);
            Console.WriteLine("HTTP Server listening @ " + port);
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient(); //阻塞直到有访问
                HttpProcessor processor = new HttpProcessor(s, this); 
                Thread thread = new Thread(new ThreadStart(processor.process)); //开新线程处理
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);

        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);

    }
}

