namespace Engine.Core.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class MonthlyParameterTests
    {
        [Test]
        public void ItShouldSetConstantValues()
        {
            var parameter = new MonthlyParameter<int>();
            parameter.SetConstant(10);
            Assert.AreEqual(10, parameter.Value[Month.January]);
            Assert.AreEqual(10, parameter.Value[Month.December]);
        }
    }
}
