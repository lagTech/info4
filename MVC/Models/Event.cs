namespace MVC.Models
{
    [Serializable]
    public class Event
    {
        public ItemType ItemType { get; set; }
        public Action Action { get; set; }
        public Post? Post { get; set; }
        public Comment? Comment { get; set; }
        public Guid? Id { get; set; }
        public string? Uri { get; set; }

        public Event()
        { }

        public Event(ItemType itemType, Action action, Post? post, Comment? comment, Guid? id, string? uri)
        { 
            ItemType = itemType;
            Action = action;
            Post = post;
            Comment = comment;
            Id = id;
            Uri = uri;
        }

        public Event(Post post) : this(ItemType.Post, Action.Create, post, null, null, null) { }

        public Event(Comment comment) : this(ItemType.Comment, Action.Create, null, comment, null, null) { }

        public Event(ItemType itemType, Action action, Guid id) : this(itemType, action, null, null, id, null) { }

        public Event(ItemType itemType, Action action, Guid id, string uri) : this(itemType, action, null, null, id, uri) { }
    }

    public enum ItemType
    { 
        Post = 0,
        Comment = 1,
    }

    public enum Action
    { 
        Create,
        Like,
        Dislike,
        Approve,
        Rejected,
    }
}
