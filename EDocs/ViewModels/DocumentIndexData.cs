using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edocs.Models;

namespace edocs.ViewModels
{
    public class DocumentIndexData
    {
        public IEnumerable<Documenti> Documents { get; set; }
        public IEnumerable<DocAttributes> DocAttributes { get; set; }
        public IEnumerable<DocFile> Files { get; set; }
    }
}
