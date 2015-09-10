using System;
using System.Configuration;
using System.Threading;
using HealthMonitoring.SelfHost.Configuration;
using Microsoft.Owin.Hosting;

namespace HealthMonitoring.SelfHost
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            string baseAddress = ConfigurationManager.AppSettings["BaseUrl"];

            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine("Started...");
                Thread.Sleep(Timeout.InfiniteTimeSpan);
            }
        }
    }
}
