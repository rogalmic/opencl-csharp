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
            Console.ForegroundColor = ConsoleColor.White;
            var devices = ClooExtensions.GetDeviceNames();
            
            foreach (var dev in devices)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(dev.Trim());
                Console.ForegroundColor = ConsoleColor.Gray;
                int[] primes = Enumerable.Range(2, 1000000).ToArray();

                var sw = new Stopwatch();
                try
                {
                    sw.Start();
                    primes.ClooForEach(IsPrime, k => true, (i, d, v) => d == dev);
                    Console.WriteLine(string.Join(", ", primes.Where(n => n != 0).Take(100)) + ", ...");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    sw.Stop();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Press enter to quit...");
            Console.ReadKey();
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
    }
}
