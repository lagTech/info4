using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Business;
using Microsoft.Extensions.Hosting;

namespace MVC.Data
{
    public abstract class EFRepositoryAPI<TContext> : IRepositoryAPI where TContext : DbContext
    {
        protected readonly TContext _context;
        private EventHubController _eventController;

        protected EFRepositoryAPI(TContext context, EventHubController eventHubController)
        {
            _context = context;
            _eventController = eventHubController;
        }

        //API
        // Avec l'implementation du DTO
        public virtual async Task<Results<Ok<List<PostReadDTO>>, InternalServerError>> GetAPIPostsIndex()
        {
            try
            {
                // Converstion dans le DTO
                Post[] posts = await _context.Set<Post>().OrderByDescending(o => o.Created).ToArrayAsync();
                List<PostReadDTO> postsDTO = posts.Select(x => new PostReadDTO(x)).ToList();

                return TypedResults.Ok(postsDTO);
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<PostReadDTO>, NotFound, InternalServerError>> GetAPIPost(Guid id)
        {
            try
            {
                var post = await _context.Set<Post>().FirstOrDefaultAsync(w => w.Id == id);
                if (post == null)
                    return TypedResults.NotFound();
                else
                    return TypedResults.Ok(new PostReadDTO(post));
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        // Normallement cette méthode ne devrait pas être Post, cette object est interne, mais nous avons géré la conversion dans une autre méthode interne avant ...
        public virtual async Task<Results<Accepted, InternalServerError>> CreateAPIPost(Post post)
        {
            try
            {
                await _eventController.SendEvent(new Event(post));

                return TypedResults.Accepted($"/Posts/{post.Id}");
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Accepted, InternalServerError>> APIIncrementPostLike(Guid id)
        {
            try
            {
                await _eventController.SendEvent(new Event(ItemType.Post, Models.Action.Like, id));

                return TypedResults.Accepted($"/Posts/{id}");
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Accepted, InternalServerError>> APIIncrementPostDislike(Guid id)
        {
            try
            {
                await _eventController.SendEvent(new Event(ItemType.Post, Models.Action.Dislike, id));

                return TypedResults.Accepted($"/Posts/{id}");
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComments()
        {
            try
            {
                Comment[] comments = await _context.Set<Comment>().OrderByDescending(o => o.Created).ToArrayAsync();
                if (comments.Length > 0)
                {
                    // Converstion dans le DTO
                    List<CommentReadDTO> commentsDTO = comments.Select(x => new CommentReadDTO(x)).ToList();
                    return TypedResults.Ok(commentsDTO);
                }
                return TypedResults.NotFound();
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }



        public virtual async Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComment(Guid id)
        {
            try
            {
                Comment[] comments = await _context.Set<Comment>().Where(x => x.Id == id || x.PostId == id).OrderByDescending(o => o.Created).ToArrayAsync();
                if (comments.Length > 0)
                {
                    // Converstion dans le DTO
                    List<CommentReadDTO> commentsDTO = comments.Select(x => new CommentReadDTO(x)).ToList();
                    return TypedResults.Ok(commentsDTO);
                }  
                return TypedResults.NotFound();
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Accepted, InternalServerError>> CreateAPIComment(Comment comment)
        {
            try
            {
                await _eventController.SendEvent(new Event(comment));

                return TypedResults.Accepted($"/Comments/{comment.Id}");
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Accepted, InternalServerError>> APIIncrementCommentLike(Guid id)
        {
            try
            {
                await _eventController.SendEvent(new Event(ItemType.Comment, Models.Action.Like, id));

                return TypedResults.Accepted($"/Comments/{id}");
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

        public virtual async Task<Results<Accepted, InternalServerError>> APIIncrementCommentDislike(Guid id)
        {
            try
            {
                await _eventController.SendEvent(new Event(ItemType.Comment, Models.Action.Dislike, id));

                return TypedResults.Accepted($"/Comments/{id}");
            }
            catch
            {
                return TypedResults.InternalServerError();
            }
        }

    }
}
