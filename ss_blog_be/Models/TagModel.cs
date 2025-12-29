namespace ss_blog_be.Models
{
    public class TagModel
    {
        public string Term { get; set; }
        public int Ocurrences { get; set; }

    }

    public class TagUpdateModel
    {
        public ICollection<string> tags { get; set; }

    }
}
