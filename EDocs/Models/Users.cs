using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace edocs.Models
{
    public class Users
    {
        [Required]
        [Display(Name = "الرقم:")]
        public int Id { get; set; }


        [Required]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "نوع الحساب")]
        public string RoleName { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = " حقل {0}  المدخل لا يمكن أن يكون بريد ألكتروني .", MinimumLength = 6)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = " {0}   يجب أن لا تقل عن {2} محارف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة السر")]
        [Compare("Password", ErrorMessage = "حقل كلمة السر وحقل تأكيد كلمة السر غير متطابقين")]
        public string ConfirmPassword { get; set; }
    }
}