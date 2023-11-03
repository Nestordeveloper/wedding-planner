#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wedding_planner.Models;
public class Invitation
{
    [Key]
    public int InvitationId { get; set; }

    public AttendanceStatus Attendance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Foreign Keys
    public int UserId { get; set; }
    public int WeddingId { get; set; }

    public User? User { get; set; }
    public Wedding? Wedding { get; set; }
}

public enum AttendanceStatus
{
    Aceptada,
    Rechazada,
    Pendiente
}
