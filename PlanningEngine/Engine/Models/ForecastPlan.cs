using Disney.HR.HCM.Contract;

namespace Engine.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Engine.Core.Interfaces;
    using FromDisney;

    public class ForecastPlan<T> : Entity, IForecastPlan
    {
        public ForecastPlanStatus Status;
        public string StatusCode { 
            get 
            { return Enum.GetName(typeof(ForecastPlanStatus), GetStatus()); }  
            set { }
        }

        public int FiscalYearId { get; set; }

        public string UploadedFileName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public int? From { get; set; }
        public int? To { get; set; }

        public List<IPosition> PositionLogs { get; set; }

        public DateTime? CreationDate { get; set; }
        public DateTime? ExecutionDate { get; set; }

        public int CreationUserId { get; set; }

        public string CreationUserName { get; set; }

        public string FiscalYearCode { get; set; }


        public ForecastPlan()
        {
            PositionLogs = new List<IPosition>();
        }

        public bool Validate()
        {
            if (this.PositionLogs == null || this.PositionLogs.Count() == 0)
                throw new ForecastPlanException("No positions associated to plan");
            return true;
        }

        public void AddPosition(IPosition position)
        {
            PositionLogs.Add(position);
        }

        public int CountPositions()
        {
            return PositionLogs.Count;
        }

        public int CountOpenPositions()
        {
            var positions = PositionLogs.Where(x => x.Status == HCPositionStatus.Open).ToList();
            return positions.Count();
        }

        public int CountActivePositions()
        {
            return PositionLogs.Where(x => x.Status == HCPositionStatus.Active).Count();
        }

        public void AddPositions(List<IPosition> positions)
        {
            PositionLogs = positions;
        }

        public ForecastPlanStatus GetStatus()
        {
            return Status;
        }

        public void SetStatus(ForecastPlanStatus status)
        {
            Status = status;
        }

        public int CountReadyToHirePositions()
        {
            return PositionLogs.Where(x => x.Status == HCPositionStatus.ReadyToHire).Count();
        }

        public int CountChangePendingPositions()
        {
            return PositionLogs.Where(x => x.Status == HCPositionStatus.ChangePending).Count();
        }
    }
}
