namespace Aristocrat.Monaco.UI.Common.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FluentAssertions;
    using Aristocrat.Monaco.UI.Common.MVVM;

    [TestClass]
    public class TrackableObservableValidatorTests
    {
        private class MockCustomObservableValidator : TrackableObservableValidator
        {
            private bool _commitPublic;
            public bool CommitPublic
            {
                get => _commitPublic;
                set
                {
                    _commitPublic = value;
                    OnPropertyChanged(nameof(CommitPublic));
                }
            }

            private bool _commitProtected;
            protected bool CommitProtected
            {
                get => _commitProtected;
                set
                {
                    _commitProtected = value;
                    OnPropertyChanged(nameof(CommitProtected));
                }
            }

            public void SetCommitProtected() => CommitProtected = true;

            private bool _commitPrivate;
            private bool CommitPrivate
            {
                get => _commitPrivate;
                set
                {
                    _commitPrivate = value;
                    OnPropertyChanged(nameof(CommitPrivate));
                }
            }

            public void SetCommitPrivate() => CommitPrivate = true;

            private bool _ignorePublic;
            [IgnoreTracking]
            public bool IgnorePublic
            {
                get => _ignorePublic;
                set
                {
                    _ignorePublic = value;
                    OnPropertyChanged(nameof(IgnorePublic));
                }
            }

            private bool _ignoreProtected;
            [IgnoreTracking]
            public bool IgnoreProtected
            {
                get => _ignoreProtected;
                set
                {
                    _ignoreProtected = value;
                    OnPropertyChanged(nameof(IgnoreProtected));
                }
            }

            public void SetIgnoreProtected() => IgnoreProtected = true;

            private bool _ignorePrivate;
            [IgnoreTracking]
            public bool IgnorePrivate
            {
                get => _ignorePrivate;
                set
                {
                    _ignorePrivate = value;
                    OnPropertyChanged(nameof(IgnorePrivate));
                }
            }
            public void SetIgnorePrivate() => IgnorePrivate = true;

            public void SetCommitted() => IsCommitted = true;

        }

        [TestMethod]
        public void NewObjectShouldBeCommitedByDefault()
        {
            var mock = new MockCustomObservableValidator();
            mock.IsCommitted.Should().BeTrue();
        }

        [TestMethod]
        public void SettingCommittedToTrueShouldNotSetItBackToFalseDueToChangeTracking()
        {
            var mock = new MockCustomObservableValidator();
            mock.SetCommitted();
            mock.IsCommitted.Should().BeTrue();
        }

        [TestMethod]
        public void SettingAPublicPropertyShouldResultInUncommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.CommitPublic = true;
            mock.IsCommitted.Should().BeFalse();
        }

        [TestMethod]
        public void SettingAProtectedPropertyShouldResultInUncommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.SetCommitProtected();
            mock.IsCommitted.Should().BeFalse();
        }

        [TestMethod]
        public void SettingAPrivatePropertyShouldResultInUncommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.SetCommitPrivate();
            mock.IsCommitted.Should().BeFalse();
        }

        [TestMethod]
        public void SettingAnIgnoredPublicPropertyShouldNotAffectCommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.IgnorePublic = true;
            mock.IsCommitted.Should().BeTrue();
        }

        [TestMethod]
        public void SettingAnIgnoredProtectedPropertyShouldNotAffectCommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.SetIgnoreProtected();
            mock.IsCommitted.Should().BeTrue();
        }

        [TestMethod]
        public void SettingAnIgnoredPrivatePropertyShouldNotAffectCommittedStatus()
        {
            var mock = new MockCustomObservableValidator();
            mock.SetIgnorePrivate();
            mock.IsCommitted.Should().BeTrue();
        }
    }
}
