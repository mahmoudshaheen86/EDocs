using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edocs.Models;

namespace edocs.Models
{
    public class DocFile
    {
        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int ID { get; set; }

        [DisplayName("اسم الملف")]
        public string NAME { get; set; }

        [DisplayName("تاريخ الملف"), Required(ErrorMessage = "تاريخ الادخال")]
        public DateTime DATEOF { get; set; }

        [DisplayName("حالة الملف"), Required(ErrorMessage ="حالة الملف")]
        public int STATUS { get; set; }

        [DisplayName("اسم الوثيقة"), Required(ErrorMessage = "اسم الوثيقة")]
        [ForeignKey("DOCFk")]
        public int? DOCUMENTID { get; set; }

        public virtual Documenti DOCFk { get; set; }

        [DisplayName("اسم المستخدم"), Required(ErrorMessage = "اسم المستخدم")]
        [ForeignKey("USERFk")]
        public int? USERID { get; set; }

        public virtual ApplicationUser USERFk { get; set; }
    }
}
