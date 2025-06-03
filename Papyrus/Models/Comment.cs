namespace Papyrus.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
