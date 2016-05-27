using System.Data.Entity;
using MediatR;
using StructureMap;
using XlsToEf.Import;

namespace XlsToEf.Example.DependencyResolution
{
    public class CommandProcessingRegistry : Registry
    {
        public CommandProcessingRegistry()
        {
            Scan(scan =>
            {
                scan.AssemblyContainingType<IMediator>();
                scan.AssemblyContainingType<CommandProcessingRegistry>();
                scan.AssemblyContainingType<ImportMatchingData>();

                scan.AddAllTypesOf(typeof(IRequestHandler<,>));
                scan.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                scan.AddAllTypesOf(typeof(INotificationHandler<>));
                scan.AddAllTypesOf(typeof(IAsyncNotificationHandler<>));
                scan.WithDefaultConventions();
            });

            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
        }
    }
}