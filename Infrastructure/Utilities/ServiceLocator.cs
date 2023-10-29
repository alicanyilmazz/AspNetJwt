using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public delegate object ServiceProvider();
    public class ServiceLocator
    {
        protected static readonly IDictionary<Type, ServiceProvider> Providers = new Dictionary<Type, ServiceProvider>();
        protected static readonly object sync = new object();
        protected static bool isPostRegistrationDone = false;
        
        public static bool PostDefaultServiceRegistered
        {
            get { return isPostRegistrationDone; }
            set { isPostRegistrationDone = value; }
        }
        public static void Register<T>(ServiceProvider provider)
        {
            Providers[typeof(T)] = provider;
        }

        public static void Register<T>(object instance)
        {
            Providers[typeof(T)] = () => instance;
        }

        public static void Register<T, C>()
        {
            Providers[typeof(T)] = DefaultProvider<C>;
        }
        protected static object DefaultProvider<C>()
        {
            return Activator.CreateInstance<C>();
        }
        public static void RegisterIfNotRegistered<T, C>()
        {
            if (!IsRegistered(typeof(T)))
            {
                Register<T,C>();
            }
        }
        public static void RegisterLazy<T, C>()
        {
            Register<T>(LazyProvider<T, C>);
        }
        public static object LazyProvider<T, C>()
        {
            object obj = Activator.CreateInstance<C>();
            Register<T>(obj);
            return obj;
        }
        public static T Create<T>()
        {
            Type typeFromHandle = typeof(T);
            if (!Providers.ContainsKey(typeFromHandle))
            {
                throw new Exception($"Unable to find service {typeFromHandle}");
            }
            return (T)Providers[typeFromHandle]();
        }

        public static bool IsRegistered(Type type)
        {
            return Providers.ContainsKey(type); 
        }
        public static ServiceProvider GetServiceProvider<T>()
        {
            return Providers[typeof(T)];
        }

        internal static void Clear()
        {
            Providers.Clear();
        }
    }
}

