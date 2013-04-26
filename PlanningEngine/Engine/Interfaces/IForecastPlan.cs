namespace Engine.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using FromDisney;

    public interface IForecastPlan
    {
        int Id { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        int? From { get; set; }
        int? To { get; set; }
        List<IPosition> PositionLogs { get; set; }
        DateTime? CreationDate { get; set; }
        DateTime? ExecutionDate { get; set; }
        int CreationUserId { get; set; }
        string CreationUserName { get; set; }
        string FiscalYearCode { get; set; }
        string StatusCode { get; set; }
        int FiscalYearId { get; set; }
        string UploadedFileName { get; set; }
        bool Validate();
        void AddPosition(IPosition position);
        int CountPositions();
        int CountOpenPositions();
        int CountActivePositions();
        void AddPositions(List<IPosition> positions);
        ForecastPlanStatus GetStatus();
        void SetStatus(ForecastPlanStatus status);
        int CountReadyToHirePositions();
        int CountChangePendingPositions();
    }
}
