using System;
using System.Linq;

namespace TestGpuProg
{
    static class Program
    {
        static void Main(string[] args)
        {
            int[] Primes = Enumerable.Range(2, 1000000).ToArray();

            Primes.OpenCLForEach(IsPrime);

            Console.WriteLine(string.Join(", ", Primes.Where(n => n != 0).Where(n => n != 0).Take(100)));
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
    " +
    //"printf(\" % d\", message[index]);" +
    @"

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
