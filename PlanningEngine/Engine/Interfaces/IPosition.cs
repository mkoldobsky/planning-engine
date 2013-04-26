using Disney.HR.HCM.Contract;

namespace Engine.Core.Interfaces
{
    using System;
    using FromDisney;

    public interface IPosition
    {
        HCTypeDto HCType { get; set; }
        HCPositionStatus Status { get; set; }
        CompanyDto Company { get; set; }
        Decimal? AnnualSalary { get; set; }
        int PositionLogId { get; set; }
        int? EmployeeLogId { get; set; }
    }
}
