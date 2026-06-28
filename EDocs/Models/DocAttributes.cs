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
    public class DocAttributesColumns
    {
        public string names { get; set; }
        public DocAttributesColumns()
        {
            names = "[  { 'data': 'NAME' }, { 'data': 'PARENT_NAME' },{ 'data': 'DESCR' },{ 'data': 'FOLDER_NAME' }]";
        }
    }


    public class DocAttributes
    {
        [DisplayName("رقم الواصفة"), Required(ErrorMessage = "رقم الواصفة")]
        public int ID { get; set; }

        [DisplayName("القيمة")]
        public string VALUEOF { get; set; }

        [ForeignKey("AttributeFk")]
        public int? ATTRIBUTEID { get; set; }

        public virtual DocAttribute AttributeFk { get; set; }


        [ForeignKey("DOCFk")]
        public int? DOCUMENTID { get; set; }

        public virtual Documenti DOCFk { get; set; }


    }

    public class DocAttributesDto
    {
        public int Id { get; set; }
        public string valuOf { get; set; }

        public static DocAttributesDto CoptTo(DocAttributes d)
        {
            return new DocAttributesDto
            {
                Id = d.ID,
                valuOf = d.VALUEOF,
            };


        }
     }
}
