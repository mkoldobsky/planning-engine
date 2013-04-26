using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FromDisney
{
    public class HCTypeDto
    {
        public string Code { get; set; }
        public bool IsTemporal { get; set; }
        public bool IsAdjust { get; set; }
        public int ADMWAccountID { get; set; }
        public string ADMWAccountType { get; set; }
    }

    public class CompanyDto
    {
        public string Code { get; set; }
    }

    public class ParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class FilterDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ParameterType ParameterType { get; set; }
        public int Parameter1Id { get; set; }
        public int Parameter2Id { get; set; }
        public int FilterTypeId { get; set; }
        public string OperationSymbol { get; set; }
        public string OperationName { get; set; }
        public string Connector { get; set; }
        public int? Sequence { get; set; }
        public string FixedValue { get; set; }
    }

    [Serializable]
    public class InputParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterType ParameterType { get; set; }
        public string Deleted { get; set; }
    }

    [Serializable]
    public class ExitParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IsAccumulable { get; set; }
    }

    [Serializable]
    public class ConstantDto
    {
        public int FiscalYearId { get; set; }
        public string FiscalYearCode { get; set; }

        #region Months

        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Ago { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }

        #endregion
    }

    [Serializable]
    public class SBAdjustmentDto
    {
        public int TempId { get; set; }
        public string Title { get; set; }
        public int CCode { get; set; }
        public string CompanyName { get; set; }
        public int Lob { get; set; }
        public string LobName { get; set; }
        public int HCType { get; set; }
        public string HCTypeName { get; set; }
        public int CCenter { get; set; }
        public string CCenterName { get; set; }
        public int ExpType { get; set; }
        public string ExpTypeName { get; set; }
        public int GLAccount { get; set; }
        public string GLAccountName { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }
        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Aug { get; set; }
        public decimal Sep { get; set; }
    }
}
