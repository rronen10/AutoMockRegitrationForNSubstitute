using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using NSubstitute;

namespace UnitTestExtention.Unity
{
    public static class MockContainerExtention
    {
        private static List<KeyValuePair<Type, Object>> _mockInstances;
        public static void RegisterSubstitutes(this IUnityContainer container)
        {
            if (_mockInstances == null)
            {
                _mockInstances = new List<KeyValuePair<Type, object>>();
                var allInterfaces = GetAllInterfaces().ToArray();
                foreach (var interfaceToMock in allInterfaces)
                {
                    var mockInstance = Substitute.For(new[] { interfaceToMock }, null);
                    container.RegisterInstance(interfaceToMock, mockInstance);
                    _mockInstances.Add(new KeyValuePair<Type, object>(interfaceToMock, mockInstance));
                }
            }
            else
            {
                foreach (var mockInstance in _mockInstances)
                {
                    container.RegisterInstance(mockInstance.Key, mockInstance.Value);
                }
            }
        }

        private static IEnumerable<Type> GetAllInterfaces()
        {
            var allAssemblies = new Dictionary<string, Assembly>();
            FillAllAssemblies(Assembly.GetExecutingAssembly(), allAssemblies);
            foreach (var assembly in allAssemblies.Values)
            {
                foreach (var type in assembly.GetTypes().Where(x => x.IsInterface && !x.IsGenericType && x.IsPublic))
                {
                    yield return type;
                }
            }
        }

        private static void FillAllAssemblies(Assembly rootAssembly, Dictionary<string, Assembly> allAssemblies)
        {
            var referencedAssemblies = rootAssembly.GetReferencedAssemblies();
            foreach (var refAssemblyName in referencedAssemblies)
            {
                if (!refAssemblyName.Name.StartsWith("mscorlib")
                    && !refAssemblyName.Name.StartsWith("Microsoft")
                    && !refAssemblyName.Name.StartsWith("System")
                    && !refAssemblyName.Name.StartsWith("Stylet")
                    && !refAssemblyName.Name.StartsWith("WindowsBase")
                    && !refAssemblyName.Name.StartsWith("NSubstitute"))
                {
                    if (!allAssemblies.ContainsKey(refAssemblyName.Name))
                    {
                        var refAssembly = Assembly.Load(refAssemblyName);
                        allAssemblies.Add(refAssemblyName.Name, refAssembly);
                        FillAllAssemblies(refAssembly, allAssemblies);
                    }
                }
            }
        }
    }
}