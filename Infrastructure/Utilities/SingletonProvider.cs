using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class SingletonProvider<T> where T : class,new()
    {
        private static volatile T instance;
        private static object singletonLock = new object(); 
        private static bool initializing = false;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    bool flag = Monitor.TryEnter(singletonLock,30000);
                    try
                    {
                        if (instance == null)
                        {
                            bool flag2 = false;
                            try
                            {
                                if (initializing)
                                {
                                    throw new Exception(typeof(T).Name + " is being called during initialization.");
                                }
                                initializing = true;
                                flag2 = true;
                                instance = new T();
                            }
                            finally
                            {
                                if (flag2)
                                {
                                    initializing = false;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            Monitor.Exit(singletonLock);
                        }
                    }
                }
                return instance;    
            }
        }
        protected SingletonProvider()
        {

        }
    }
}
