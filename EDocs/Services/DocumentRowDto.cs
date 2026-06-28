using System.Collections.Generic;

namespace edocs.Services
{
    public class DocumentRowDto
    {
        public int ID { get; set; }
        public string NUMBEROF { get; set; }
        public string DATEOFDOC { get; set; }
        public Dictionary<string, string> DocAttribute { get; set; }
        public int ATTACHMENTS { get; set; }
        public string ACTIONS { get; set; }
    }
}