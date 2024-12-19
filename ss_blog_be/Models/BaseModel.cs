namespace ss_blog_be.Models
{
    public abstract class BaseModel 
    {
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
