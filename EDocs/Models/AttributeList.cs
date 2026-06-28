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
    public class AttributeList
    {


        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int ID { get; set; }
        [DisplayName("قيم الواصفة"), Required(ErrorMessage = "ادخل الاسم")]
        public string VALUE { get; set; }


        [DisplayName("تصنيف الواصفة")]

        [ForeignKey("AttributeFk")]
        public int? ATTRID { get; set; }

        public virtual DocAttribute AttributeFk { get; set; }


    }
}