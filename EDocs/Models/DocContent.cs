using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using edocs.Models;
namespace edocs.Models
{
    public class DocContentColumns
    {
        public string names { get; set; }
        public DocContentColumns()
        {
                names = "[ { 'data': 'TYPE' }, { 'data': 'CONTENT' },{ 'data': 'NOTES' },{ 'data': 'DESCRIPTION' },{ 'data': 'DATEDOC' },{ 'data': 'DOCUMENTID' },"
                + "{ render : function (data, type, row){"
                + "var linkEdit = '<a href =\"doc_contetn/Edit?ID=-1\"> تعديل</a>';"
                + " linkEdit = linkEdit.replace(\"-1\" ,row.ID);"
                + "var linkDetails = '<a href=\"doc_contetn/Details?ID=-1\">استعراض </a>';"
                + " linkDetails = linkDetails.replace(\"-1\" ,row.ID);"
                + "var linkDelete = '<a href = \"doc_contetn/Delete?ID=-1\"> حذف  </a>';"
                + " linkDelete = linkDelete.replace(\"-1\" ,row.ID);"
                + "return (linkEdit + \"|\" + linkDetails + \"|\" +  linkDelete + \"|\" + linkATT);"
                + "} }  ]";

        }



    }


    public class DocContent
    {
        [Key]
        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int NUMBERDOC { get; set; }



        [DisplayName("نوع الوثيقة"), Required(ErrorMessage = "ادخل نوع الوثيقة")]
        public string TYPE { get; set; }


        [DisplayName("معلومات الوثيقة"), Required(ErrorMessage = "ادخل معلومات الوثيقة")]
        public string CONTENT { get; set; }

        [DisplayName("ملاحظات الوثيقة"), Required(ErrorMessage = "ادخل ملاحظات الوثيقة")]
        public string NOTES { get; set; }

        [DisplayName(" توصيف الوثيقة"), Required(ErrorMessage = "ادخل توصيف الوثيقة")]
        public string DESCRIPTION { get; set; }


        [DataType(DataType.Date)]
        [DisplayName("التاريخ "), Required(ErrorMessage = "ادخل التاريخ الوثيقة")]
        public DateTime DATEDOC { get; set; }


        [DisplayName("الوثيقةالأساسية"), Required(ErrorMessage = "اسم الوثيقة")]
        [ForeignKey("DOCFk")]
        public int? DOCUMENTID { get; set; }
        public virtual Documenti DOCFk { get; set; }
    }



    public class DocContentDto
    {
        public int NUMBERDOC { get; set; }
        public string TYPE { get; set; }
        public string CONTENT { get; set; }
        public string NOTES { get; set; }

        public string DESCRIPTION { get; set; }
        public DateTime DATEDOC { get; set; }
        public int DOCUMENTID { get; set; }

        public static DocContentDto CoptTo(DocContent c)
        {
            return new DocContentDto
            {
                NUMBERDOC = c.NUMBERDOC,
                TYPE = c.TYPE,
                CONTENT = c.CONTENT,
                NOTES = c.NOTES,
                DESCRIPTION = c.DESCRIPTION,
                DATEDOC = c.DATEDOC,
                DOCUMENTID = c.DOCUMENTID.Value
            };
        }
    }

}