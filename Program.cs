using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;

namespace MainHomework
{
    enum CryptMode { Encrypt, Decrypt, Unknown };
    class Program
    {
        static byte[] ReadEncrypted(string file, out long realsize) 
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                realsize = br.ReadInt64();
                byte[] data;
                data = new byte[fs.Length - sizeof(long)];
                br.Read(data, 0, data.Length);
                return data;
            }

        }

        static byte[] ReadDecrypted(string file)
        {
            byte[] data = null;
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                data = new byte[fs.Length];
                br.Read(data, 0, data.Length);
            }
            return data;
        }

        static void WriteEncrypted(string fname, byte[] data, long realsize)
        {
            using (var fo = new FileStream(fname, FileMode.Truncate))
            using (var br = new BinaryWriter(fo))
            {
                br.Write(realsize);
                br.Write(data);
            }
        }

        static void WriteDecrypted(string fname, byte[] data, long realsize)
        {
            using (var fo = new FileStream(fname, FileMode.Truncate))
            using (var br = new BinaryWriter(fo))
            {
                br.Write(data, 0, (int)realsize);
            }
        }

        static int Hex2Dec(char c)
        {
            c = Char.ToLower(c);
            if (c >= 'a' && c <= 'f')
                return 10 + (c - 'a');
            else if (c >= '0' && c <= '9')
                return c - '0';
            throw new Exception("Bad hex character");
        }

        static byte[] ParseKey(string key)
        {
            byte[] r = new byte[16];
            if (key.Length != r.Length * 2)
                return null;
            
            for (int i = 0; i < key.Length; i+=2)
            {
                r[i / 2] = (byte)((Hex2Dec(key[i]) << 4)  + Hex2Dec(key[i + 1]));
            }
            return r;
        }


        static string GetMd5Hash(byte[] input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(input);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        static void Main(string[] args)
        {
            byte[] key = null;
            string input = null;
            string output = null;
            CryptMode mode = CryptMode.Unknown;
            Stopwatch stopWatch = new Stopwatch();

            #region debugging
            if (args.Length == 0)
            {
                args = new string[5];
                args[1] = "-in";
                args[2] = "out2.txt";
                args[3] = "-out";
                args[4] = "out.txt";
            }
            #endregion debugging

            for (var argi = 0; argi < args.Length; argi++ )
            {

                if (args[argi] == "-help")
                {
                    Console.WriteLine("\n\n-in - входной файл\n-out - выходной файл\n-e - шифрование\n-d - дешифровка\n-key - ключ (ключ вводить слитно в 16-тиричной системе счисления, 32 символа");
                    Console.Write(" (пример: -key 01234567AaB12345cCd9012345678901))\n\n");
                    Environment.Exit(0);
                }
                else if (args[argi] == "-in")
                    input = args[++argi];
                else if (args[argi] == "-out")
                    output = args[++argi];
                else if (args[argi] == "-e")
                    mode = CryptMode.Encrypt;
                else if (args[argi] == "-d")
                    mode = CryptMode.Decrypt;
                else if (args[argi] == "-key")
                    key = ParseKey(args[++argi]);
            }
            #region debugging
            /*
            input = "in.txt";
            output = "out.txt";
            mode = CryptMode.Encrypt;
            key = ParseKey("01234567890123456789012345678912");
            
            input = "out.txt";
            output = "out2.txt";
            mode = CryptMode.Decrypt;
            key = ParseKey("01234567890123456789012345678912");
            */
            #endregion debugging

            if (key == null)
                throw new Exception("\ninvalid key size\n");

            byte[] data = null;
            long realsize = 0;
            if (mode == CryptMode.Encrypt)
            {
                data = ReadDecrypted(input);
                realsize = data.Length;
            }
            else if (mode == CryptMode.Decrypt)
                data = ReadEncrypted(input, out realsize);
            else throw new Exception("\nPass -e or -d\n");


            Console.WriteLine("\n\nMd5 hash: " + GetMd5Hash(data));
            Console.WriteLine("data.Length = " + data.Length + " realsizie = " + realsize);
                       
            ICryptoTransform idea = new IDEA(mode, key);
          
            stopWatch.Start();
            byte[] outputData = idea.TransformFinalBlock(data, 0, data.Length);            

            // write out
            if (mode == CryptMode.Encrypt)
                WriteEncrypted(output, outputData, realsize);
            else
                WriteDecrypted(output, outputData, realsize);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            Console.WriteLine("RunTime " + elapsedTime);
    
        }
    }
}
