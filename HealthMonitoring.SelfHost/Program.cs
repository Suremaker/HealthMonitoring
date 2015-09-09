using System;
using HealthMonitoring.SelfHost.Configuration;
using Microsoft.Owin.Hosting;

namespace HealthMonitoring.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("Started...");
                Console.ReadLine(); 
            }


        }
    }
}
