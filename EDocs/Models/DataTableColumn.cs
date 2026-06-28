using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace edocs.Models
{
    public class DataTableColumn
    {
        public string data { get; set; }
        public string @class { get; set; }
        public bool orderable { get; set; } = true;
        public string render { get; set; }
        public string defaultContent { get; set; }
        public bool searchable { get; set; }
    }
}