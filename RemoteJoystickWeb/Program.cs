using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Web;

namespace RemoteJoystickWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            var localAddress = GetLocalIP();
            args = DefaultArgs(args, "example", "http://{0}:8000/".FormatSelf(localAddress), "8000", "8001");
            string layout = args[0];
            string pageAddress = args[1];
            int httpPort = int.Parse(args[2]);
            int socketPort = int.Parse(args[3]);
            ColorfulWriteLine("Client page : {0}\nLayout : {1}\nHTTP Port : {2}\nSocket Port : {3}"
                .FormatSelf(pageAddress, layout, httpPort, socketPort), ConsoleColor.Yellow);
            

            // joystick
            JoystickEmulator joystick = new JoystickEmulator(1);
            KeyboardEmulator keyboard = new KeyboardEmulator();

            // http server
            if (httpPort > 0)
            {
                HttpServer httpServer = new WebPageServer(httpPort);
                Thread thread = new Thread(new ThreadStart(httpServer.Listen));
                thread.Start();
            }

            // Socket server
            SocketServer socketServer = new SocketServer(socketPort);
            socketServer.Start(binary =>
            {
                var floats = binary.ShortToFloatArray(0, 8);
                joystick.SetAxis("x", floats[0]);
                joystick.SetAxis("y", floats[1]);
                joystick.SetAxis("z", floats[2]);
                joystick.SetAxis("rx", floats[3]);
                joystick.SetAxis("ry", floats[4]);
                joystick.SetAxis("rz", floats[5]);
                joystick.SetAxis("sl0", floats[6]);
                joystick.SetAxis("sl1", floats[7]);
                for (ushort shift = 0; shift < 2; shift++)
                {
                    for (ushort i = 0; i < 8; i++)
                    {
                        joystick.SetButton(shift * 8u + i + 1u, binary[floats.Length * 2 + shift].GetBit(i));
                    }
                }
            }, msg=>
            {
                var arr = msg.Split(' ');
                if (arr.Length == 2)
                {
                    if (arr[0] == "d") keyboard.KeyDown(arr[1]);
                    else if (arr[0] == "u") keyboard.KeyUp(arr[1]);
                }
            });

            // QR code
            var mobileAddress = string.Format("{0}?s={1}:{2}&l={3}", pageAddress, localAddress, socketPort, HttpUtility.UrlEncode(GetLayout(layout)));
            var QRImage = QRUtils.GenerateImage(mobileAddress, 6);
            var tempPath = Path.Combine(Environment.CurrentDirectory, "qr.png");
            QRImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            QRImage.Dispose();
            System.Diagnostics.Process.Start(tempPath);
            ColorfulWriteLine("1. Open port {0} and {1} in the firewall.".FormatSelf(socketPort, httpPort), ConsoleColor.Yellow);
            ColorfulWriteLine("2. Make sure your device and PC are in the same LAN.", ConsoleColor.Yellow);
            ColorfulWriteLine("3. Scan the QR code(qr.png) with your device.", ConsoleColor.Yellow);
            if (pageAddress.StartsWith("https:"))
            {
                ColorfulWriteLine("4. If you are using android chrome, goto chrome://flags and find \"insecure origins treated as secure\", add ws://{0}:{1}/".FormatSelf(localAddress, socketPort), ConsoleColor.Yellow);
            }
            ColorfulWriteLine("You might also want to disable auto-rotate and keep a portrait orientation because the UI was designed to fit vertically.", ConsoleColor.Yellow);
            Console.WriteLine(mobileAddress);

            Console.WriteLine("If you see \"Socket Opened\" below, it means a client has successfully connected to this server.");


            Console.ReadLine();
        }

        static string GetIPViaIPConfig()
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();

                p.StandardInput.WriteLine("chcp 437 \n ipconfig &exit");

                p.StandardInput.AutoFlush = true;

                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();
                p.Close();

                var arr = output.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    if (arr[i].Contains("LAN:"))
                    {
                        var lines = arr[i + 1].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        for (int j = 0; j < arr.Length; j++)
                        {
                            if (lines[j].Contains("IPv4"))
                            {
                                ColorfulWriteLine(lines[j], ConsoleColor.Cyan);
                                return lines[j].Split(new string[] { " : " }, StringSplitOptions.None)[1];
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }

            return null;
        }

        static string GetIPViaDns()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            for (int i = addresses.Length - 1; i > 0; i--)
            {
                var IPstr = addresses[i].ToString();
                if (IPstr.Contains("."))
                {
                    return IPstr;
                }
            }
            return null;
        }

        static string GetLocalIP()
        {
            string res = GetIPViaIPConfig();
            if (res == null)
            {
                ColorfulWriteLine("Ipconfig failed. Trying dns.", ConsoleColor.Red);
                res = GetIPViaDns();
            }
            if (res == null)
            {
                ColorfulWriteLine("Dns Failed. Can not get local ip address.", ConsoleColor.Red);
            }
            return res;
        }

        static string GetLayout(string fname)
        {
            fname = Path.ChangeExtension(fname, "txt");
            var layoutFolder = Path.Combine(Environment.CurrentDirectory, "layout");
            if (!Directory.Exists(layoutFolder)) Directory.CreateDirectory(layoutFolder);
            var fpath = Path.Combine(layoutFolder, fname);
            var reader = new StreamReader(fpath);
            var line = reader.ReadLine();
            reader.Close();
            return line;
        }

        static void ColorfulWriteLine(object message, ConsoleColor color)
        {
            var tempColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = tempColor;
        }

        static string[] DefaultArgs(string[] args, params string[] defaultValues)
        {
            var res = new string[defaultValues.Length];
            for (int i = 0; i < res.Length; i++)
            {
                if (i < args.Length && args[i] != "/") {
                    res[i] = args[i];
                }
                else
                {
                    res[i] = defaultValues[i];
                }
            }
            return res;
        }

    }
}
