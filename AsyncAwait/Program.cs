using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwait
{
    class Program
    {
        interface IA
        {
            Task a();
        }

        class A : IA
        {
            public async Task a()
            {
                await Task.Delay(5000);
                Console.WriteLine("From task");
            }
        }


        static async void doA()
        {
            IA a = new A();
            a.a();
        }

        static void Main(string[] args)
        {
            doA();
            Console.WriteLine("Main complete");
            Console.ReadKey();
        }
    }
}
