using edocs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace edocs.helper
{
    public static class StatusMessage
    {
        public enum MessageId
        {
            AddSuccess,
            AddError,
            EditeSuccess,
            EditeError,
            DeleteSuccess,
            DeleteError,
            connectionerror,
            dataerror,
            fileerror
        }

        public static string GetStatusMessage(MessageId? message)
        {
            if (!message.HasValue) return "";

            switch (message.Value)
            {
                case MessageId.AddSuccess:
                    return "<div class=\"alert alert-success\"><strong>إضافة</strong>تم عملية الإضافة بنجاح </div>";

                case MessageId.AddError:
                    return "<div class=\"alert alert-danger\"><strong>تنبيه</strong>فشل في عملية الإضافة </div>";

                case MessageId.EditeSuccess:
                    return "<div class=\"alert alert-info\"><strong>تعديل</strong>تم عملية التعديل بنجاح </div>";

                case MessageId.DeleteSuccess:
                    return "<div class=\"alert alert-danger\"><strong>حذف</strong>تمت عملية الحذف بنجاح</div>";

                case MessageId.fileerror:
                    return "<div class=\"alert alert-danger\"><strong>تنبيه</strong>فشل في عملية إضافة الملفات</div>";

                case MessageId.connectionerror:
                    return "<div class=\"alert alert-danger\"><strong>تنبيه</strong>حدث خطأ غير متوقع</div>";

                case MessageId.dataerror:
                    return "<div class=\"alert warning-\"><strong>تنبيه</strong>الرجاء التحقق من البيانات المدخلة</div>";

                default:
                    return "";
            }

        }
    }
}