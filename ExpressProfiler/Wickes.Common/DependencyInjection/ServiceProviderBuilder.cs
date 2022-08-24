using System;
using System.Collections.Generic;

namespace Wickes.DependencyInjection
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
            return new WickesServiceProvider(_registrations);
        }
    }
}
