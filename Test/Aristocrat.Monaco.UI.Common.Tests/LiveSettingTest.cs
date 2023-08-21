namespace Aristocrat.Monaco.UI.Common.Tests
{
    using Aristocrat.Monaco.UI.Common.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Aristocrat.Monaco.UI.Common.Models.LiveSetting;

    /// <summary>
    /// <see cref="LiveSetting"/> tests.
    /// </summary>
    [TestClass]
    public class LiveSettingTest
    {
        [TestMethod]
        public void StateTransitionTest()
        {
            // setup
            var a = new AssertSetting();
            var s = new LiveSetting<string>(null, null)
            {
                OnEditing = (setting, v) => v == "bad" ? "fixed" : v,
                OnChanged = setting => { a.OnChangedCount++; }
            };

            // verify default state
            a.AssertState(live: default, edit: default, status: EditStatus.Unedited, dirty: false, changed: false, actual: s);

            // verify the live value is echoed to the unedited value
            foreach (var v in new[] {"foo1", "foo2", "foo3"})
            {
                s.LiveValue = v;
                a.AssertState(live: v, edit: v, status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            }

            // verify conflict if live value is changed while the setting is focused
            s.LiveValue = "foo1";
            s.IsFocused = true;
            foreach (var v in new[] { "foo2", "foo3" })
            {
                s.LiveValue = v;
                a.AssertState(live: v, edit: "foo1", status: EditStatus.Conflicted, dirty: true, changed: true, actual: s);
            }

            // verify Reset() restores live-edit echo
            s.Reset();
            a.AssertState(live: default, edit: default, status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.LiveValue = "foo1";
            a.AssertState(live: "foo1", edit: "foo1", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.LiveValue = "foo2";
            a.AssertState(live: "foo2", edit: "foo2", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);

            // verify conflict/deconflict if live value is changed to different/same as edited value
            s.LiveValue = "foo1";
            a.AssertState(live: "foo1", edit: "foo1", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.EditedValue = "foo2";
            a.AssertState(live: "foo1", edit: "foo2", status: EditStatus.Edited, dirty: true, changed: true, actual: s);
            s.LiveValue = "foo2";
            a.AssertState(live: "foo2", edit: "foo2", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.LiveValue = "foo3";
            a.AssertState(live: "foo3", edit: "foo2", status: EditStatus.Conflicted, dirty: true, changed: true, actual: s);
            s.LiveValue = "foo4";
            a.AssertState(live: "foo4", edit: "foo2", status: EditStatus.Conflicted, dirty: true, changed: true, actual: s);
            s.LiveValue = "foo2";
            a.AssertState(live: "foo2", edit: "foo2", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.LiveValue = "foo3";
            a.AssertState(live: "foo3", edit: "foo2", status: EditStatus.Conflicted, dirty: true, changed: true, actual: s);

            // verify editing dirties/undirties as expected
            s.Reset();
            s.LiveValue = "foo1";
            a.AssertState(live: "foo1", edit: "foo1", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.EditedValue = "foo2";
            a.AssertState(live: "foo1", edit: "foo2", status: EditStatus.Edited, dirty: true, changed: true, actual: s);
            s.EditedValue = "foo1";
            a.AssertState(live: "foo1", edit: "foo1", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);

            // verify OnEditing delegate is called on attempted edit
            s.Reset();
            s.LiveValue = "foo1";
            a.AssertState(live: "foo1", edit: "foo1", status: EditStatus.Unedited, dirty: false, changed: true, actual: s);
            s.EditedValue = "bad";
            a.AssertState(live: "foo1", edit: "fixed", status: EditStatus.Edited, dirty: true, changed: true, actual: s);
        }

        private class AssertSetting
        {
            public int OnChangedCount = 0;
            private int LastOnChangedCount = 0;

            public void AssertState<T>(T live, T edit, EditStatus status, bool dirty, bool changed, LiveSetting<T> actual)
            {
                Assert.AreEqual(live, actual.LiveValue, "LiveValue");
                Assert.AreEqual(edit, actual.EditedValue, "EditedValue");
                Assert.AreEqual(status, actual.Status, "Status");
                Assert.AreEqual(dirty, actual.IsDirty, "IsDirty");
                var changeCountPassed = changed
                    ? OnChangedCount > LastOnChangedCount
                    : OnChangedCount == LastOnChangedCount;
                Assert.IsTrue(changeCountPassed, "OnChangedCount delegate invoked?");
                LastOnChangedCount = OnChangedCount;
            }
        }
    }
}