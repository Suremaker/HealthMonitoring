using System;
using System.Configuration;
using System.Text;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    static class CredentialsProvider
    {
        public static readonly Credentials PersonalCredentials = new Credentials(
            new Guid("6894b213-7da7-4569-94aa-d9047fa3fde1"),
            "1x8cm6vhtmooph12xfheqm8jtpfn68g1ukfm264tzs7svgekgsuk9i3u1uqscv84ml7sn5gh85m0al7e7vvbtvp90vjy3jqg10or7yib531b5smtnm2iksb6gyclx3y3");

        public static Credentials AdminCredentials => 
            new Credentials(
                Guid.Parse(ConfigurationManager.AppSettings["AdminMonitorId"]), 
                ConfigurationManager.AppSettings["AdminMonitorPrivateToken"]
            );

        public static Credentials PullMonitorCredentials =>
            new Credentials(
                Guid.Parse(ConfigurationManager.AppSettings["PullMonitorId"]),
                ConfigurationManager.AppSettings["PullMonitorPrivateToken"]
            );

        public static Credentials RandomCredentials => new Credentials(Guid.NewGuid(), PersonalCredentials.PrivateToken.ToUpper());

        public static void UpdatePersonalId(Guid newId)
        {
            PersonalCredentials.MonitorId = newId;
        }

        public static string EncryptBase64String(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }

    class Credentials
    {
        public Guid MonitorId { get; set; }
        public string PrivateToken { get; set; }

        public Credentials(Guid id, string token)
        {
            MonitorId = id;
            PrivateToken = token;
        }
    }

}
