using Microsoft.EntityFrameworkCore;
using MVC.Business;
using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;
        private EventHubController _eventHub;

        protected EFRepository(TContext context, EventHubController eventHub) 
        { 
            _context = context; 
            _eventHub = eventHub;
        }

        //Post
        public abstract Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize);
        public virtual async Task<int> GetPostsCount() { return await _context.Set<Post>().CountAsync(); }
        public virtual async Task Add(Post post) 
        { 
            await _eventHub.SendEvent(new Event(post));

        }
        public virtual async Task IncrementPostLike(Guid id) 
        {
            await _eventHub.SendEvent(new Event(ItemType.Post, Models.Action.Like, id));
        }
        public virtual async Task IncrementPostDislike(Guid id) 
        {
            await _eventHub.SendEvent(new Event(ItemType.Post, Models.Action.Dislike, id));
        }

        //Comments
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) { return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); }
        public virtual async Task AddComments(Comment comment) 
        {
            await _eventHub.SendEvent(new Event(comment));
        }
        public virtual async Task IncrementCommentLike(Guid id) 
        {

            await _eventHub.SendEvent(new Event(ItemType.Comment, Models.Action.Like, id));
        }
        public virtual async Task IncrementCommentDislike(Guid id) 
        {
            await _eventHub.SendEvent(new Event(ItemType.Comment, Models.Action.Dislike, id));
        }
    }
}
