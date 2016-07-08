using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using Fixie;
using Respawn;
using StructureMap;
using XlsToEf.Tests.Infrastructure;

namespace XlsToEf.Tests
{
    public static class TestDependencyScope
    {
        private static readonly Lazy<IContainer> RootContainer = new Lazy<IContainer>(InitializeContainer, true);

        private static IContainer _currentNestedContainer;

        private static IContainer InitializeContainer()
        {
            var container = IoC.Initialize();
            Bootstrapper.Initialize(container);
            return container;
        }

        public static void Begin()
        {
            if (_currentNestedContainer != null)
                throw new Exception("Cannot begin test dependency scope. Another dependency scope is still in effect.");

            _currentNestedContainer = RootContainer.Value.GetNestedContainer();

        }

        public static IContainer CurrentNestedContainer
        {
            get
            {
                if (_currentNestedContainer == null)
                    throw new Exception("Cannot access the current nested container. There is no dependency scope in effect.");

                return _currentNestedContainer;
            }
        }

        public static void End()
        {
            if (_currentNestedContainer == null)
                throw new Exception("Cannot end test dependency scope. There is no dependency scope in effect.");

            _currentNestedContainer.Dispose();
            _currentNestedContainer = null;
        }
    }

    public class TestingConvention : Convention
    {
        public TestingConvention()
        {
            Classes
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid() || method.IsAsync());

            ClassExecution
                .CreateInstancePerCase();

            CaseExecution
                .Wrap<DbTestBehavior>()
                .Wrap<NestedContainerBehavior>()
                .Skip(@case => @case.Method.HasOrInherits<SkipAttribute>());

            Parameters
                .Add<FromInputAttributes>();
        }
    }

    public class NestedContainerBehavior : CaseBehavior
    {
        public void Execute(Case context, Action next)
        {
            TestDependencyScope.Begin();
            next();
            TestDependencyScope.End();
        }
    }

    public class DbTestBehavior : CaseBehavior
    {
        public void Execute(Case context, Action next)
        {
            if (context.Class.IsSubclassOf(typeof(DbTestBase)))
                ResetDatabases();

            next();

            foreach (var ex in context.Exceptions.OfType<DbEntityValidationException>())
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Error on {0} entity: {1}", err.IsValid ? "valid" : "invalid", err.Entry);
                    foreach (var ve in err.ValidationErrors)
                    {
                        Console.WriteLine("  {0}: {1}", ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
        }

        private static void ResetDatabases()
        {
            var unitDatabase = ConfigurationManager.ConnectionStrings["UnitDatabase"].ToString();
            
            DatabaseTestCheckpoint.DbCheckpoint.Reset(unitDatabase);
        }


        private static class DatabaseTestCheckpoint
        {

            public static readonly Checkpoint DbCheckpoint = new Checkpoint
            {
                TablesToIgnore = new string[]
                {
                },
                SchemasToExclude = new[]
                {
                    "RoundhousE"
                },
            };
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public InputAttribute(params object[] parameters)
        {
            Parameters = parameters;
        }

        public object[] Parameters { get; private set; }
    }

    class FromInputAttributes : ParameterSource
    {
        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            return method.GetCustomAttributes<InputAttribute>(true).Select(input => input.Parameters);
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class SkipAttribute : Attribute
    {
    }
}