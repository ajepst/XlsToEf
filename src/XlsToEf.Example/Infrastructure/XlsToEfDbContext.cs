using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace XlsToEf.Example.Infrastructure
{
    public class XlsToEfDbContext : DbContext
    {
        private DbContextTransaction _currentTransaction;

        public XlsToEfDbContext(string connString)
            : base(connString)
        {
            Database.SetInitializer<XlsToEfDbContext>(null);
        }


        public void InsertOrUpdate<TEntity>(TEntity entity, object Id) where TEntity : BaseEntity
        {
            if (!Id.Equals(GetDefaultValue(Id.GetType())))
            {
                if (this.Entry(entity).State == System.Data.Entity.EntityState.Detached)
                {
                    this.Set<TEntity>().Add(entity);
                }
                this.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                this.Set<TEntity>().Add(entity);
            }
        }

        public void InsertOrUpdate<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            base.Set<TEntity>().AddOrUpdate(entity);
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(typeof (XlsToEfDbContext).Assembly);
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
                    if (_currentTransaction != null && _currentTransaction.UnderlyingTransaction.Connection != null)
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