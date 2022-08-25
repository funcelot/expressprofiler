using System;

namespace SqlServer.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }


        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            var service = provider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException(string.Format("Type {0} is not registered", serviceType));
            }

            return service;
        }

        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            return (T) provider.GetRequiredService(typeof(T));
        }


        public static IServiceScope CreateScope(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
