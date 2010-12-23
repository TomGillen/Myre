using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using System.Reflection;
using Myre.Extensions;
using Myre.Entities.Services;
using Ninject.Activation;
using Ninject.Parameters;
using Myre.Entities.Events;
using Ninject.Planning.Targets;

namespace Myre.Entities.Ninject
{
    /// <summary>
    /// A ninject module which sets type bindings for services, in such a way that
    /// if the target has the [SceneService] attribute, ninject will attempt to retrieve the value from
    /// the scene.
    /// </summary>
    public class EntityNinjectModule
        : NinjectModule
    {
        private Dictionary<Type, Type> bindings;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNinjectModule"/> class.
        /// </summary>
        public EntityNinjectModule()
            : this(new Dictionary<Type, Type>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNinjectModule"/> class.
        /// </summary>
        /// <param name="bindings">Additional type bindings.</param>
        public EntityNinjectModule(IDictionary<Type, Type> bindings)
        {
            this.bindings = new Dictionary<Type,Type>(bindings)
            {
                { typeof(EventService),    typeof(EventService)   },
                { typeof(IEventService),   typeof(EventService)   },
                { typeof(ProcessService),  typeof(ProcessService) },
                { typeof(IProcessService), typeof(ProcessService) },
                { typeof(DrawingService),  typeof(DrawingService) }
            };
        }

        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            foreach (var binding in bindings)
            {
                var from = binding.Key;
                var to = binding.Value;

                Kernel.Bind(from).To(to);
                //Kernel.Bind(from).ToMethod(context => FindServiceInScene(context, to)).WhenTargetHas<SceneServiceAttribute>();
            }
            Bind<object>().ToMethod(FindServiceInScene).When(BindingService);
            //Bind<object>().ToMethod(FindProperty).When(BindingProperty);
            Bind<Scene>().ToMethod(FindScene).When(SceneParameterProvided);
        }

        private bool BindingService(IRequest request)
        {
            return request.Parameters.FirstOrDefault(p => p.Name.Equals("scene")) != null
                && request.Target != null
                && request.Target.GetCustomAttributes(typeof(SceneServiceAttribute), false).FirstOrDefault() != null
                && typeof(IService).IsAssignableFrom(request.Service);
        }

        private IService FindServiceInScene(IContext context)
        {
            var serviceType = context.Request.Service;

            IParameter sceneParameter = context.Parameters.FirstOrDefault(p => p.Name.ToLower() == "scene");
            IService service = null;
            Scene scene = sceneParameter.GetValue(context, null) as Scene;

            if (scene != null)
                service = scene.GetService(serviceType);

            return service;
        }

        /*
        private bool BindingProperty(IRequest request)
        {
            return request.Parameters.FirstOrDefault(p => p.Name.Equals("entity")) != null
                && typeof(IProperty).IsAssignableFrom(request.Service);
        }

        private IProperty FindProperty(IContext context)
        {
            var nameAttribute = context.Request.Target.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() as NameAttribute;
            var name = (nameAttribute != null) ? nameAttribute.Name : context.Request.Target.Name;
            return FindProperty(context, name);
        }

        
        private static IProperty FindProperty(IContext context, string propertyName)
        {
            var entityParameter = context.Parameters.First(p => p.Name.Equals("entity"));
            var entity = entityParameter.GetValue(context) as Entity;

            var property = entity.GetProperty(propertyName);

            if (property == null)
            {
                var service = context.Request.Service;
                var propertyData = new PropertyData()
                {
                    Name = propertyName,
                    DataType = service.ContainsGenericParameters ? service.GetGenericArguments()[0] : typeof(object),
                    Serialise = false
                };

                property = entity.AddProperty(context.Kernel, propertyData);
            }

            return property;
        }
        */

        private bool SceneParameterProvided(IRequest request)
        {
            foreach (var param in request.Parameters.Where(p => p.Name == "scene"))
            {
                var value = param.GetValue(request.ParentContext, null);
                if (value.GetType() == typeof(Scene))
                    return true;
            }

            return false;

            //return request.Parameters.Where(p => p.Name == "scene" && p.GetValue(request.ParentContext).GetType() == typeof(Scene)).FirstOrDefault() != null;
        }

        private Scene FindScene(IContext context)
        {
            foreach (var param in context.Parameters.Where(p => p.Name == "scene"))
            {
                var value = param.GetValue(context, null);
                if (value.GetType() == typeof(Scene))
                    return value as Scene;
            }

            return null;
        }
    }
}
