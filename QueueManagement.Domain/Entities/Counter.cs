using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Domain.Entities
{
    public class Counter
    {
        public int Id { get; set; }

        public string CounterName { get; set; } = "";

        public bool IsActive { get; set; }

        public ICollection<QueueToken> QueueTokens { get; set; }
    }
}
