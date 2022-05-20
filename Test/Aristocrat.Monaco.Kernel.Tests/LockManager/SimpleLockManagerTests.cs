namespace Aristocrat.Monaco.Kernel.Tests.LockManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.LockManagement;
    using LockManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SimpleLockManagerTests
    {
        private SimpleLockManager _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _target = new SimpleLockManager();
            _target.Initialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(1)]
        [DataRow(4)]
        [DataTestMethod]
        public void SimpleLockManager_AcquireExclusiveLock(int noOfResources)
        {
            var resourceList = new List<Resource>();
            for (int index = 1; index <= noOfResources; index++)
            {
                resourceList.Add(new Resource($"resource{index}"));
            }

            _ = _target.AcquireExclusiveLock(resourceList);

            foreach (var resource in resourceList)
                Assert.IsTrue(resource.IsExclusiveLockTaken);
        }

        [DataRow(1)]
        [DataRow(4)]
        [DataTestMethod]
        public void SimpleLockManager_AcquireReadOnlyLock(int noOfResources)
        {
            var resourceList = new List<Resource>();
            for (int index = 1; index <= noOfResources; index++)
            {
                resourceList.Add(new Resource($"resource{index}"));
            }

            _ = _target.AcquireReadOnlyLock(resourceList);

            foreach (var resource in resourceList)
                Assert.IsTrue(resource.IsReadOnlyLockTaken);
        }


        [TestMethod]
        public void SimpleLockManager_ReleaseExclusiveLock()
        {
            var resource = new Resource("resource1");
            _ = _target.AcquireExclusiveLock(new List<ILockable> { resource });
            resource.ReleaseLock();

            Assert.IsFalse(resource.IsExclusiveLockTaken);
        }

        [TestMethod]
        public void SimpleLockManager_ReleaseReadOnlyLock()
        {
            var resource = new Resource("resource1");
            _ = _target.AcquireReadOnlyLock(new List<ILockable> { resource });
            resource.ReleaseLock();

            Assert.IsFalse(resource.IsExclusiveLockTaken);
        }

        [DataRow(1)]
        [DataRow(4)]
        [DataTestMethod]
        public void SimpleLockManager_ReleaseExclusiveLockOnDispose(int noOfResources)
        {
            var resourceList = new List<Resource>();
            for (int index = 1; index <= noOfResources; index++)
            {
                resourceList.Add(new Resource($"resource{index}"));
            }

            using (_target.AcquireExclusiveLock(resourceList))
            {
            }

            foreach (var resource in resourceList)
                Assert.IsFalse(resource.IsExclusiveLockTaken);
        }

        [DataRow(1)]
        [DataRow(4)]
        [DataTestMethod]
        public void SimpleLockManager_ReleaseReadOnlyLockOnDispose(int noOfResources)
        {
            var resourceList = new List<Resource>();
            for (int index = 1; index <= noOfResources; index++)
            {
                resourceList.Add(new Resource($"resource{index}"));
            }

            using (_target.AcquireReadOnlyLock(resourceList))
            {
            }

            foreach (var resource in resourceList)
                Assert.IsFalse(resource.IsReadOnlyLockTaken);
        }

        [TestMethod]
        public void SimpleLockManager_TryAcquireExclusiveLock()
        {
            var resource = new Resource("resource1");
            Assert.IsTrue(_target.TryAcquireExclusiveLock(new List<ILockable> { resource }, 100, out _));
        }

        [TestMethod]
        public void SimpleLockManager_TryAcquireReadOnlyLock()
        {
            var resource = new Resource("resource1");
            Assert.IsTrue(_target.TryAcquireReadOnlyLock(new List<ILockable> { resource }, 100, out _));
        }

        [TestMethod]
        public void SimpleLockManager_TryAcquireExclusiveLock_Fail_IfExlusiveLockAlreadyTaken()
        {
            var resource = new Resource("resource1");
            Task.Run(
                () => Assert.IsTrue(
                    _target.TryAcquireExclusiveLock(new List<ILockable> { resource }, 100, out _)));
            Task.Run(
                () => Assert.IsFalse(
                    _target.TryAcquireExclusiveLock(new List<ILockable> { resource }, 100, out _)));
        }

        [TestMethod]
        public void SimpleLockManager_TryAcquireReadOnlyLock_Fail_IfExclusiveLockAlreadyTaken()
        {
            var resource = new Resource("resource1");
            Task.Run(
                () => Assert.IsTrue(
                    _target.TryAcquireExclusiveLock(new List<ILockable> { resource }, 100, out _)));
            Task.Run(
                () => Assert.IsFalse(
                    _target.TryAcquireReadOnlyLock(new List<ILockable> { resource }, 100, out _)));
        }

        [TestMethod]
        public void SimpleLockManager_TryAcquireReadOnlyLock_Pass_IfReadOnlyLockAlreadyTaken()
        {
            var resource = new Resource("resource1");
            Task.Run(
                () => Assert.IsTrue(
                    _target.TryAcquireReadOnlyLock(new List<ILockable> { resource }, 100, out _)));
            Task.Run(
                () => Assert.IsTrue(
                    _target.TryAcquireReadOnlyLock(new List<ILockable> { resource }, 100, out _)));
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void SimpleLockManager_Exception_NegativeTimeoutArgument()
        {
            var resource = new Resource("resource1");
            _target.TryAcquireExclusiveLock(new List<ILockable> { resource }, -2, out _);
        }

        /// <summary>
        ///     A test resource class to act as resources to get locks on
        /// </summary>
        private class Resource : IReadWriteLockable
        {
            private readonly ReaderWriterLockSlim _lock;

            public Resource(string name)
            {
                UniqueLockableName = name;
                _lock = new ReaderWriterLockSlim();
            }

            public bool IsExclusiveLockTaken => _lock.IsWriteLockHeld;

            public bool IsReadOnlyLockTaken => _lock.IsReadLockHeld;

            public string UniqueLockableName { get; }

            public IDisposable AcquireExclusiveLock()
            {
                return _lock.GetWriteLock();
            }

            public void ReleaseLock()
            {
                _lock.ReleaseLock();
            }

            public bool TryAcquireExclusiveLock(int timeout, out IDisposable disposableToken)
            {
                return _lock.TryGetWriteLock(timeout, out disposableToken);
            }

            public IDisposable AcquireReadOnlyLock()
            {
                return _lock.GetReadLock();
            }

            public bool TryAcquireReadOnlyLock(int timeout, out IDisposable disposableToken)
            {
                return _lock.TryGetReadLock(timeout, out disposableToken);
            }
        }
    }
}