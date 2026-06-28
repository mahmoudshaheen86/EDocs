using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using edocs.Models;

namespace edocs.ViewModels
{
    public class DocumentIndexViewModel
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public IEnumerable<DocAttribute> Attributes { get; set; }
        public string ColumnsConfig { get; set; }
        public string UserRole { get; set; }
        public string StatusMessage { get; set; }
        public bool HasPermission { get; set; }
        public string Columns { get; set; }
    }
}