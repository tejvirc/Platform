namespace Aristocrat.Monaco.Mgam.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Monaco.Common.Storage;
    using Moq;

    public static class DataModelHelpers
    {
        public static (Mock<IUnitOfWork> unit, Mock<Protocol.Common.Storage.Repositories.IRepository<T>> repo, MockDbSet<T> query) SetUpDataModel<T>(
            Mock<IUnitOfWorkFactory> unitOfWorkFactory,
            T data)
            where T : BaseEntity
        {
            var unitOfWork = new Mock<IUnitOfWork>();

            unitOfWorkFactory.Setup(u => u.Create()).Returns(unitOfWork.Object).Verifiable();
            var work = AddRepoValue(unitOfWork, data);

            return (unitOfWork, work.repo, work.query);
        }

        public static (Mock<IUnitOfWork> unit, Mock<Protocol.Common.Storage.Repositories.IRepository<T>> repo, MockDbSet<T> query) SetUpDataModel<T>(
           Mock<IUnitOfWorkFactory> unitOfWorkFactory)
           where T : BaseEntity
        {
            var unitOfWork = new Mock<IUnitOfWork>();

            unitOfWorkFactory.Setup(u => u.Create()).Returns(unitOfWork.Object).Verifiable();
            var work = AddRepoNoValue<T>(unitOfWork);

            return (unitOfWork, work.repo, work.query);
        }

        public static (Mock<Protocol.Common.Storage.Repositories.IRepository<T>> repo, MockDbSet<T> query) AddRepoNoValue<T>(
            Mock<IUnitOfWork> unitOfWork)
            where T : BaseEntity
        {
            var repo = new Mock<Protocol.Common.Storage.Repositories.IRepository<T>>();
            var list = new List<T>();
            var query = new MockDbSet<T>(list);
            repo.Setup(r => r.Queryable()).Returns(query);
            unitOfWork.Setup(u => u.Repository<T>()).Returns(repo.Object);

            return (repo, query);
        }

        public static (Mock<Protocol.Common.Storage.Repositories.IRepository<T>> repo, MockDbSet<T> query) AddRepoValue<T>(
            Mock<IUnitOfWork> unitOfWork,
            T data)
            where T : BaseEntity
        {
            var repo = new Mock<Protocol.Common.Storage.Repositories.IRepository<T>>();
            var list = new List<T>();
            if (data != default(T))
            {
                list.Add(data);
            }
            var query = new MockDbSet<T>(list);
            repo.Setup(r => r.Queryable()).Returns(query);
            unitOfWork.Setup(u => u.Repository<T>()).Returns(repo.Object);

            return (repo, query);
        }

        public class MockDbSet<TEntity> : DbSet<TEntity>, IQueryable<TEntity> where TEntity : class
        {
            private List<TEntity> list = null;

            /// <summary>Initializes a new instance of the MockDbSet class.</summary>
            public MockDbSet(IEnumerable<TEntity> collection)
            {
                this.list = new List<TEntity>(collection);
            }

            public override IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
            {
                list.AddRange(entities);

                return list;
            }

            public override TEntity Add(TEntity entity)
            {
                list.Add(entity);

                return entity;
            }

            public override TEntity Attach(TEntity entity)
            {
                return entity;
            }

            public new TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
            {
                return (TDerivedEntity)list.FirstOrDefault();
            }

            public override TEntity Create()
            {
                return list.FirstOrDefault();
            }

            public override TEntity Find(params object[] keyValues)
            {
                return null;
            }

            public override System.Collections.ObjectModel.ObservableCollection<TEntity> Local
            {
                get { return new System.Collections.ObjectModel.ObservableCollection<TEntity>(this.list); }
            }

            public override TEntity Remove(TEntity entity)
            {
                list.Remove(entity);
                return entity;
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }

            public Type ElementType
            {
                get { return this.list.AsQueryable().ElementType; }
            }

            public System.Linq.Expressions.Expression Expression
            {
                get { return this.list.AsQueryable().Expression; }
            }

            public IQueryProvider Provider
            {
                get { return this.list.AsQueryable().Provider; }
            }
        }

    }
}
