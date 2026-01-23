namespace ss_blog_be.Models
{
    public class TagOcurrencesModel
    {
        public string Term { get; set; }
        public int Ocurrences { get; set; }

    }

    public class TagUpdateModel
    {
        public ICollection<string> tags { get; set; }

    }

    public class TagsModel
    {
        public string[] Content { get; set; }
    }

    public class TagModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public  string PreviousContent { get; set; }
        public int FtsId { get; set; }

        public string[] ToTagsArray()
        {
            return !string.IsNullOrEmpty(Content) ? Content.Split(" ")  : Array.Empty<string>();
        }

    }
}
