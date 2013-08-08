using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class EnvelopeEvents
    {
        public EnvelopeEventStatus envelopeEventStatusCode { get; set; }
    }
    public enum EnvelopeEventStatus
    {
        sent,
        comlpeted,
        Delivered,
        Declined,
        Voided
    }
}
