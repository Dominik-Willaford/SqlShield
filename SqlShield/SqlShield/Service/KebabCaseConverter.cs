using SqlShield.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    internal class KebabCaseConverter : INameConventionConverter
    {
        /// <summary>
        /// Converts a kebab-case string to PascalCase.
        /// Example: "customer-id" -> "CustomerId"
        /// </summary>
        public string Convert(string databaseColumnName)
        {
            if (string.IsNullOrEmpty(databaseColumnName))
            {
                return databaseColumnName;
            }

            var parts = databaseColumnName.Split('-');
            var pascalCaseBuilder = new System.Text.StringBuilder();

            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    pascalCaseBuilder.Append(char.ToUpper(part[0]));
                    pascalCaseBuilder.Append(part.Substring(1));
                }
            }

            return pascalCaseBuilder.ToString();
        }
    }
}
