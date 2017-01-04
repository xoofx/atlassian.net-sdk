using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Locates services used by jira client.
    /// </summary>
    public class ServiceLocator
    {
        private readonly ConcurrentDictionary<Type, object> _factories;

        /// <summary>
        /// Creates a new instance of ServiceLocator.
        /// </summary>
        public ServiceLocator()
        {
            _factories = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="factory">Factory that creates the service instance.</param>
        public void Register<TService>(Func<TService> factory)
        {
            _factories.AddOrUpdate(typeof(TService), factory, (s, f) => factory);
        }

        /// <summary>
        /// Gets a service.
        /// </summary>
        public TService Get<TService>()
        {
            object factoryObj;
            if (_factories.TryGetValue(typeof(TService), out factoryObj))
            {
                return ((Func<TService>)factoryObj).Invoke();
            }
            else
            {
                throw new InvalidOperationException(String.Format("Service '{0}' not found.", typeof(TService)));
            }
        }

        /// <summary>
        /// Removes all registered services.
        /// </summary>
        public void Clear()
        {
            _factories.Clear();
        }
    }
}
