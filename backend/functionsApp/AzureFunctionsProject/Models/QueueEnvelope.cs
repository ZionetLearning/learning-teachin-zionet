using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsProject.Models
{    public class QueueEnvelope<T>
    {
        public string Action { get; set; }
        public T? Entity { get; set; }   // null on Delete
        public Guid? Id { get; set; }   // populated on Delete
        public uint? Version { get; set; }
    }
}
