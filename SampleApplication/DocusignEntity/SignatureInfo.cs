using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class SignatureInfo
    {
        public FontStyle fontStyle { get; set; }
        public string signatureInitials { get; set; }
        public string signatureName { get; set; }
    }

    public enum FontStyle
    {
        Mistral,
        LucialBT
    }
}
