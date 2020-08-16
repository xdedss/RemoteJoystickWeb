using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;

namespace RemoteJoystickWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            args = DefaultArgs(args, "192.168.1.2:8000", "example", "8000", "8001");
            string pageAddress = args[0];
            string layout = args[1];
            int httpPort = int.Parse(args[2]);
            int socketPort = int.Parse(args[3]);
            ColorfulWriteLine("Client page : {0}\nLayout : {1}\nHTTP Port : {2}\nSocket Port : {3}"
                .FormatSelf(pageAddress, layout, httpPort, socketPort), ConsoleColor.Yellow);
            

            // joystick
            JoystickEmulator joystick = new JoystickEmulator(1);
            KeyboardEmulator keyboard = new KeyboardEmulator();

            // http server
            HttpServer httpServer = new WebPageServer(httpPort);
            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();

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
            var localAddress = GetLocalIP();
            var mobileAddress = string.Format("http://{0}/?s={1}:{2}&l={3}", pageAddress, localAddress, socketPort, GetLayout(layout));
            var QRImage = QRUtils.GenerateImage(mobileAddress, 6);
            var tempPath = Path.Combine(Environment.CurrentDirectory, "qr.png");
            QRImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            QRImage.Dispose();
            Console.WriteLine("Opening qr.png");
            System.Diagnostics.Process.Start(tempPath);
            ColorfulWriteLine("1. Open port {0} and {1} in the firewall.".FormatSelf(socketPort, httpPort), ConsoleColor.Yellow);
            ColorfulWriteLine("2. Make sure your device and PC are in the same LAN.", ConsoleColor.Yellow);
            ColorfulWriteLine("3. Scan the QR code with your device.", ConsoleColor.Yellow);
            Console.WriteLine(mobileAddress);

            //var t = 0.0;
            //float x;
            //while (true)
            //{
            //    Thread.Sleep(10);
            //    t += 0.01 * 5;
            //    x = (float)Math.Sin(t);
            //    //Console.WriteLine(x);
            //    joystick.SetAxis("x", x);
            //    joystick.SetButton(1, x > 0);
            //}


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
                p.Start();//启动程序

                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine("chcp 437 \n ipconfig &exit");

                p.StandardInput.AutoFlush = true;
                //p.StandardInput.WriteLine("exit");
                //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();//等待程序执行完退出进程
                p.Close();

                var arr = output.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    if (arr[i].Contains("LAN:"))
                    {
                        ColorfulWriteLine(arr[i + 1], ConsoleColor.Cyan);
                        var lines = arr[i + 1].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        for (int j = 0; j < arr.Length; j++)
                        {
                            if (lines[j].Contains("IPv4"))
                            {
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
                if (i < args.Length) {
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
