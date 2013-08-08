using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class envelopeDefinition
    {
        public string accountId { get; set; }
        public string emailSubject { get; set; }
        public string emailBlurb { get; set; }
        public string templateId { get; set; }
        public string brandId { get; set; }
        public List<templateRole> templateRoles { get; set; }
        public CustomFields customFields { get; set; }
        public signatureStatus status { get; set; }
        public DocDetail documents { get; set; }
        public Recipients recipients { get; set; }
        public bool messageLock { get; set; }
    }

    public enum signatureStatus
    {
        sent,
        created
    }
}
