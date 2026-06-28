using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using edocs.Models;

namespace edocs.Models
{
    public class DocMessage
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [DisplayName("الحاشية")]
        public string SENDNOTES { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("تاريخ الارسال")]
        public DateTime SENDDATE { get; set; }

        [DisplayName("اسم المرسل")]
        public int USERSENDID { get; set; }

        [DisplayName("اسم المرسل إليه")]
        public int USERECIVEID { get; set; }

        [DisplayName("الوثيقة")]
        [ForeignKey("DocumentiFk")]
        public int? Documenti { get; set; }
        public virtual Documenti DocumentiFk { get; set; }

    }
}