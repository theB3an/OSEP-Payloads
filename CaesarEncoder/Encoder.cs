using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CaesarEncoder
{
    internal class Encoder
    {
        static void Main(string[] args)
        {
            string filePath = null;
            int cipherKey = 0;

            // Parse command line args
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--file" && i + 1 < args.Length)
                {
                    filePath = args[i + 1];
                    i++;
                }
                else if (args[i] == "--cipher" && i + 1 < args.Length)
                {
                    if (!int.TryParse(args[i + 1], out cipherKey))
                    {
                        Console.WriteLine("Invalid cipher key.");
                        return;
                    }
                    i++;
                }
            }

            if (filePath == null || !File.Exists(filePath))
            {
                Console.WriteLine("Usage: --file <path> --cipher <int>");
                return;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // Extract content inside { ... }
                Match match = Regex.Match(content, @"\{([^}]*)\}");
                if (!match.Success)
                {
                    Console.WriteLine("Failed to extract byte array from file.");
                    return;
                }

                string[] byteStrings = match.Groups[1].Value
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                byte[] buf = byteStrings
                    .Select(b => Convert.ToByte(b, 16))
                    .ToArray();

                // Encode with Caesar cipher
                byte[] encoded = new byte[buf.Length];
                for (int i = 0; i < buf.Length; i++)
                {
                    encoded[i] = (byte)(((uint)buf[i] + cipherKey) & 0xFF);
                }

                // Output encoded hex
                StringBuilder hex = new StringBuilder(encoded.Length * 5);
                foreach (byte b in encoded)
                {
                    hex.AppendFormat("0x{0:x2},", b);
                }

                Console.WriteLine("Encoded Hex: ");
                Console.WriteLine("byte[] buf = new byte[" + encoded.Length + "] {" + hex + "};");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
