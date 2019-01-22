using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using XlsToEf.Core.Tests.Models;

namespace XlsToEf.Core.Tests.Infrastructure
{
    public class XlsToEfDbContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public XlsToEfDbContext(DbContextOptions<XlsToEfDbContext> options)
            : base(options)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<XlsToEfDbContext>(options => options.UseSqlServer("Data Source=blog.db"));
        }


        public void InsertOrUpdate<TEntity>(TEntity entity, object Id) where TEntity : BaseEntity
        {
            if (!Id.Equals(GetDefaultValue(Id.GetType())))
            {
                if (this.Entry(entity).State == EntityState.Detached)
                {
                    this.Set<TEntity>().Add(entity);
                }
                this.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                this.Set<TEntity>().Add(entity);
            }
        }

        protected object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof (XlsToEfDbContext).Assembly);
        }

        public void BeginTransaction()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    return;
                }

                _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            catch (Exception)
            {
                // todo: log transaction exception
                throw;
            }
        }

        public void RollbackTransaction()
        {
            if (_currentTransaction == null)
                throw new ApplicationException("Cannot rollback when no transaction was started");

            try
            {
                _currentTransaction.Rollback();
            }
            catch (Exception rollbackEx)
            {
                System.Diagnostics.Trace.WriteLine(rollbackEx);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void CloseTransaction()
        {
            CloseTransaction(exception: null);
        }

        public void CloseTransaction(Exception exception)
        {
            try
            {
                if (_currentTransaction != null && exception != null)
                {
                    // todo: log exception

                    RollbackTransaction();

                    return;
                }

                SaveChanges();

                if (_currentTransaction != null)
                {
                    _currentTransaction.Commit();
                }
            }
            catch (Exception closeTransactionEx)
            {
                System.Diagnostics.Trace.WriteLine(closeTransactionEx);
                try
                {
                    if (_currentTransaction != null && _currentTransaction.GetDbTransaction().Connection != null)
                    {
                        _currentTransaction.Rollback();
                    }
                }
                catch (Exception rollbackEx)
                {
                    System.Diagnostics.Trace.WriteLine(rollbackEx);
                }

                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}