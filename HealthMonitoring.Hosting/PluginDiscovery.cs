using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;

namespace HealthMonitoring.Hosting
{
    public static class PluginDiscovery<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PluginDiscovery<T>));

        public static IEnumerable<T> DiscoverAllInCurrentFolder(string searchPattern)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var location = (entryAssembly != null) ? Path.GetDirectoryName(entryAssembly.Location) : ".";
            var assemblies = Directory.EnumerateFiles(location, searchPattern, SearchOption.AllDirectories).ToArray();
            return DiscoverAll(assemblies);
        }

        public static T[] DiscoverAll(params string[] assemblyNames)
        {
            return assemblyNames
                .Select(LoadAssembly)
                .Distinct()
                .Where(a => a != null)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(T).IsAssignableFrom(t))
                .Distinct()
                .Select(CreateInstance)
                .Where(p => p != null)
                .Cast<T>()
                .ToArray();
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                Logger.InfoFormat("Instantiating plugins: {0}", type);
                return Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Unable to instantiate plugin: {0}\n{1}", type, e);
                return null;
            }
        }

        private static Assembly LoadAssembly(string path)
        {
            try
            {
                Logger.InfoFormat("Loading plugins from: {0}", path);
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
