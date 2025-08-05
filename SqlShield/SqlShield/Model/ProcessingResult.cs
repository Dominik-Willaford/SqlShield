using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Model
{
    public class ProcessingResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<string> StepsCompleted { get; set; } = new();
    }
}
