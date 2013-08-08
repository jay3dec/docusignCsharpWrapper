using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class DocusignAuth
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string IntegratorKey { get; set; }
        public string TemplateId { get; set; }
        public string Url { get; set; }
    }

    public enum from_to_status
    {
        changed,
        voided,
        created,
        deleted,
        sent,
        delivered,
        signed,
        completed,
        declined,
        timedout
    }
}
