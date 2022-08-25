using System;
using System.Collections.Generic;

namespace SqlServer.DependencyInjection
{
    public class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();

        public void Register(Type contract, Func<object> factory)
        {
            _registrations[contract] = factory;
        }

        public IServiceProvider Build()
        {
            return new ServiceProvider(_registrations);
        }
    }
}
