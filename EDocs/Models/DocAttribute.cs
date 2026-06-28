using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edocs.Models
{
    public class DocAttribute
    {
        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int ID { get; set; }
        [DisplayName("اسم الواصفة"), Required(ErrorMessage = "ادخل الاسم")]
        public string NAME { get; set; }
        [DisplayName("نمط الواصفة"), Required(ErrorMessage = "ادخل النمط")]
        public string TYPE { get; set; }


        [DisplayName("تصنيف الواصفة")]
        [ForeignKey("CategoryFk")]
        public int? Category { get; set; }

        [DisplayName("ترتيب الواصفة")]
        public int COLM_ORDER { get; set; }
        public virtual Category CategoryFk { get; set; }


        public virtual ICollection<DocAttributes> DocAttributes { get; set; }

        public virtual ICollection<AttributeList> AttributeList { get; set; }
        public enum AttributeType
        {
            رقم,
            نص,
            تاريخ,
            ملف,
            محرر
        }

    }
}
