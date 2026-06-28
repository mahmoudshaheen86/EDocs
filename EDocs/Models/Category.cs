using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace edocs.Models
{
    public class Columns
    {
        public string names { get; set; }
        public Columns()
        {
            edocs.Models.ApplicationUser users = (edocs.Models.ApplicationUser)HttpContext.Current.Session["Users"];

            if (users.USERTYPE == "admin")
            {
                names = "[ { 'data': 'NAME' }, { 'data': 'PARENT_NAME' },{ 'data': 'DESCR' },{ 'data': 'FOLDER_NAME' },"
                + "{ render : function (data, type, row){"
                + "var linkEdit = '<a href =\"category/Edit?ID=-1\"> تعديل</a>';"
                + " linkEdit = linkEdit.replace(\"-1\" ,row.ID);"
                + "var linkDetails = '<a href=\"category/Details?ID=-1\">استعراض </a>';"
                + " linkDetails = linkDetails.replace(\"-1\" ,row.ID);"
                + "var linkDelete = '<a href = \"category/Delete?ID=-1\"> حذف  </a>';"
                + " linkDelete = linkDelete.replace(\"-1\" ,row.ID);"
                + "var linkATT = '<a href = \"DocAttribute/Index?ID=-1\" > الواصفات </a>';"
                + " linkATT = linkATT.replace(\"-1\",row.ID);"
                + "return (linkEdit + \"|\" + linkDetails + \"|\" +  linkDelete + \"|\" + linkATT);"
                + "} }  ]";
            }
            else if (users.USERTYPE == "manager")
            {
                names = "[ { 'data': 'NAME' }, { 'data': 'PARENT_NAME' },{ 'data': 'DESCR' },{ 'data': 'FOLDER_NAME' },"
                + "{ render : function (data, type, row){"
                + "var linkATT = '<a href = \"DocAttribute/Index?ID=-1\" > الواصفات </a>';"
                + " linkATT = linkATT.replace(\"-1\",row.ID);"
                + "return (linkATT);"
                + "} }  ]";
            }
            else
            {

            }

        }



    }
    public class Category
    {

        [DisplayName("الرقم:"), Required(ErrorMessage = "ادخل الرقم")]
        public int ID { get; set; }
        [DisplayName("اسم الصنف"), Required(ErrorMessage = "ادخل الاسم")]
        public string NAME { get; set; }
        [DisplayName("التوصيف"), Required(ErrorMessage = "ادخل توصيف")]
        public string DESCR { get; set; }
        [DisplayName("الصنف الأب")]

        [ForeignKey("ParentCategory")]
        public int? PARENT_NAME { get; set; }

        public virtual Category ParentCategory { get; set; }

        public virtual ICollection<Category> Children { get; set; }


        [DisplayName("اسم المجلد"), Required(ErrorMessage = "ادخل اسم المجلد")]

        public string FOLDER_NAME { get; set; }

        public virtual ICollection<DocAttribute> Attributes { get; set; }
        public virtual ICollection<Documenti> Documents { get; set; }
    }

    public  class CategoryDTO
    {

     
        public int ID { get; set; }
        public string NAME { get; set; }
        public string DESCR { get; set; }
        public string PARENT_NAME { get; set; }
        public string FOLDER_NAME { get; set; }


        public static CategoryDTO CoptTo(Category c)
        {
            string a = "";
            if(c.PARENT_NAME != 0)
            {
                a = c.ParentCategory.NAME;
            }
            return new CategoryDTO
            {
                DESCR = c.DESCR,
                FOLDER_NAME = c.FOLDER_NAME,
                ID = c.ID,
                NAME = c.NAME,
                PARENT_NAME = a
            };
        }
    }
}