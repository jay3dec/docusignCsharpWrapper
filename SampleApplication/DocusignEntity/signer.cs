using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class signer
    {
        public string recipientId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        //public offlineAttrbutes offlineAttrbutes { get; set; }
        //public DateTime signedDateTime { get; set; }
        //public DateTime deliveredDateTime { get; set; }
        //public DeliveryMethod deliveryMethod { get; set; }
        //public SignatureInfo signatureInfo { get; set; }
        public Tab tabs { get; set; }
        //public string routingOrder { get; set; }
        //public string clientUserId { get; set; }

    }

    public enum DeliveryMethod
    {
        Email,
        Offline
    }
}
