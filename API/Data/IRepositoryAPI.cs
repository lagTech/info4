using Microsoft.AspNetCore.Http.HttpResults;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepositoryAPI
    {
        // API, avec implementation des DTO
        Task<Results<Ok<List<PostReadDTO>>, InternalServerError>> GetAPIPostsIndex();

        Task<Results<Ok<PostReadDTO>, NotFound, InternalServerError>> GetAPIPost(Guid id);

        Task<Results<Accepted, InternalServerError>> CreateAPIPost(Post post);

        Task<Results<Accepted, InternalServerError>> APIIncrementPostLike(Guid id);

        Task<Results<Accepted, InternalServerError>> APIIncrementPostDislike(Guid id);

        Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComments();

        Task<Results<Ok<List<CommentReadDTO>>, NotFound, InternalServerError>> GetAPIComment(Guid id);

        Task<Results<Accepted, InternalServerError>> CreateAPIComment(Comment comment);

        Task<Results<Accepted, InternalServerError>> APIIncrementCommentLike(Guid id);

        Task<Results<Accepted, InternalServerError>> APIIncrementCommentDislike(Guid id);


    }
}
