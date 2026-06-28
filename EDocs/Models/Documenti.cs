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

    public class DocColumns
    {
        public string names { get; set; }
        public DocColumns()
        {
            names = "[{'class' : 'red_design', 'data': 'NUMBEROF' },{'data': 'DATEOFDOC' },";
        }
    }


    public class Documenti
    {

        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int ID { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("تاريخ الادخال"), Required(ErrorMessage = "تاريخ الادخال")]
        public DateTime DATEIN { get; set; }


        [DisplayName("تاريخ الوثيقة"), Required(ErrorMessage = "الرجاء إدخال تاريخ الوثيقة")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DATEOFDOC { get; set; }
        [DisplayName("رقم الوثيقة"), Required(ErrorMessage = " الرجاء إدخال رقم تسلسلي")]
        public string NUMBEROF { get; set; }

        [DisplayName("الصنف "), Required(ErrorMessage = "الصنف")]
        [ForeignKey("CategoryFk")]
        public int? Category { get; set; }

        public virtual Category CategoryFk { get; set; }

        [DisplayName("اسم المستخدم"), Required(ErrorMessage = "اسم المستخدم")]
        [ForeignKey("USERFk")]
        public int? USERID { get; set; }

        public virtual ApplicationUser USERFk { get; set; }

        public virtual List<DocAttributes> DOCATTRIBUTEs { get; set; }

        public virtual ICollection<DocFile> DocFile { get; set; }

        public virtual ICollection<DocMessage> DOCSENTS { get; set; }

    }


    public class DocumentiDto
    {
        public int ID { get; set; }
        public string DATEIN { get; set; }
        public string DATEOFDOC { get; set; }
        public string NUMBEROF { get; set; }


        public static DocumentiDto CoptTo(Documenti d)
        {
            return new DocumentiDto
            {
                DATEIN = d.DATEIN.ToString(),
                ID = d.ID,
                DATEOFDOC = d.DATEOFDOC.HasValue ? d.DATEOFDOC.Value.ToString("yyyy-MM-dd"): null,
                NUMBEROF = d.NUMBEROF,
            };
        }
    }



}
