using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core
{
    public class HCType
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool IsTemporal { get; set; }
        public bool IsAdjust { get; set; }
        public int ADMWAccountID { get; set; }
        public string ADMWAccountType { get; set; }
    }
}
