using System;
using SqlServer.DependencyInjection;
using SqlServer.Logging;

namespace SqlServer.Configuration
{
    public class AppContainerBuilder : AppBuilderBase
    {
        public AppContainerBuilder(string appName)
            : base(appName)
        {

        }

        public virtual IServiceProvider Start()
        {
            //init logging
            ILogger logger = base.Initialize();

            var serviceProvider = BuildServiceProvider(logger);

            return serviceProvider;
        }

        protected virtual IServiceProvider BuildServiceProvider(ILogger logger)
        {
            try
            {
                var builder = new ServiceProviderBuilder();
                RegisterTypes(builder);

                var provider = builder.Build();
                logger.LogInformation("Build ServiceProvider '{ServiceProvider}'.", provider);

                return provider;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Can't build service provider.");
                throw new InvalidOperationException("Can't build service provider.", ex);
            }
        }

        protected virtual void RegisterTypes(IServiceProviderBuilder builder)
        {

        }
    }
}
