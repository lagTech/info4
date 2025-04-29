using Microsoft.EntityFrameworkCore;

using MVC.Models;

namespace MVC.Data
{
    public abstract class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _context;

        protected EFRepository(TContext context) 
        { 
            _context = context; 

        }

        //Post
        public abstract Task<List<Post>> GetPostsIndex(int pageNumber, int pageSize);
        public virtual async Task<int> GetPostsCount() 
        { 
            return await _context.Set<Post>().CountAsync(); 
        }
        public virtual async Task Add(Post post) 
        { 
            _context.Add(post);
            await _context.SaveChangesAsync();
        }
        public virtual async Task IncrementPostLike(Guid id) 
        {
            var post = await _context.Set<Post>().FindAsync(id);
            if (post != null)
            {
                post!.IncrementLike();
                await _context.SaveChangesAsync();
            }
        }
        public virtual async Task IncrementPostDislike(Guid id) 
        {
            var post = await _context.Set<Post>().FindAsync(id);
            if (post != null)
            {
                post!.IncrementDislike();
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task ApprovePost(Guid id, Boolean approved, string Uri)
        {
            var post = await _context.Set<Post>().FindAsync(id);
            if (post != null)
            {
                post!.IsApproved = approved;
                post!.Url = Uri;
                await _context.SaveChangesAsync();
            }
        }

        //Comments
        public virtual async Task<List<Comment>> GetCommentsIndex(Guid id) 
        { 
            return await _context.Set<Comment>().Where(w => w.PostId == id).OrderBy(o => o.Created).ToListAsync(); 
        }
        public virtual async Task AddComments(Comment comment) 
        {
            var post = await _context.Set<Post>().FindAsync(comment.PostId);
            if (post != null)
            {
                _context.Add(comment);
                //post!.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }
        }
        public virtual async Task IncrementCommentLike(Guid id) 
        {
            var comment = await _context.Set<Comment>().FindAsync(id);
            if (comment != null)
            {
                comment!.IncrementLike();
                await _context.SaveChangesAsync();
            }
        }
        public virtual async Task IncrementCommentDislike(Guid id) 
        {
            var comment = await _context.Set<Comment>().FindAsync(id);
            if (comment != null)
            {
                comment!.IncrementDislike();
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task ApproveComment(Guid id, Boolean approved)
        {
            try
            {
                var comment = await _context.Set<Comment>().FindAsync(id);
                if (comment != null)
                {
                    comment!.IsApproved = approved;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }
    }
}
