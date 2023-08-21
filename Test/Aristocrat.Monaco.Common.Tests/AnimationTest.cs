namespace Aristocrat.Monaco.Common.Tests
{
    using System;
    using System.Numerics;
    using Animation;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AnimationTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WhenEvaluatingZeroKeyFramesExpectException()
        {
            var animation = new Animation<double>();
            animation.Evaluate(0.3f);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void WhenEvaluatingUnsupportedTypesExpectException()
        {
            // Animation types require operator+, and operator*.
            var animation = new Animation<string>();
            animation.Insert(0.0f, "Hello");
            animation.Insert(1.0f, "World");
            animation.Evaluate(0.3f);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenInsertingDuplicateKeysExpectException()
        {
            var animation = new Animation<Vector2>();
            animation.Insert(0.2f, new Vector2(2.0f, 0.0f));
            animation.Insert(0.5f, new Vector2(3.0f, 0.0f));
            animation.Insert(0.5f, new Vector2(4.0f, 0.0f));
            animation.Insert(1.0f, new Vector2(5.0f, 0.0f));

            Assert.Fail();
        }

        [TestMethod]
        public void WhenInsertingExpectSorted()
        {
            var animation = new Animation<Vector2>();
            animation.Insert(2.0f, new Vector2(1.0f, 0.0f));
            animation.Insert(0.2f, new Vector2(2.0f, 0.0f));
            animation.Insert(0.5f, new Vector2(3.0f, 0.0f));
            animation.Insert(1.5f, new Vector2(4.0f, 0.0f));
            animation.Insert(1.0f, new Vector2(5.0f, 0.0f));
            animation.Insert(1.2f, new Vector2(6.0f, 0.0f));

            bool sorted = IsSorted(animation);

            Assert.IsTrue(sorted);
        }

        [TestMethod]
        public void EvaluateLinearModeFrontBoundaryTest()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>();
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(0.0f);

            Assert.IsTrue(result == p1);
        }

        [TestMethod]
        public void EvaluateLinearModeEndBoundaryTest()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>();
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(1.0f);

            Assert.IsTrue(result == p2);
        }

        [TestMethod]
        public void EvaluateLinearModeInteriorBoundaryTest()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>();
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(0.5f);

            var expected = new Vector2(2.0f, 1.5f);

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void EvaluateDiscreteModeFrontBoundaryTest()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>() { AnimationMode = AnimationMode.Discrete };
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(0.0f);

            Assert.IsTrue(result == p1);
        }

        [TestMethod]
        public void EvaluateDiscreteModeEndBoundaryTest()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>() { AnimationMode = AnimationMode.Discrete };
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(1.0f);

            Assert.IsTrue(result == p2);
        }

        [TestMethod]
        public void EvaluateDiscreteModeInteriorBoundaryTest1()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>() { AnimationMode = AnimationMode.Discrete };
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(0.4f);

            Assert.IsTrue(result == p1);
        }

        [TestMethod]
        public void EvaluateDiscreteModeInteriorBoundaryTest2()
        {
            var p1 = new Vector2(1.0f, 1.0f);
            var p2 = new Vector2(3.0f, 2.0f);

            var animation = new Animation<Vector2>() { AnimationMode = AnimationMode.Discrete };
            animation.Insert(0.25f, p1);
            animation.Insert(0.75f, p2);

            var result = animation.Evaluate(0.6f);

            Assert.IsTrue(result == p2);
        }

        private bool IsSorted<T>(Animation<T> animation)
        {
            for (int i = 1; i < animation.KeyFrameCount; ++i)
            {
                if (animation[i - 1].KeyTime > animation[i].KeyTime)
                {
                    return false;
                }
            }

            return true;
        }
    }
}