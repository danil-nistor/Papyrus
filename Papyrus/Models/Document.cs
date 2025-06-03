namespace Papyrus.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public int StatusId { get; set; }
        public virtual Status Status { get; set; }
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public decimal FileSize { get; set; }


        public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
