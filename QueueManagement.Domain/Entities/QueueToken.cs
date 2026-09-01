using QueueManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManagement.Domain.Entities
{
    public class QueueToken
    {
        public int Id { get; set; }

        public string TokenNo { get; set; } = "";

        public DateTime CreatedDate { get; set; }

        public QueueStatus Status { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int? CounterId { get; set; }
        public Counter Counter { get; set; }

        public DateTime? CalledTime { get; set; }

        public DateTime? CompletedTime { get; set; }
    }
}
