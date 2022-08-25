using System;

namespace Express.DependencyInjection
{
    public interface IServiceProviderBuilder
    {
        void Register(Type contract, Func<object> factory);
        //void Register(Type contract, Type implementation);

        IServiceProvider Build();
    }

    public static class IServiceProviderBuilderExt
    {
        public static void Register<TBase, TImplementation>(this IServiceProviderBuilder builder)
            where TImplementation : TBase, new()
        {
            builder.Register(typeof(TBase), () => new TImplementation());
        }
    }
}
