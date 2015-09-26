using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;
using SimpleHttpMock;

namespace HealthMonitoring.AcceptanceTests.Helpers.Http
{
    class MockWebEndpoint : IDisposable
    {
        private MockedHttpServer _server;
        public string Address { get; private set; }

        public string StatusAddress { get { return Address + "status"; } }

        public MockWebEndpoint(int port)
        {
            Address = string.Format("http://localhost:{0}/", port);
            _server = StartServer(port);
        }

        private MockedHttpServer StartServer(int port)
        {
            try
            {
                return new MockedHttpServerBuilder().Build(Address);
            }
            catch (AggregateException e)
            {
                if (!e.InnerExceptions.Any(ex => ex is AddressAccessDeniedException))
                    throw;

                MakeNamespaceReservation(port);
                return new MockedHttpServerBuilder().Build(Address);
            }
        }

        private void MakeNamespaceReservation(int port)
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            var args = string.Format("http add urlacl url=http://+:{0}/ user={1}", port, windowsIdentity.Name);
            var startInfo = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.Format("Unable to make namespace reservation for port {0}. Exit code:{1}", port, process.ExitCode));
        }

        public void Dispose()
        {
            if (_server == null)
                return;
            _server.Dispose();
            _server = null;
        }

        public void SetupStatusResponse(HttpStatusCode code)
        {
            var builder = new MockedHttpServerBuilder();
            builder.WhenGet("/status").Respond(code);
            builder.Reconfigure(_server, true);
        }

        public void SetupStatusResponse(HttpStatusCode code, object model)
        {
            var builder = new MockedHttpServerBuilder();
            builder.WhenGet("/status").RespondContent(code, r => new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
            builder.Reconfigure(_server, true);
        }

        public void SetupStatusPlainResponse(HttpStatusCode code, string text)
        {
            var builder = new MockedHttpServerBuilder();
            builder.WhenGet("/status").RespondContent(code, r => new StringContent(text, Encoding.UTF8, "text/plain"));
            builder.Reconfigure(_server, true);
        }
    }
}