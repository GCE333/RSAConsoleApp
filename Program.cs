using System.Text;
using System.Numerics;
using System.Globalization;
using System.Diagnostics;

namespace RSAConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RSAKeyComponents rsaKeys = new RSAKeyComponents();
            while (true)
            {
                string prompt = "Do you want to encrypt or decrypt? [E]ncrypt/[D]ecrypt/[C]ancel: ";
                char key = ConsolePrompt(prompt, new char[] { 'E', 'D', 'C' });
                if (key == 'C') break;
                bool isDecryption = key == 'D';

                prompt = "Do you want to use padding? If yes, then PKCS #1 v1.5 padding will be used [Y]es/[N]o: ";
                key = ConsolePrompt(prompt, new char[] { 'Y', 'N' });
                bool usePadding = key == 'Y';

                if (rsaKeys.IsEmpty || (rsaKeys.PublicOnly && isDecryption)) // Existing keys can't be used
                {
                    prompt = "Do you want to generate, import, or calculate the values of new RSA keys? [G]enerate/[I]mport/[C]alculate: ";
                    key = ConsolePrompt(prompt, new char[] { 'G', 'I', 'C' });
                }
                else
                {
                    prompt = "Do you want to generate, import, or calculate the values of new RSA keys or use existing keys? [G]enerate/[I]mport/[C]alculate/[E]xisting: ";
                    key = ConsolePrompt(prompt, new char[] { 'G', 'I', 'C', 'E' });
                }
                if (key == 'G') rsaKeys = RSAKeyGenerator.GenerateKeys(KeySizePrompt(usePadding));
                else if (key == 'I') rsaKeys = KeyImportPrompt(usePadding, isDecryption);
                else if (key == 'C')
                {
                    Console.WriteLine("Calculating the values of RSA keys is not implemented yet.");
                    continue;
                }

                if (isDecryption)
                {
                    prompt = "Choose the format of your ciphertext that you want to decrypt. [B]ase64/[H]ex/[N]umber: ";
                    key = ConsolePrompt(prompt, new char[] { 'B', 'H', 'N' });
                    SupportedFormats format = (SupportedFormats)key;
                    byte[] ciphertext = ParseToByteArray("Enter your ciphertext: ", format);
                    try
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        byte[] messageBytes = RSACryptography.Decrypt(rsaKeys, ciphertext, usePadding);
                        stopwatch.Stop();
                        Console.WriteLine("Decrypted in {0} ms", stopwatch.Elapsed.TotalMilliseconds);

                        prompt = "Choose output format of the decrypted message. [B]ase64/[H]ex/[T]ext/[N]umber: ";
                        key = ConsolePrompt(prompt, new char[] { 'B', 'H', 'T', 'N' });

                        string message = "";
                        if (key == 'B') message = Convert.ToBase64String(messageBytes);
                        else if (key == 'H') message = Convert.ToHexString(messageBytes);
                        else if (key == 'T') message = Encoding.UTF8.GetString(messageBytes);
                        else if (key == 'N') message = new BigInteger(messageBytes, true, true).ToString();
                        Console.WriteLine("Decrypted message: " + message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + ex.Message);
                    }
                }
                else
                {
                    prompt = "Choose the format of your message that you want to encrypt. [B]ase64/[H]ex/[T]ext/[N]umber: ";
                    key = ConsolePrompt(prompt, new char[] { 'B', 'H', 'T', 'N' });
                    SupportedFormats format = (SupportedFormats)key;
                    byte[] message = ParseToByteArray("Enter your message: ", format);
                    try
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        byte[] ciphertextBytes = RSACryptography.Encrypt(rsaKeys, message, usePadding);
                        stopwatch.Stop();
                        Console.WriteLine("Encrypted in {0} ms", stopwatch.Elapsed.TotalMilliseconds);

                        prompt = "Choose output format of the ciphertext. [B]ase64/[H]ex/[N]umber: ";
                        key = ConsolePrompt(prompt, new char[] { 'B', 'H', 'N' });

                        string ciphertext = "";
                        if (key == 'B') ciphertext = Convert.ToBase64String(ciphertextBytes);
                        else if (key == 'H') ciphertext = Convert.ToHexString(ciphertextBytes);
                        else if (key == 'N') ciphertext = new BigInteger(ciphertextBytes, true, true).ToString();
                        Console.WriteLine("Ciphertext: " + ciphertext);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + ex.Message);
                    }
                }
            }
        }

        static public char ConsolePrompt(string prompt, char[] keys)
        {
            char key;
            do
            {
                Console.Write(prompt);
                key = (Console.ReadLine() ?? "").ToUpper().FirstOrDefault();
            }
            while (!keys.Contains(key));
            return key;
        }

        static public int KeySizePrompt(bool usePadding)
        {
            int keySize;
            bool invalidKey = true;
            do
            {
                Console.Write("Enter key size (number of bits in the RSA modulus n): ");
                if (!int.TryParse(Console.ReadLine(), out keySize))
                {
                    Console.WriteLine("Unable to parse number. Please try again.");
                }
                else if (keySize <= 0 || keySize % 16 > 0)
                {
                    Console.WriteLine("Key size must be a positive number and a multiple of 16.");
                }
                else if (usePadding && keySize < 96)
                {
                    Console.WriteLine("Key size should be at least 96 (12 bytes) for PKCS #1 v1.5 padding to be used.");
                }
                else invalidKey = false;
            }
            while (invalidKey);
            return keySize;
        }

        static public RSAKeyComponents KeyImportPrompt(bool usePadding, bool isPrivate)
        {
            BigInteger n;
            BigInteger e;
            BigInteger d;
            bool isValid;
            string prompt = "Specify input format for your RSA keys. [H]ex/[N]umber: ";
            char key = ConsolePrompt(prompt, new char[] { 'H', 'N' });
            SupportedFormats format = (SupportedFormats)key;
            do
            {
                n = ParseToBigInteger("Enter RSA modulus n: ", format);
                isValid = n >= 10 || (usePadding && n.GetByteCount(true) > 11);
                if (!isValid) Console.WriteLine("Given value is too small. Please try again.");
            }
            while (isValid == false);

            do
            {
                e = ParseToBigInteger("RSA public exponent e: ", format);
                isValid = e > 1 && !e.IsEven;
                if (!isValid) Console.WriteLine("RSA exponent e must be an odd number greater than 1.");
            }
            while (isValid == false);

            if (!isPrivate) return new RSAKeyComponents(n, e);
            do
            {
                d = ParseToBigInteger("RSA private exponent d: ", format);
                isValid = d > 1 && !d.IsEven;
                if (!isValid) Console.WriteLine("RSA exponent d must be an odd number greater than 1.");
            }
            while (isValid == false);
            return new RSAKeyComponents(n, e, d);
        }

        static public BigInteger ParseToBigInteger(string prompt, SupportedFormats format)
        {
            var result = ParseValuePrompt(prompt, format);
            return (result.Item1 > 0) ? result.Item1 : new BigInteger(result.Item2, true, true);
        }

        static public byte[] ParseToByteArray(string prompt, SupportedFormats format)
        {
            var result = ParseValuePrompt(prompt, format);
            return (result.Item2.Length > 0) ? result.Item2 : result.Item1.ToByteArray(true, true);
        }

        static public (BigInteger, byte[]) ParseValuePrompt(string prompt, SupportedFormats format)
        {
            BigInteger intValue = new();
            byte[] byteValue = Array.Empty<byte>();
            bool isParsed;
            do
            {
                Console.Write(prompt);
                if (format == SupportedFormats.Base64)
                {
                    try
                    {
                        byteValue = Convert.FromBase64String(Console.ReadLine() ?? "");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (format == SupportedFormats.Hex)
                {
                    string s = (Console.ReadLine() ?? "").TrimStart(new char[] { '0', 'x', 'X' }).Insert(0, "00");
                    BigInteger.TryParse(s, NumberStyles.HexNumber, null, out intValue);
                }
                else if (format == SupportedFormats.Text)
                {
                    byteValue = Encoding.UTF8.GetBytes(Console.ReadLine() ?? "");
                }
                else if (format == SupportedFormats.Number)
                {
                    BigInteger.TryParse(Console.ReadLine(), out intValue);
                }
                isParsed = intValue > 0 || byteValue.Length > 0;
                if (!isParsed) Console.WriteLine("Unable to parse value. Please try again.");
            }
            while (isParsed == false);
            return (intValue, byteValue);
        }
    }
}