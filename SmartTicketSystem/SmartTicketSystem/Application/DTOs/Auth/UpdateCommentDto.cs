using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Application.DTOs
{
    public class UpdateCommentDto
    {
        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public bool IsInternal { get; set; }
    }
}