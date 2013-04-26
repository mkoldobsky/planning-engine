namespace Engine.DataAccess.Tests
{
    using NUnit.Framework;
    using Engine.Core;
    using System.Collections.Generic;

    [TestFixture]
    public class ForecastDataAccessServiceTests
    {
        [Test]
        public void ItShouldRetrieveAnSchemeAndItsConcepts()
        {
            var service = new ForecastDataAccessService();
            var scheme = service.GetScheme(1, 2012, null, null, 1, null, "FTE", "4001");
        }

        [Test]
        public void ItShouldCreatePlanWithProperPositions()
        {
            var userName = "userName";
            var service = new ForecastDataAccessService();
            service.CreateForecastPlan("TestCreatePlan", "Plan description", 1, userName, 1, 12);
        }

        [Test]
        public void ItshouldDeleteResults()
        {
            var service = new ForecastDataAccessService();
            service.DeleteResults(14);
        }

        [Test]
        public void ItShouldFindSchemes()
        {
            var service = new ForecastDataAccessService();
            List<Scheme<decimal>> schemes = service.GetSchemes(6, null, null, null, null, null, null);
            Assert.IsTrue(schemes.Count > 0);
        }
    }
}
