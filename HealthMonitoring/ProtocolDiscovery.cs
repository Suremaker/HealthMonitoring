using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using HealthMonitoring.Protocols;

namespace HealthMonitoring
{
    public static class ProtocolDiscovery
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProtocolDiscovery));

        public static IEnumerable<IHealthCheckProtocol> DiscoverAllInCurrentFolder()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var location = (entryAssembly != null) ? Path.GetDirectoryName(entryAssembly.Location) : ".";
            var assemblies = Directory.EnumerateFiles(location, "*.Protocols.*.dll", SearchOption.AllDirectories).ToArray();
            return DiscoverAll(assemblies);
        }

        public static IHealthCheckProtocol[] DiscoverAll(params string[] assemblyNames)
        {
            return assemblyNames
                .Select(LoadAssembly)
                .Distinct()
                .Where(a => a != null)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IHealthCheckProtocol).IsAssignableFrom(t))
                .Distinct()
                .Select(CreateInstance)
                .Where(p => p != null)
                .Cast<IHealthCheckProtocol>()
                .ToArray();
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                Logger.InfoFormat("Instantiating protocols: {0}", type);
                return Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Unable to instantiate protocol: {0}\n{1}", type, e);
                return null;
            }
        }

        private static Assembly LoadAssembly(string path)
        {
            try
            {
                Logger.InfoFormat("Loading protocols from: {0}", path);
                return Assembly.LoadFrom(path);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Unable to load assembly: {0}\n{1}", path, e);
                return null;
            }
        }
    }
}
