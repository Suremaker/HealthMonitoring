using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using SimpleHttpMock;

namespace HealthMonitoring.AcceptanceTests.Helpers.Http
{
    public class MockWebEndpoint : IDisposable
    {
        private MockedHttpServer _server;
        public string Address { get; }

        public string StatusAddress => Address + "status";

        public MockWebEndpoint(int port)
        {
            Address = $"http://localhost:{port}/";
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
            var args = $"http add urlacl url=http://+:{port}/ user={windowsIdentity.Name}";
            var startInfo = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Unable to make namespace reservation for port {port}. Exit code:{process.ExitCode}");
        }

        public void Dispose()
        {
            if (_server == null)
                return;
            _server.Dispose();
            _server = null;
        }

        public void Reconfigure(Action<MockedHttpServerBuilder> configure)
        {
            var builder = new MockedHttpServerBuilder();
            configure(builder);
            builder.Reconfigure(_server, true);
        }
    }
}