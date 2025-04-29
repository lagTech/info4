using MVC.Models;

namespace MVC.Data
{
    public interface IRepository
    {
        // Post
        Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize);

        Task<int> GetPostsCount();

        abstract Task Add(Post post);

        abstract Task IncrementPostLike(Guid id);

        abstract Task IncrementPostDislike(Guid id);

        abstract Task ApprovePost(Guid id, Boolean approved, string Uri);

        // Comments
        Task<List<Comment>> GetCommentsIndex(Guid id);

        abstract Task AddComments(Comment comment);

        abstract Task IncrementCommentLike(Guid id);

        abstract Task IncrementCommentDislike(Guid id);

        abstract Task ApproveComment(Guid id, Boolean approved);
    }
 }