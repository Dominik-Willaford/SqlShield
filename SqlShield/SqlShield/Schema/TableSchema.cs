using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Schema
{
    public class TableSchema
    {
        /// <summary>
        /// The actual name of the table in the database
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// A fast-lookup set of all column names in the table.
        /// </summary>
        public HashSet<string> Columns { get; }

        public TableSchema(string tableName, IEnumerable<string> columnNames)
        {
            TableName = tableName;
            // Using OrdinalIgnoreCase is crucial for case-insensitive database comparisons.
            Columns = new HashSet<string>(columnNames, StringComparer.OrdinalIgnoreCase);
        }
    }
}
