using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{

    // إنشاء تذكرة من العميل
    public class TicketCreateRequest
    {
        public long TenantId { get; set; }

        [Required, MaxLength(200)]
        public string SubjectLine { get; set; } = null!;

        [Required] // Low|Medium|High|Urgent (غير حساسة لحالة الأحرف)
        public string PriorityLevel { get; set; } = null!;

        // مرفق اختياري (احفظ المسار بعد الرفع)
        public string? AttachmentPath { get; set; }

        [Required]
        public string BodyText { get; set; } = null!;
    }

    // إضافة رد قديم (أبقيناه لمن يستخدمه)
    public class TicketReplyRequest
    {
        public long TicketId { get; set; }
        public long TenantId { get; set; }
        public bool ByAdmin { get; set; }
        public string Message { get; set; } = null!;
    }

    // نموذج الصف المعروض في الجداول
    public sealed class TicketDto
    {
        public long TicketId { get; set; }
        public long TenantId { get; set; }

        public string TenantName { get; set; } = "";
        public string TenantEmail { get; set; } = "";

        public string SubjectLine { get; set; } = "";
        public string PriorityLevel { get; set; } = "";

        public int TicketStatusId { get; set; }
        public string TicketStatusName { get; set; } = "";

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public int ChatsCount { get; set; }
    }

    public sealed class UpdateStatusRequest
    {
        [Required] public int StatusId { get; set; }     // 1 Open, 2 InProgress, 3 Waiting, 4 Closed
    }

    public sealed class UpdatePriorityRequest
    {
        [Required] public string Priority { get; set; } = ""; // Low|Medium|High|Urgent
    }

    public sealed class BulkUpdateStatusRequest
    {
        [Required] public List<long> TicketIds { get; set; } = new();
        [Required] public int StatusId { get; set; }
    }

    // إضافة رسالة محادثة (للعميل أو للإدارة)
    public sealed class AddChatRequest
    {
        [Required] public long TenantId { get; set; }
        [Required] public string Message { get; set; } = "";
        public bool IsAdmin { get; set; }
        
    }

    


}
