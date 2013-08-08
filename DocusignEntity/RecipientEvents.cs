using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class RecipientEvents
    {
        public RecipientEventCode recipientEventStatusCode { get; set; }
    }

    public enum RecipientEventCode
    {
        Comlpeted,
        authenticationFailed,
        autoResponded,
        declined,
        delivered,
        sent
    }
}
