using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace SendReceive
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .Build();
        }
    }
}
