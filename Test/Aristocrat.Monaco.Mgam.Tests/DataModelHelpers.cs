namespace Aristocrat.Monaco.Mgam.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Monaco.Common.Storage;
    using Moq;
    using Microsoft.EntityFrameworkCore;

    public static class DataModelHelpers
    {
        public static (Mock<IUnitOfWork> unit, Mock<Protocol.Common.Storage.Repositories.IRepository<T>> repo) SetUpDataModel<T>(
            Mock<IUnitOfWorkFactory> unitOfWorkFactory,
            T data)
            where T : BaseEntity
        {
            var unitOfWork = new Mock<IUnitOfWork>();

            unitOfWorkFactory.Setup(u => u.Create()).Returns(unitOfWork.Object).Verifiable();
            var work = AddRepoValue(unitOfWork, data);

            return (unitOfWork, work);
        }

        public static Mock<Protocol.Common.Storage.Repositories.IRepository<T>> AddRepoNoValue<T>(
            Mock<IUnitOfWork> unitOfWork)
            where T : BaseEntity
        {
            var repo = new Mock<Protocol.Common.Storage.Repositories.IRepository<T>>();
            var query = new List<T>();
            repo.Setup(r => r.Queryable()).Returns(query.AsQueryable());
            unitOfWork.Setup(u => u.Repository<T>()).Returns(repo.Object);

            return repo;
        }

        public static Mock<Protocol.Common.Storage.Repositories.IRepository<T>> AddRepoValue<T>(
            Mock<IUnitOfWork> unitOfWork, T data)
            where T : BaseEntity
        {
            var repo = new Mock<Protocol.Common.Storage.Repositories.IRepository<T>>();
            var list = new List<T>();
            if (data != default(T))
            {
                list.Add(data);
            }
            repo.Setup(r => r.Queryable()).Returns(list.AsQueryable());
            unitOfWork.Setup(u => u.Repository<T>()).Returns(repo.Object);

            return repo;
        }
    }
}
