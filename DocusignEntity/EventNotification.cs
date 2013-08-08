using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class EventNotification
    {
        public List<RecipientEvents> RecipientEvents { get; set; }
        public List<EnvelopeEvents> EnvelopeEvents { get; set; }
        public bool includeSenderAccountasCustomField { get; set; }
        public bool includeEnvelopeVoidReason { get; set; }
        public bool includeTimeZoneInformation { get; set; }
        public bool includeDocuments { get; set; }
        public bool signMessagewithX509Certificate { get; set; }
        public bool includeCertificateWithSoap { get; set; }
        public string soapNameSpace { get; set; }
        public bool useSoapInterface { get; set; }
        public bool requireAcknowledgment { get; set; }
        public bool loggingEnabled { get; set; }
        public string url { get; set; }
    }
}
