using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YASS.Web.Interface.Models
{
    public class Measure
    {

        public DateTimeOffset Timestamp { get; set; }
        public string ActivityIdentifier { get; set; }
        public string GroupIdentifier { get; set; }
        public string EntityIdentifier { get; set; }
        public string SensorName { get; set; }
        public SensorType SensorType { get; set; }
        public string Value { get; set; }

    }
}
