using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    class CredentialsProvider
    {
        public NameValueCollection AccessConfiguration { get; }

        public readonly Credentials PersonalCredentials = new Credentials(
            new Guid("6894b213-7da7-4569-94aa-d9047fa3fde1"),
            "1x8cm6vhtmooph12xfheqm8jtpfn68g1ukfm264tzs7svgekgsuk9i3u1uqscv84ml7");

        public Credentials AdminCredentials => 
            new Credentials(
                Guid.Parse(AccessConfiguration["AdminMonitorId"]),
                AccessConfiguration["AdminMonitorPrivateToken"]
            );

        public Credentials PullMonitorCredentials =>
            new Credentials(
                Guid.Parse(AccessConfiguration["PullMonitorId"]),
                AccessConfiguration["PullMonitorPrivateToken"]
            );

        public CredentialsProvider()
        {
            AccessConfiguration = (NameValueCollection)ConfigurationManager.GetSection("accessConfiguration");
        }

        public Credentials GenerateRandomCredentials()
        {
            return new Credentials(Guid.NewGuid(), PersonalCredentials.PrivateToken.ToUpper());
        }

        public string EncryptBase64String(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }
}
