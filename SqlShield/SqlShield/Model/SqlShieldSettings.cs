using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Model
{
    public class SqlShieldSettings
    {
        public int Iterations { get; set; }
        public Dictionary<string, ConnectionSetting> Connections { get; set; }
    }
}
