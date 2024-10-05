using System;
using System.Collections.Generic;

namespace Curupira.AppClient.Services
{
    public interface IAutofacHelper
    {
        IList<(string Name, Type ImplementationType)> GetNamedImplementationsOfInterface<TInterface>();
    }
}
