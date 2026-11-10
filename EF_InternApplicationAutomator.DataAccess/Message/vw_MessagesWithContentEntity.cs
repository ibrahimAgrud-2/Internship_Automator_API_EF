using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Message
{
    [Table("vw_MessagesWithContent")]
    public class vw_MessagesWithContent
    {
        [Key]
        public int MessageID { get; set; }

        public string PersonFullName { get; set; }

        public int ConversationID { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsStaff { get; set; }

        public int ReferencedContentID { get; set; }

        public short ReferencedContentType { get; set; }

        public string? ReferencedContentTitle { get; set; }

        public vw_MessagesWithContent(int messageID,string personFullName, int conversationID, string content, DateTime createdDate, bool isStaff, int referencedContentID, short referencedContentType, string? referencedContentTitle)
        {
            MessageID = messageID;
            PersonFullName = personFullName;
            ConversationID = conversationID;
            Content = content;
            CreatedDate = createdDate;
            IsStaff = isStaff;
            ReferencedContentID = referencedContentID;
            ReferencedContentType = referencedContentType;
            ReferencedContentTitle = referencedContentTitle;
        }
    }
}