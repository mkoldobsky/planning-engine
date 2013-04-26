using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disney.HR.HCM.Contract;
using CompanyDto = FromDisney.CompanyDto;
using HCTypeDto = FromDisney.HCTypeDto;
using IPosition = Engine.Core.Interfaces.IPosition;

namespace Engine.Core.Models
{
    public class Position : IPosition
    {
        public HCTypeDto HCType  { get; set; }
        public HCPositionStatus Status  { get; set; }
        public CompanyDto Company  { get; set; }
        public decimal? AnnualSalary  { get; set; }
        public int PositionLogId  { get; set; }
        public int? EmployeeLogId { get; set; }
    }
}
