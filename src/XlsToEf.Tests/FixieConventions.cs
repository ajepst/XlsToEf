using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using Fixie;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using XlsToEf.Tests;

namespace XlsToEf.Tests
{
    public class TestDependencyScope
    {
        private static readonly Lazy<IServiceCollection> RootContainer =
            new Lazy<IServiceCollection>(InitializeContainer, true);

        private static IServiceScope _currentScope;

        private static IServiceCollection InitializeContainer()
        {
            var sc = new ServiceCollection();
            return sc;
        }

        public static void Begin()
        {

            if (_currentScope != null)
                throw new Exception("Cannot begin test dependency scope. Another dependency scope is still in effect.");

            _currentScope = RootContainer.Value.BuildServiceProvider().GetService<IServiceScopeFactory>().CreateScope();

        }

        public static IServiceScope CurrentScope
        {
            get
            {
                if (_currentScope == null)
                    throw new Exception(
                        "Cannot access the current nested container. There is no dependency scope in effect.");
                return _currentScope;
            }
        }

        public static void End()
        {
            if (_currentScope == null)
                throw new Exception("Cannot end test dependency scope. There is no dependency scope in effect.");
            _currentScope.Dispose();
            _currentScope = null;
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
            if (context.Class.IsSubclassOf(typeof (DbTestBase)))
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
            var testDb = ConfigurationManager.ConnectionStrings["XlsToEfTestDatabase"].ToString();

            DatabaseTestCheckpoint.DbCheckpoint.Reset(testDb);
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

    internal class FromInputAttributes : ParameterSource
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