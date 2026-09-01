using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string MobileNo { get; set; } = "";

        public ICollection<QueueToken> QueueTokens { get; set; }

    }
}
