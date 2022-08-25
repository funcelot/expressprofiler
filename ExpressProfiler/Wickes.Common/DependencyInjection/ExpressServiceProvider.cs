using System;
using System.Collections.Generic;

namespace Express.DependencyInjection
{
    public class ExpressServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> _registrations;

        public ExpressServiceProvider(Dictionary<Type, Func<object>> registrations)
        {
            _registrations = registrations;
        }

        public object GetService(Type serviceType)
        {
            Func<object> createFunc;
            if (!_registrations.TryGetValue(serviceType, out createFunc))
            {
                throw new InvalidOperationException(string.Format("Not registered serviceType '{0}' for ServiceProvider.", serviceType.FullName));
            }

            return createFunc();
        }

    }
}
