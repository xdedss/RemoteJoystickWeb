using Gma.QrCodeNet.Encoding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteJoystickWeb
{
    class QRUtils
    {

        public static void PrintQR(string msg)
        {
            var qr = new QrEncoder(ErrorCorrectionLevel.M);
            var qrCode = qr.Encode(msg);

            Console.WriteLine();
            Console.Write("██");
            Console.Write("██");
            for (int i = 0; i < qrCode.Matrix.Width; i++)
            {
                Console.Write("█");
            }
            Console.WriteLine();
            for (int j = 0; j < qrCode.Matrix.Height; j++)
            {
                Console.Write("██");
                for (int i = 0; i < qrCode.Matrix.Width; i++)
                {
                    if (qrCode.Matrix[i, j])
                    {
                        Console.Write("▏");//▏
                    }
                    else
                    {
                        Console.Write("█");
                    }

                }
                Console.Write("██");
                Console.WriteLine();
            }
            Console.Write("██");
            Console.Write("██");
            for (int i = 0; i < qrCode.Matrix.Width; i++)
            {
                Console.Write("█");
            }
            Console.WriteLine();
        }

        public static Bitmap GenerateImage(string msg, int scale = 4, int padding = 10)
        {
            var qr = new QrEncoder(ErrorCorrectionLevel.M);
            var matrix = qr.Encode(msg).Matrix;
            
            Bitmap image = new Bitmap(matrix.Width * scale + padding, matrix.Height * scale + padding);//初始化大小
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
                for (int j = 0; j < matrix.Height; j++)
                {
                    for (int i = 0; i < matrix.Width; i++)
                    {
                        if (matrix[i, j])
                        {
                            g.FillRectangle(Brushes.Black, new Rectangle(padding + scale * i, padding + scale * j, scale, scale));
                        }
                    }
                }
            }
            return image;
        }
    }
}
