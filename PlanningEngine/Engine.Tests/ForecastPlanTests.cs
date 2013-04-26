namespace Engine.Core.Tests
{
    using Engine.Core.Interfaces;
    using Engine.Core.Models;
    using FromDisney;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ForecastPlanTests
    {
        [Test]
        public void ItShouldNotValidateWhenNoPositions()
        {
            var forecastPlan = new ForecastPlan<int>();
            Assert.Throws(typeof (ForecastPlanException), ()=>forecastPlan.Validate());
        }

        [Test]
        public void ItShouldValidateWhenPositions()
        {
            var forecastPlan = new ForecastPlan<int>();
            forecastPlan.AddPosition(new Mock<IPosition>().Object);
          
            Assert.IsTrue(forecastPlan.Validate());
        }

        [Test]
        public void ItShouldFetchSchemeForEachPosition()
        {
            var forecastPlan = new ForecastPlan<int>();
            var position = new Mock<IPosition>();
            position.Setup(x => x.HCType).Returns(new HCTypeDto{Code = "FTE"});
            forecastPlan.AddPosition(position.Object);

            
        }
    
    }
}
