using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Interface
{
    /// <summary>
    /// Defines the interface for a naming convention converter.
    /// This allows for different conversion strategies to be implemented and injected.
    /// </summary>
    public interface INameConventionConverter
    {
        string Convert(string databaseColumnName);
    }
}
