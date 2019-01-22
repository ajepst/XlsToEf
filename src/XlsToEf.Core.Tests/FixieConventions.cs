using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fixie;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace XlsToEf.Core.Tests
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

    public class TestingConvention : Discovery, Execution
    {
        public TestingConvention()
        {
            Classes
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .Where(method => method.IsVoid() || method.IsAsync())
                .Where(method => !method.Has<SkipAttribute>());

            Parameters
                .Add<FromInputAttributes>();
        }

        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                if (@case.Class.IsSubclassOf(typeof(DbTestBase)))
                    ResetDatabases();
                TestDependencyScope.Begin();
                var instance = testClass.Construct();

                @case.Execute(instance);
                instance.Dispose();
                TestDependencyScope.End();

                if(@case.Exception != null && (@case.Exception.GetType() == typeof(DbUpdateException)))
                {
                    var ex = @case.Exception as DbUpdateException;
                    foreach (var err in ex.Entries)
                    {
                        Console.WriteLine("Error on entity: {0}, msg {1}", err.Entity.TypeName(), ex.Message);
                    }
                }
            });
        }

        private static Task ResetDatabases()
        {
            var testDb = ConfigurationManager.ConnectionStrings["XlsToEfTestDatabase"].ToString();

            return DatabaseTestCheckpoint.DbCheckpoint.Reset(testDb);
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
