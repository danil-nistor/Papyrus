namespace Papyrus.Models
{
    public class Route
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
        public int Order { get; set; }
        public string ApproverId { get; set; }
        public virtual ApplicationUser Approver { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        }
}
