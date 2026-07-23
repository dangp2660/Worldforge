using System;
using System.Collections.Generic;
using System.Reflection;

namespace Worldforge.Core.Bootstrap
{
    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient
    }

    public interface IServiceResolver
    {
        object Resolve(Type serviceType);

        bool TryResolve(Type serviceType, out object service);

        T Resolve<T>();

        bool TryResolve<T>(out T service);

        ServiceScope CreateScope();
    }

    public interface IServiceRegistry
    {
        void Add(Type serviceType, Func<IServiceResolver, object> factory, ServiceLifetime lifetime);

        void AddSingleton<TService>(Func<IServiceResolver, TService> factory);

        void AddSingleton<TService>(TService instance);

        void AddScoped<TService>(Func<IServiceResolver, TService> factory);

        void AddTransient<TService>(Func<IServiceResolver, TService> factory);
    }

    public interface IServiceRegistrationProvider
    {
        int Order { get; }

        void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services);
    }

    internal sealed class ServiceDescriptor
    {
        public ServiceDescriptor(Type serviceType, Func<IServiceResolver, object> factory, ServiceLifetime lifetime)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;
        }

        public Type ServiceType { get; }

        public Func<IServiceResolver, object> Factory { get; }

        public ServiceLifetime Lifetime { get; }
    }

    internal sealed class ServiceCollection : IServiceRegistry
    {
        private readonly Dictionary<Type, ServiceDescriptor> descriptors = new Dictionary<Type, ServiceDescriptor>();

        public int Count
        {
            get { return descriptors.Count; }
        }

        public IEnumerable<ServiceDescriptor> Descriptors
        {
            get { return descriptors.Values; }
        }

        public void Add(Type serviceType, Func<IServiceResolver, object> factory, ServiceLifetime lifetime)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (descriptors.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.FullName}' has already been registered.");
            }

            descriptors.Add(serviceType, new ServiceDescriptor(serviceType, factory, lifetime));
        }

        public void AddSingleton<TService>(Func<IServiceResolver, TService> factory)
        {
            Add(typeof(TService), resolver => factory(resolver), ServiceLifetime.Singleton);
        }

        public void AddSingleton<TService>(TService instance)
        {
            AddSingleton<TService>(_ => instance);
        }

        public void AddScoped<TService>(Func<IServiceResolver, TService> factory)
        {
            Add(typeof(TService), resolver => factory(resolver), ServiceLifetime.Scoped);
        }

        public void AddTransient<TService>(Func<IServiceResolver, TService> factory)
        {
            Add(typeof(TService), resolver => factory(resolver), ServiceLifetime.Transient);
        }
    }

    internal sealed class ServiceContainer : IServiceResolver, IDisposable
    {
        private readonly Dictionary<Type, ServiceDescriptor> descriptors;
        private readonly Dictionary<Type, object> singletonInstances = new Dictionary<Type, object>();
        private readonly ServiceScope rootScope;

        private bool disposed;

        public ServiceContainer(IEnumerable<ServiceDescriptor> descriptors)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            this.descriptors = new Dictionary<Type, ServiceDescriptor>();

            foreach (var descriptor in descriptors)
            {
                this.descriptors[descriptor.ServiceType] = descriptor;
            }

            rootScope = new ServiceScope(this);
        }

        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();
            return Resolve(serviceType, rootScope);
        }

        public bool TryResolve(Type serviceType, out object service)
        {
            ThrowIfDisposed();
            return TryResolve(serviceType, rootScope, out service);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public bool TryResolve<T>(out T service)
        {
            if (TryResolve(typeof(T), out var resolved))
            {
                service = (T)resolved;
                return true;
            }

            service = default;
            return false;
        }

        public ServiceScope CreateScope()
        {
            ThrowIfDisposed();
            return new ServiceScope(this);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            rootScope.Dispose();

            foreach (var instance in singletonInstances.Values)
            {
                DisposeInstance(instance);
            }

            singletonInstances.Clear();
            disposed = true;
        }

        internal object Resolve(Type serviceType, ServiceScope scope)
        {
            if (!TryResolve(serviceType, scope, out var service))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.FullName}' is not registered.");
            }

            return service;
        }

        internal bool TryResolve(Type serviceType, ServiceScope scope, out object service)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (serviceType == typeof(IServiceResolver))
            {
                service = scope;
                return true;
            }

            if (serviceType == typeof(ServiceContainer))
            {
                service = this;
                return true;
            }

            if (serviceType == typeof(ServiceScope))
            {
                service = scope;
                return true;
            }

            if (!descriptors.TryGetValue(serviceType, out var descriptor))
            {
                service = null;
                return false;
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    service = GetOrCreateSingleton(descriptor, scope);
                    return true;
                case ServiceLifetime.Scoped:
                    service = scope.GetOrCreateScoped(descriptor);
                    return true;
                default:
                    service = CreateInstance(descriptor, scope);
                    return true;
            }
        }

        internal object CreateInstance(ServiceDescriptor descriptor, ServiceScope scope)
        {
            var instance = descriptor.Factory(scope);
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Service factory for '{descriptor.ServiceType.FullName}' returned null.");
            }

            return instance;
        }

        private object GetOrCreateSingleton(ServiceDescriptor descriptor, ServiceScope scope)
        {
            if (singletonInstances.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(descriptor, scope);
            singletonInstances[descriptor.ServiceType] = instance;
            return instance;
        }

        private void DisposeInstance(object instance)
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ServiceContainer));
            }
        }
    }

    public sealed class ServiceScope : IServiceResolver, IDisposable
    {
        private readonly ServiceContainer container;
        private readonly Dictionary<Type, object> scopedInstances = new Dictionary<Type, object>();

        private bool disposed;

        internal ServiceScope(ServiceContainer container)
        {
            this.container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();
            return container.Resolve(serviceType, this);
        }

        public bool TryResolve(Type serviceType, out object service)
        {
            ThrowIfDisposed();
            return container.TryResolve(serviceType, this, out service);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public bool TryResolve<T>(out T service)
        {
            if (TryResolve(typeof(T), out var resolved))
            {
                service = (T)resolved;
                return true;
            }

            service = default;
            return false;
        }

        public ServiceScope CreateScope()
        {
            ThrowIfDisposed();
            return container.CreateScope();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            foreach (var instance in scopedInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            scopedInstances.Clear();
            disposed = true;
        }

        internal object GetOrCreateScoped(ServiceDescriptor descriptor)
        {
            if (scopedInstances.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = container.CreateInstance(descriptor, this);
            scopedInstances[descriptor.ServiceType] = instance;
            return instance;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ServiceScope));
            }
        }
    }

    internal static class ServiceRegistrationDiscovery
    {
        public static IReadOnlyList<IServiceRegistrationProvider> DiscoverProviders()
        {
            var providers = new List<IServiceRegistrationProvider>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly == null || assembly.IsDynamic)
                {
                    continue;
                }

                var types = GetLoadableTypes(assembly);
                for (var i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    if (type == null ||
                        type.IsAbstract ||
                        type.IsInterface ||
                        type.ContainsGenericParameters ||
                        !typeof(IServiceRegistrationProvider).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null) == null)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type, true) is IServiceRegistrationProvider provider)
                    {
                        providers.Add(provider);
                    }
                }
            }

            providers.Sort(CompareProviders);
            return providers;
        }

        private static int CompareProviders(IServiceRegistrationProvider left, IServiceRegistrationProvider right)
        {
            var orderComparison = left.Order.CompareTo(right.Order);
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            var leftName = left.GetType().FullName ?? left.GetType().Name;
            var rightName = right.GetType().FullName ?? right.GetType().Name;
            return StringComparer.Ordinal.Compare(leftName, rightName);
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return Array.FindAll(exception.Types, type => type != null);
            }
        }
    }
}
