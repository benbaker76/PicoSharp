using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PicoSharp
{
    class Tools
    {
        public static bool IsEven(int number)
        {
            return number % 2 == 0;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] GetEmbeddedResourceAsBytes(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream($"PicoSharp.Resources.{resourceName}"))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Embedded resource '{resourceName}' not found.");
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public static string GetEmbeddedResourceAsString(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"PicoSharp.Resources.{resourceName}"))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"Embedded resource '{resourceName}' not found.");
                }
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
