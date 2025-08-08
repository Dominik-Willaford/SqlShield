using SqlShield.Interface;
using SqlShield.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SqlShield.Service
{
    internal class DatabaseService : IDatabaseService
    {
        private readonly SqlShieldSettings _sqlShieldSettings;

        public DatabaseService(IOptions<SqlShieldSettings> sqlShieldSettings)
        {
            _sqlShieldSettings = sqlShieldSettings.Value;
        }    

    }
}
