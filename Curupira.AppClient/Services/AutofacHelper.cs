using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curupira.AppClient.Services
{
    public class AutofacHelper : IAutofacHelper
    {
        private readonly IComponentContext _context;

        public AutofacHelper(IComponentContext context)
        {
            _context = context;
        }

        public IList<(string Name, Type ImplementationType)> GetNamedImplementationsOfInterface<TInterface>()
        {
            // Retrieve the component registry
            var componentRegistry = _context.ComponentRegistry;

            // List to store the results
            var implementations = new List<(string Name, Type ImplementationType)>();

            // Loop through all registrations in the registry
            foreach (var registration in componentRegistry.Registrations)
            {
                // Find services registered as TInterface
                var services = registration.Services.OfType<KeyedService>().Where(s => s.ServiceType == typeof(TInterface));

                // For each service, get the name and implementation type
                foreach (var service in services)
                {
                    var implementationType = registration.Activator.LimitType;
                    var name = service.ServiceKey as string; // The name key (if registered with a name)

                    // Add to the list
                    implementations.Add((name, implementationType));
                }
            }

            return implementations;
        }
    }
}
