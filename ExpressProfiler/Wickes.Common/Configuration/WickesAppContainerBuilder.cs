using System;
using Wickes.DependencyInjection;
using Wickes.Logging;

namespace Wickes.Configuration
{
    public class WickesAppContainerBuilder : WickesAppBuilderBase
    {
        public WickesAppContainerBuilder(string appName)
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
