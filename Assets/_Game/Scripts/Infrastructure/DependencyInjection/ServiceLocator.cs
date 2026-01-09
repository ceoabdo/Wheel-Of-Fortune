using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Infrastructure.DependencyInjection
{
    public sealed class ServiceLocator
    {
        private const int DEFAULT_SERVICE_CAPACITY = 16;

        private static ServiceLocator _instance;

        private readonly Dictionary<Type, object> _services;

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }

                return _instance;
            }
        }

        private ServiceLocator()
        {
            _services = new Dictionary<Type, object>(DEFAULT_SERVICE_CAPACITY);
        }

        public void Register<TService>(TService service) where TService : class
        {
            Type serviceType = typeof(TService);
            _services[serviceType] = service;
        }

        public TService Get<TService>() where TService : class
        {
            Type serviceType = typeof(TService);

            if (_services.TryGetValue(serviceType, out object service))
            {
                return service as TService;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[ServiceLocator] Service not found: {serviceType.Name}. " +
                             "Ensure the service is registered before accessing it.");
#endif

            return null;
        }

        public void Unregister<TService>() where TService : class
        {
            Type serviceType = typeof(TService);
            _services.Remove(serviceType);
        }
    }
}
