using System;
using System.Diagnostics;
using System.Linq;
using Cloo.Extensions;

namespace TestGpuProg
{
    static class Program
    {
        static void Main(string[] args)
        {
            var devices = ClooExtensions.GetDeviceNames();

            foreach (var dev in devices.Where(d => args.Length == 0 || args.Contains(d.Trim())))
            {
                WriteLine(dev.Trim(), ConsoleColor.Yellow);

                int[] primes = Enumerable.Range(2, 1000000).ToArray();

                var sw = new Stopwatch();
                try
                {
                    sw.Start();
                    primes.ClooForEach(IsPrime, k => true, (i, d, v) => d == dev);
                    WriteLine($"{string.Join(", ", primes.Where(n => n != 0).Take(100))}, ...", ConsoleColor.Gray);
                }
                catch (Exception ex)
                {
                    WriteLine($"Error: {ex.Message}", ConsoleColor.Red);
                }
                finally
                {
                    sw.Stop();
                    WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
                }

                WriteLine();
            }

            WriteLine("Press enter to quit...", ConsoleColor.White, true);
        }

        static string IsPrime
        {
            get
            {
                return
@"
kernel void GetIfPrime(global int* message)
{
    int index = get_global_id(0);
    int upperl=(int)sqrt((float)message[index]);
    for(int i=2;i<=upperl;i++)
    {
        if(message[index]%i==0)
        {
            message[index]=0;
            return;
        }
    }
}";
            }
        }

        static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White, bool wait = false)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);

            if (wait)
            {
                Console.ReadKey();
            }
        }
    }
}
