using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsumeAPI.Utilities
{
    public delegate object ServiceProvider();
    public class ServiceLocator
    {
        protected static readonly IDictionary<Type, ServiceProvider> Providers = new Dictionary<Type, ServiceProvider>();
        protected static readonly object sync = new object();
        protected static bool isPostRegistrationDone = false; 
        public static void Register<T>(ServiceProvider provider)
        {
            Providers[typeof(T)] = provider;
        }

        public static void Register<T>(object instance)
        {
            Providers[typeof(T)] = () => instance;
        }

        public static void Register<T,C>()
        {
            Providers[typeof(T)] = DefaultProvider<C>;
        }
        protected static object DefaultProvider<C>()
        {
            return Activator.CreateInstance<C>(); 
        }

        public static T Create<T>() 
        {
            Type typeFromHandle = typeof(T);
            return (T)Providers[typeFromHandle]();
        }

    }
}