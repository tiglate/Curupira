﻿using Autofac;
using Curupira.Plugins.IoC;
using System.Diagnostics.CodeAnalysis;

namespace Curupira.WindowsService.Infra.IoC
{
    [ExcludeFromCodeCoverage]
    public static class AutofacContainerBuilder
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonModule>();
            builder.RegisterModule<LocalModule>();
            return builder.Build();
        }
    }
}
