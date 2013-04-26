using Disney.HR.HCM.Contract;

namespace Engine.Sevices.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Engine.Core.Interfaces;
    using Engine.Core.Models;
    using Engine.DataAccess;
    using Engine.Services;
    using FromDisney;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ForecastServiceTest
    {
        [Test]
        public void ItShouldThrowExceptionWhenPlanNotExists()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.GetPlan(1)).Throws(new ForecastServiceException());
            var service = new ForecastService<int>(dataService.Object);
            Assert.Throws(typeof(ForecastServiceException), () => service.LoadPlan(1));

        }

        [Test]
        public void ItShouldLoadPlanWhenExists()
        {
            var forecastPlan = new ForecastPlan<int>{CreationDate = DateTime.Today};
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.GetPlan(1)).Returns(forecastPlan);
            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            Assert.AreEqual(forecastPlan, service.Plan);
        }



        //[Test]
        //public void ItShouldSetPlanAsCreatingWhenCreated()
        //{
        //    var positions = new List<IPosition>();
        //    var dataService = new Mock<IForecastDataAccessService>();
        //    dataService.Setup(x => x.PlanExist("Plan")).Returns(false);
        //    var plan = new Mock<IForecastPlan>();
        //    plan.Setup(x=>x.CreationDate)
        //    dataService.Setup(x => x.GetPlan("Plan")).Returns(.Object);
        //    var service = new ForecastService<int>(dataService.Object);
        //    service.CreatePlan("Plan", "Plan", 1);
            
        //    Assert.AreEqual(ForecastPlanStatus.Creating, service.GetStatus());
        //    Assert.AreEqual(DateTime.Today.Date, service.Plan.CreationDate.Value.Date);
        //}

        //[Test]
        //public void ItShouldFetchProperSchemeForEachPosition()
        //{
        //    var positions = new List<IPosition>();
        //    var positionOpen = new Mock<IPosition>();
        //    var positionActive1 = new Mock<IPosition>();
        //    var positionActive2 = new Mock<IPosition>();
        //    positionOpen.Setup(x => x.Status).Returns(HCPositionStatus.Open);
        //    positionOpen.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
        //    positionOpen.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });
        //    positionActive1.Setup(x => x.Status).Returns(HCPositionStatus.Active);
        //    positionActive1.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
        //    positionActive1.Setup(x => x.Company).Returns(new CompanyDto { Code = "4003" });
        //    positionActive2.Setup(x => x.Status).Returns(HCPositionStatus.Active);
        //    positionActive2.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
        //    positionActive2.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });

        //    positions.Add(positionActive1.Object);
        //    positions.Add(positionActive2.Object);
        //    positions.Add(positionOpen.Object);

        //    var dataService = new Mock<IForecastDataAccessService>();
        //    dataService.Setup(x => x.GetCurrentPositions()).Returns(positions);
        //    dataService.Setup(x => x.GetScheme(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        //    var service = new ForecastService<int>(dataService.Object);
        //    service.CreatePlan("Plan");
        //    service.Run();
        //    dataService.Verify(x => x.GetScheme("FTE", "4001"), Times.Exactly(2));
        //    dataService.Verify(x => x.GetScheme("FTE", "4003"), Times.Exactly(1));
        //}

        //[Test]
        //public void ItShouldThrowExceptionWhenRunWithoutPositions()
        //{
        //    var plan = new Mock<IForecastPlan>();
        //    plan.Setup(x => x.SetStatus(It.IsAny<ForecastPlanStatus>())).Verifiable();
        //    plan.Setup(x => x.CreationDate).Returns(DateTime.Today);

        //    var dataService = new Mock<IForecastDataAccessService>();
        //    dataService.Setup(x => x.GetPlan(1)).Returns(plan.Object);
        //    dataService.Setup(x => x.GetPositions(DateTime.Today)).Returns(new List<IPosition>());
        //    var service = new ForecastService<int>(dataService.Object);
        //    service.LoadPlan(1);
        //    Assert.Throws(typeof(ForecastServiceException), () => service.Run());

        //}

        [Test]
        public void ItShouldChangeStatusToRunningWhenRunning()
        {
            var plan = new Mock<IForecastPlan>();
            plan.Setup(x => x.SetStatus(It.IsAny<ForecastPlanStatus>())).Verifiable();
            plan.Setup(x => x.CreationDate).Returns(DateTime.Today);
            var positions = new List<IPosition>();
            var positionOpen = new Mock<IPosition>();
            var positionActive1 = new Mock<IPosition>();
            var positionActive2 = new Mock<IPosition>();
            positionOpen.Setup(x => x.Status).Returns(HCPositionStatus.Open);
            positionOpen.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionOpen.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });
            positionActive1.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive1.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive1.Setup(x => x.Company).Returns(new CompanyDto { Code = "4003" });
            positionActive2.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive2.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive2.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });

            positions.Add(positionActive1.Object);
            positions.Add(positionActive2.Object);
            positions.Add(positionOpen.Object);

            plan.Setup(x => x.PositionLogs).Returns(positions);


            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.GetPlan(1)).Returns(plan.Object);
            //dataService.Setup(x => x.GetPositions(DateTime.Today)).Returns(positions);
            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            service.Run();
            plan.Verify(x => x.SetStatus(ForecastPlanStatus.Running), Times.Once());

        }

        [Test]
        public void ItShouldChangeStatusToRunAfterRun()
        {
            var plan = new Mock<IForecastPlan>();
            plan.Setup(x => x.SetStatus(It.IsAny<ForecastPlanStatus>())).Verifiable();
            plan.Setup(x => x.CreationDate).Returns(DateTime.Today);
            var positions = new List<IPosition>();
            var positionOpen = new Mock<IPosition>();
            var positionActive1 = new Mock<IPosition>();
            var positionActive2 = new Mock<IPosition>();
            positionOpen.Setup(x => x.Status).Returns(HCPositionStatus.Open);
            positionOpen.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionOpen.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });
            positionActive1.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive1.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive1.Setup(x => x.Company).Returns(new CompanyDto { Code = "4003" });
            positionActive2.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive2.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive2.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });

            positions.Add(positionActive1.Object);
            positions.Add(positionActive2.Object);
            positions.Add(positionOpen.Object);

            plan.Setup(x => x.PositionLogs).Returns(positions);


            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.GetPlan(1)).Returns(plan.Object);
            //dataService.Setup(x => x.GetPositions(DateTime.Today)).Returns(positions);
            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            service.Run();
            Thread.Sleep(5000);
            plan.Verify(x => x.SetStatus(ForecastPlanStatus.Run), Times.Once());

        }

        [Test]
        public void ItShouldSetExecutionDateAfterRun()
        {
            var plan = new ForecastPlan<int>{CreationDate = DateTime.Today};
            var positions = new List<IPosition>();
            var positionOpen = new Mock<IPosition>();
            var positionActive1 = new Mock<IPosition>();
            var positionActive2 = new Mock<IPosition>();
            positionOpen.Setup(x => x.Status).Returns(HCPositionStatus.Open);
            positionOpen.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionOpen.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });
            positionActive1.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive1.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive1.Setup(x => x.Company).Returns(new CompanyDto { Code = "4003" });
            positionActive2.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive2.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive2.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });

            positions.Add(positionActive1.Object);
            positions.Add(positionActive2.Object);
            positions.Add(positionOpen.Object);
            plan.AddPositions(positions);

            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.GetPlan(1)).Returns(plan);
            //dataService.Setup(x => x.GetPositions(It.IsAny<DateTime>())).Returns(positions);
            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            service.Run();
            Thread.Sleep(5000);
            Assert.AreEqual(DateTime.Today.Date, service.Plan.ExecutionDate.Value.Date);

        }

        [Test]
        public void ItShouldNotDeleteNonExistingPlan()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.DeletePlan(It.IsAny<int>())).Returns(false);

            var service = new ForecastService<int>(dataService.Object);
            Assert.IsFalse(service.DeletePlan(1));

        }

        [Test]
        public void ItShouldDeleteExistingPlan()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.DeletePlan(It.IsAny<int>())).Returns(true);

            var service = new ForecastService<int>(dataService.Object);
            Assert.IsTrue(service.DeletePlan(1));

        }

        [Test]
        public void ItShouldCopyPlan()
        {
            var plan = new Mock<IForecastPlan>();
            plan.Setup(x => x.Title).Returns("PlanName");
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.CopyPlan(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            dataService.Setup(x => x.GetPlan(It.IsAny<int>())).Returns(plan.Object);

            var service = new ForecastService<int>(dataService.Object);
            service.CopyPlan(1, "UserName");
            dataService.Verify(x=>x.CopyPlan(1, "PlanName - Copy", "UserName"), Times.Once());

        }

        [Test]
        public void ItShouldDeleteResults()
        {
            var plan = new ForecastPlan<int>{CreationDate = DateTime.Today, Id = 1};
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.DeleteResults(It.IsAny<int>())).Verifiable();
            dataService.Setup(x => x.GetPlan(1)).Returns(plan);


            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            service.DeleteResults();
            dataService.Verify(x => x.DeleteResults(1), Times.Once());
            
        }


        [Test]
        public void ItShouldDeleteResultsBeforeRun()
        {
            var plan = new ForecastPlan<int> { CreationDate = DateTime.Today, Id = 1 };
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.DeleteResults(It.IsAny<int>())).Verifiable();
            dataService.Setup(x => x.GetPlan(1)).Returns(plan);
            var positions = new List<IPosition>();
            var positionOpen = new Mock<IPosition>();
            var positionActive1 = new Mock<IPosition>();
            var positionActive2 = new Mock<IPosition>();
            positionOpen.Setup(x => x.Status).Returns(HCPositionStatus.Open);
            positionOpen.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionOpen.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });
            positionActive1.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive1.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive1.Setup(x => x.Company).Returns(new CompanyDto { Code = "4003" });
            positionActive2.Setup(x => x.Status).Returns(HCPositionStatus.Active);
            positionActive2.Setup(x => x.HCType).Returns(new HCTypeDto { Code = "FTE" });
            positionActive2.Setup(x => x.Company).Returns(new CompanyDto { Code = "4001" });

            positions.Add(positionActive1.Object);
            positions.Add(positionActive2.Object);
            positions.Add(positionOpen.Object);
            plan.PositionLogs = positions;

            var service = new ForecastService<int>(dataService.Object);
            service.LoadPlan(1);
            service.Run();
            dataService.Verify(x => x.DeleteResults(1), Times.Once());
        }

        [Test]
        public void ItShouldVerifyIfPlanExists()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.PlanExist("Plan")).Returns(true);

            var service = new ForecastService<decimal>(dataService.Object);
            Assert.IsTrue(service.PlanExist("Plan"));
        }


        [Test]
        public void ItShouldNotVerifyIfPlanNotExists()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.PlanExist("Plan")).Returns(false);

            var service = new ForecastService<decimal>(dataService.Object);
            Assert.IsFalse(service.PlanExist("Plan"));
        }

        [Test]
        public void ItShouldThrowExceptionWhenCreatingAnExistingPlan()
        {
            var dataService = new Mock<IForecastDataAccessService>();
            dataService.Setup(x => x.PlanExist("Plan")).Returns(true);

            var service = new ForecastService<decimal>(dataService.Object);
            Assert.Throws(typeof(ForecastServiceException),()=>service.CreatePlan("Plan", "Plan", 1, "userName", 1, 12));

        }


        // ******************* Integration
        [Test]
        public void ItShouldRunPlan()
        {
            var planId = 15;
            var service = new ForecastService<decimal>(new ForecastDataAccessService());
            service.LoadPlan(planId);
            service.Run();

            Thread.Sleep(30000);
        }
    }
}
