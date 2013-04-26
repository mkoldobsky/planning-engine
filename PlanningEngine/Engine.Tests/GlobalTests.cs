namespace Engine.Core.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class GlobalTests
    {
        [Test]
        public void ItShouldBeNotValidWhenIsConstantAndDateoutOfScope()
        {
            var global = new Global<int> {IsConstant = true};
            global.From = new DateTime(2011, 12, 12);
            global.To = new DateTime(2011, 12, 31);
            Assert.IsFalse(global.IsValid(DateTime.Today));
        }

        [Test]
        public void ItShouldBeAlwaysValidWhenNotConstant()
        {
            var global = new Global<int> {IsConstant = false};
            Assert.IsTrue(global.IsValid(DateTime.Today));
        }

        [Test]
        public void ItShouldBeValidWhenConstantAndDateInScope()
        {
            var global = new Global<int> { IsConstant = true };
            global.From = new DateTime(2011, 12, 12);
            global.To = new DateTime(2011, 12, 31);
            Assert.IsTrue(global.IsValid(new DateTime(2011, 12, 12)));
        }

        [Test]
        public void ItShouldSetConstantValues()
        {
            var global = new Global<int>();
            global.SetConstant(10);
            Assert.AreEqual(10, global.Value[Month.January]);
            Assert.AreEqual(10, global.Value[Month.December]);
        }
    }
}
