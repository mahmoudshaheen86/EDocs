using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using edocs.Implementations.Repositories;
using edocs.Models;
using edocs.Repositories;
using edocs.Services;

namespace edocs.Infrastructure
{
    public class SimpleDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private readonly Dictionary<Type, Type> _registrations = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _singletonFactories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _perRequestInstances = new Dictionary<Type, object>();

        public SimpleDependencyResolver()
        {
            RegisterServices();
        }

        // =========================
        // MVC REQUIRED METHODS
        // =========================

        public object GetService(Type serviceType)
        {
            return GetServiceInternal(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service != null)
                yield return service;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        // =========================
        // REGISTRATION
        // =========================

        private void RegisterServices()
        {
            Register<ApplicationDbContext, ApplicationDbContext>();

            Register<IDocumentRepository, DocumentRepository>();
            Register<ICategoryRepository, CategoryRepository>();
            Register<IAttributeRepository, AttributeRepository>();
            Register<IAttributeListRepository, AttributeListRepository>();
            Register<IDocAttributeRepository, DocAttributeRepository>();
            Register<IFileRepository, FileRepository>();
            Register<IDocSentRepository, DocSentRepository>();
            Register<IUserCategoryRepository, UserCategoryRepository>();

            Register<IDocSentService, DocSentService>();
            Register<IDocumentService, DocumentService>();
            Register<ICategoryService, CategoryService>();
            Register<IAttributeService, AttributeService>();
            Register<IAttributeListService, AttributeListService>();
            Register<IDocAttributeService, DocAttributeService>();
            Register<IFileService, FileService>();
            Register<IUserCategoryService, UserCategoryService>();
        }

        private void Register<TInterface, TImplementation>()
            where TImplementation : class
        {
            _registrations[typeof(TInterface)] = typeof(TImplementation);
        }

        private void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, new()
        {
            _singletonFactories[typeof(TInterface)] = () => new TImplementation();
        }

        // =========================
        // CORE RESOLUTION
        // =========================

        private object GetServiceInternal(Type serviceType)
        {
            if (serviceType == null)
                return null;

            // Singleton
            if (_singletonFactories.ContainsKey(serviceType))
                return _singletonFactories[serviceType]();

            // Per-request cache
            if (_perRequestInstances.ContainsKey(serviceType))
                return _perRequestInstances[serviceType];

            // Registered mapping
            if (_registrations.ContainsKey(serviceType))
            {
                var implementationType = _registrations[serviceType];
                var instance = CreateInstance(implementationType);

                if (IsRequestScoped(implementationType))
                    _perRequestInstances[serviceType] = instance;

                return instance;
            }

            // Concrete type fallback
            if (!serviceType.IsInterface && !serviceType.IsAbstract)
                return CreateInstance(serviceType);

            return null;
        }

        // =========================
        // CREATION
        // =========================

        private object CreateInstance(Type implementationType)
        {
            // DbContext (per request)
            if (implementationType == typeof(ApplicationDbContext))
            {
                var key = "ApplicationDbContext";

                if (HttpContext.Current != null)
                {
                    var ctx = HttpContext.Current.Items[key] as ApplicationDbContext;

                    if (ctx != null)
                        return ctx;
                }

                var context = new ApplicationDbContext();

                if (HttpContext.Current != null)
                    HttpContext.Current.Items[key] = context;

                return context;
            }

            // Repositories
            if (implementationType == typeof(DocumentRepository))
                return new DocumentRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(CategoryRepository))
                return new CategoryRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(AttributeRepository))
                return new AttributeRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(AttributeListRepository))
                return new AttributeListRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(DocAttributeRepository))
                return new DocAttributeRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(FileRepository))
                return new FileRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(DocSentRepository))
                return new DocSentRepository(GetService<ApplicationDbContext>());

            if (implementationType == typeof(UserCategoryRepository))
                return new UserCategoryRepository(GetService<ApplicationDbContext>());

            // Services
            if (implementationType == typeof(DocumentService))
                return new DocumentService(
                    GetService<IDocumentRepository>(),
                    GetService<ICategoryRepository>(),
                    GetService<IAttributeRepository>(),
                    GetService<IDocAttributeRepository>(),
                    GetService<IFileRepository>(),
                    GetService<IUserCategoryRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(CategoryService))
                return new CategoryService(
                    GetService<ICategoryRepository>(),
                    GetService<IUserCategoryRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(AttributeService))
                return new AttributeService(
                    GetService<IAttributeRepository>(),
                    GetService<IAttributeListRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(AttributeListService))
                return new AttributeListService(
                    GetService<IAttributeListRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(DocAttributeService))
                return new DocAttributeService(
                    GetService<IDocAttributeRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(FileService))
                return new FileService(
                    GetService<IFileRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(DocSentService))
                return new DocSentService(
                    GetService<IDocSentRepository>(),
                    GetService<IUserCategoryRepository>(),
                    GetService<ApplicationDbContext>());

            if (implementationType == typeof(UserCategoryService))
                return new UserCategoryService(
                    GetService<IUserCategoryRepository>(),
                    GetService<ApplicationDbContext>());

            var constructor = implementationType.GetConstructors().First();
            var parameters = constructor.GetParameters()
                .Select(p => GetService(p.ParameterType))
                .ToArray();

            return Activator.CreateInstance(implementationType, parameters);
        }

        // =========================
        // SCOPING
        // =========================

        private bool IsRequestScoped(Type type)
        {
            return type == typeof(ApplicationDbContext)
                || type.Name.EndsWith("Repository")
                || type.Name.EndsWith("Service");
        }

        // =========================
        // CLEANUP
        // =========================

        public void ClearRequestInstances()
        {
            foreach (var obj in _perRequestInstances.Values)
            {
                var disposable = obj as IDisposable;

                if (disposable != null)
                {
                    try { disposable.Dispose(); }
                    catch { }
                }
            }

            _perRequestInstances.Clear();
        }
    }
}