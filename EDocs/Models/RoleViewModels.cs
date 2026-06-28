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
    public class RoleViewModels
    {
        public RoleViewModels() { }

        public RoleViewModels(Role role)
        {
            Id = role.Id;
            Name = role.Name;
        }

        public int Id { get; set; }
        [DisplayName("اسم الصلاحية:"), Required(ErrorMessage = "ادخل اسم الصلاحية")]
        public string Name { get; set; }
    }
}
