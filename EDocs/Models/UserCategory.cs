using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace edocs.Models
{
    public class UserCategoryColumns
    {
        public string names { get; set; }
        public UserCategoryColumns()
        {
            edocs.Models.ApplicationUser users = (edocs.Models.ApplicationUser)HttpContext.Current.Session["Users"];

            if (users.USERTYPE == "admin" || users.USERTYPE == "manager")
            {
                names = "[ { \"data\": \"NAME\" }, { \"data\": \"UserName\" }, { \"data\": \"TYPE\" },"
    + "{ \"data\": null, \"render\" : function (data, type, row){"
    + "var linkEdit = '<a href=\"usercategory/Edit?ID=-1\"> تعديل</a>';"
    + " linkEdit = linkEdit.replace(\"-1\" ,row.ID);"
    + "var linkDetails = '<a href=\"usercategory/Details?ID=-1\">استعراض </a>';"
    + " linkDetails = linkDetails.replace(\"-1\" ,row.ID);"
    + "var linkDelete = '<a href=\"usercategory/Delete?ID=-1\"> حذف  </a>';"
    + " linkDelete = linkDelete.replace(\"-1\" ,row.ID);"
    + "return (linkEdit + \"|\" + linkDetails + \"|\" +  linkDelete);"
    + "} } ]";
            }
            else
            {

            }
        }
    }

    public class UserCategory
    {
        [DisplayName("رقم"), Required(ErrorMessage = "رقم ")]
        public int ID { get; set; }

        [DisplayName("نوع الحساب")]
        public string TYPE { get; set; }

        [ForeignKey("UserFK")]
        public int? USERID { get; set; }

        public virtual ApplicationUser UserFK { get; set; }


        [ForeignKey("CategoryFK")]
        public int? CATEGORYID { get; set; }

        public virtual Category CategoryFK { get; set; }
    }
}
