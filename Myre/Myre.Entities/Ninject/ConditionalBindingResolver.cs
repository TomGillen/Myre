using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Components;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Planning.Bindings;
using Ninject.Infrastructure;

namespace Myre.Entities.Ninject
{
    /// <summary>
    /// Resolves bindings that have been registered to object with a condition.
    /// </summary>
    public class ConditionalBindingResolver : NinjectComponent, IBindingResolver
    {
        /// <summary>
        /// Returns any bindings from the specified collection that match the specified service.
        /// </summary>
        /// <param name="bindings">The multimap of all registered bindings.</param>
        /// <param name="service">The service in question.</param>
        /// <returns>The series of matching bindings.</returns>
        public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, Type service)
        {
            foreach (IBinding binding in bindings[typeof(object)])
            {
                if (binding.Condition != null)
                    yield return binding;
            }
        }
    }
}
