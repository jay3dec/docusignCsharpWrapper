using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocusignEntity
{
    public class ListCustomFields
    {
        public List<string> listItems { get; set; }
        public string value { get; set; }
        public bool required { get; set; }
        public bool show { get; set; }
        public string name { get; set; }
    }
}
