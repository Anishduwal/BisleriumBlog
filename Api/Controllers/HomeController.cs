using BisleriumBlog.Application.Features.Response.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Metadata;
using BisleriumBlog.Application.Models;
using BisleriumBlog.Infrastructure.Interfaces.Services;
using BisleriumBlog.Application.Features.Response.Home;
using BisleriumBlog.Application.Features.Request.Reaction;
using Microsoft.EntityFrameworkCore;
using BisleriumBlog.Infrastructure.Persistence;
using System.Linq;

namespace BisleriumBlog.Api.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : Controller
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _DBContext;

    public HomeController(IUserService userService, ApplicationDbContext dbContext)
    {
        _userService = userService;
        _DBContext = dbContext;
    }
    /*This method retrieves blog posts for the home page, applies sorting and pagination, and returns a list of blog details 
   along with their reactions, comments, and other related information*/

    [HttpGet("home-page-blogs")]
    public IActionResult GetHomePageBlogs(int pageNumber, int pageSize, string? sortBy = null)
    {
        var userId = _userService.UserId;
        var user = _DBContext.Users.Find(userId);

        var blogs = _DBContext.Blogs.Where(x => x.IsActive).ToArray();

        var blogPostDetails = new List<BlogPostDetailsResponse>();

        foreach (var blog in blogs)
        {
            var reactions = _DBContext.Reactions.Where(x => x.BlogId == blog.Id && x.IsReactedForBlog && x.IsActive).ToArray();
            var comments = _DBContext.Comments.Where(x => x.BlogId == blog.Id && x.IsActive).ToArray();

            var upVotes = reactions.Count(x => x.ReactionId == 1);   // Count of Up Vote reactions
            var downVotes = reactions.Count(x => x.ReactionId == 2); // Count of Down Vote reactions

            var commentForComments = comments.Where(x =>
                comments.Select(z => z.CommentId).Contains(x.CommentId) && x.IsCommentForComment).ToArray(); // Details of comment replies

            var popularity = upVotes * 2 - downVotes * 1 + comments.Length + commentForComments.Length;

            var blogUser = _DBContext.Users.Find(blog.CreatedById);

            var blogPostDetail = new BlogPostDetailsResponse
            {
                BlogId = blog.Id,
                Title = blog.Title,
                BloggerName = blogUser?.FullName ?? "Unknown",
                BloggerImage = string.IsNullOrEmpty(blogUser?.ImagePath) ? "Sample.svg" : blogUser.ImagePath,
                Address = blog.Location,
                Reaction = blog.Reaction,
                Body = blog.Body,
                UpVotes = upVotes,
                DownVotes = downVotes,
                IsUpVotedByUser = user != null && reactions.Any(reaction => reaction.ReactionId == 1 && reaction.CreatedById == user.Id),
                IsDownVotedByUser = user != null && reactions.Any(reaction => reaction.ReactionId == 2 && reaction.CreatedById == user.Id),
                IsUpdated = blog.LastUpdatedDate.HasValue,
                CreatedDate = blog.CreatedDate,
                PopularityPoints = popularity,
                Images = _DBContext.BlogImages.Where(image => image.BlogId == blog.Id && image.IsActive).Select(image => image.ImagePath).ToList(),
                UploadedDuration = GetUploadedTimePeriod(blog.CreatedDate),
                CommentCount = commentForComments.Length,
                Comments = RetrievePostComments(blog.Id, user)
            };

            blogPostDetails.Add(blogPostDetail);
        }

        // Sorting the blog posts based on the provided sorting criteria
        blogPostDetails = sortBy switch
        {
            null or "Recency" => blogPostDetails.OrderByDescending(blog => blog.CreatedDate).ToList(),
            "Popularity" => blogPostDetails.OrderByDescending(blog => blog.PopularityPoints).ToList(),
            _ => blogPostDetails.OrderBy(blog => Guid.NewGuid()).ToList() // Random shuffle for default case
        };

        // Paginate the results
        var paginatedBlogs = blogPostDetails.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        // Construct the response model
        var result = new ResponseModel<List<BlogPostDetailsResponse>>
        {
            Message = "Success",
            Result = paginatedBlogs,
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = blogPostDetails.Count
        };

        return Ok(result);
    }

    [Authorize]
    [HttpGet("my-blogs")]
    public IActionResult GetUserBlogs(int pageNumber, int pageSize, string? sortBy = null)
    {
        var userId = _userService.UserId;
        var user = _DBContext.Users.Find(userId);

        var blogs = _DBContext.Blogs.Where(x => x.IsActive && x.CreatedById == userId).ToArray();


        // Fetch all relevant users in one query
        var blogIds = blogs.Select(b => b.Id).ToList();

        // Collect user IDs from blogs
        var userIdsFromBlogs = blogs
            .Select(b => b.CreatedById)
            .ToHashSet();

        // Collect user IDs from comments related to these blogs
        var userIdsFromComments = _DBContext.Comments
            .Where(c => c.IsActive && c.BlogId.HasValue && blogIds.Contains(c.BlogId.Value)) // Ensure BlogId is not null and convert to int
            .Select(c => c.CreatedById)  // No need to check for nulls if CreatedBy is int
            .ToHashSet();

        // Combine both sets of user IDs
        var userIds = userIdsFromBlogs.Union(userIdsFromComments).ToHashSet();


        var users = _DBContext.Users.Where(u => userIds.Contains(u.Id))
                                    .ToDictionary(u => u.Id, u => u);

        var blogPostDetails = new List<BlogPostDetailsResponse>();

        foreach (var blog in blogs)
        {
            var reactions = _DBContext.Reactions.Where(x => x.BlogId == blog.Id && x.IsReactedForBlog && x.IsActive).ToArray();
            var comments = _DBContext.Comments.Where(x => x.BlogId == blog.Id && x.IsActive).ToArray();

            var upVotes = reactions.Count(x => x.ReactionId == 1);   // Count of Up Vote reactions
            var downVotes = reactions.Count(x => x.ReactionId == 2); // Count of Down Vote reactions

            var commentForComments = comments.Where(x => x.IsCommentForComment).ToArray(); // Details of comment replies

            var popularity = upVotes * 2 - downVotes + comments.Length + commentForComments.Length;

            var blogUser = users.GetValueOrDefault(blog.CreatedById);

            var blogPostDetail = new BlogPostDetailsResponse
            {
                BlogId = blog.Id,
                Title = blog.Title,
                Body = blog.Body,
                BloggerName = blogUser?.FullName ?? "Unknown",
                BloggerImage = string.IsNullOrEmpty(blogUser?.ImagePath) ? "Sample.svg" : blogUser.ImagePath,
                Address = blog.Location,
                Reaction = blog.Reaction,
                UpVotes = upVotes,
                DownVotes = downVotes,
                IsUpVotedByUser = reactions.Any(x => x.ReactionId == 1 && x.CreatedById == userId),
                IsDownVotedByUser = reactions.Any(x => x.ReactionId == 2 && x.CreatedById == userId),
                IsUpdated = blog.LastUpdatedDate.HasValue,
                CreatedDate = blog.CreatedDate,
                PopularityPoints = popularity,
                Images = _DBContext.BlogImages.Where(x => x.BlogId == blog.Id && x.IsActive).Select(x => x.ImagePath).ToList(),
                UploadedDuration = (DateTime.Now - blog.CreatedDate).TotalMinutes < 1 ? $"{(int)(DateTime.Now - blog.CreatedDate).TotalSeconds} seconds ago" :
                    (DateTime.Now - blog.CreatedDate).TotalHours < 1 ? $"{(int)(DateTime.Now - blog.CreatedDate).TotalMinutes} minutes ago" :
                    (DateTime.Now - blog.CreatedDate).TotalHours < 24 ? $"{(int)(DateTime.Now - blog.CreatedDate).TotalHours} hours ago" :
                    blog.CreatedDate.ToString("dd-MM-yyyy HH:mm"),
                CommentCount = commentForComments.Length,
                Comments = comments.Where(x => x.IsCommentForBlog).Select(x => new PostComments()
                {
                    Comment = x.Message,
                    UpVotes = _DBContext.Reactions.Count(z => z.IsReactedForComment && z.ReactionId == 1 && z.CommentId == x.Id),
                    DownVotes = _DBContext.Reactions.Count(z => z.IsReactedForComment && z.ReactionId == 2 && z.CommentId == x.Id),
                    IsUpVotedByUser = _DBContext.Reactions.Any(z => z.IsReactedForComment && z.ReactionId == 1 && z.CommentId == x.Id && z.CreatedById == userId),
                    IsDownVotedByUser = _DBContext.Reactions.Any(z => z.IsReactedForComment && z.ReactionId == 2 && z.CommentId == x.Id && z.CreatedById == userId),
                    CommentId = x.Id,
                    CommentedBy = users.GetValueOrDefault(x.CreatedById)?.FullName ?? "Unknown",
                    ImagePath = users.GetValueOrDefault(x.CreatedById)?.ImagePath ?? "Sample.svg",
                    IsUpdated = x.LastUpdatedDate.HasValue,
                    CommentedDuration = (DateTime.Now - x.CreatedDate).TotalMinutes < 1 ? $"{(int)(DateTime.Now - x.CreatedDate).TotalSeconds} seconds ago" :
                        (DateTime.Now - x.CreatedDate).TotalHours < 1 ? $"{(int)(DateTime.Now - x.CreatedDate).TotalMinutes} minutes ago" :
                        (DateTime.Now - x.CreatedDate).TotalHours < 24 ? $"{(int)(DateTime.Now - x.CreatedDate).TotalHours} hours ago" :
                        x.CreatedDate.ToString("dd-MM-yyyy HH:mm"),
                }).Take(1).ToList()
            };

            blogPostDetails.Add(blogPostDetail);
        }

        // Sorting the blog posts based on the provided sorting criteria
        blogPostDetails = sortBy switch
        {
            null => blogPostDetails.OrderByDescending(x => x.CreatedDate).ToList(),
            "Popularity" => blogPostDetails.OrderByDescending(x => x.PopularityPoints).ToList(),
            "Recency" => blogPostDetails.OrderByDescending(x => x.CreatedDate).ToList(),
            _ => blogPostDetails.OrderBy(x => Guid.NewGuid()).ToList() // Random shuffle for default case
        };

        // Paginate the results
        var paginatedBlogs = blogPostDetails.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        // Construct the response model
        var result = new ResponseModel<List<BlogPostDetailsResponse>>
        {
            Message = "Success",
            Result = paginatedBlogs,
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = blogs.Length
        };

        return Ok(result);
    }


    [HttpGet("blogs-details/{blogId:int}")]
    public IActionResult GetBlogDetails(int blogId)
    {
        var userId = _userService.UserId;
        var user = _DBContext.Users.Find(userId);

        var blog = _DBContext.Blogs.Find(blogId);
        if (blog == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Blog not found",
                Status = "Error",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Result = null
            });
        }

        var reactions = _DBContext.Reactions.Where(x => x.BlogId == blog.Id && x.IsReactedForBlog && x.IsActive).ToArray();
        var comments = _DBContext.Comments.Where(x => x.BlogId == blog.Id && x.IsActive).ToArray();

        var upVotes = reactions.Count(x => x.ReactionId == 1);
        var downVotes = reactions.Count(x => x.ReactionId == 2);

        var commentForComments = comments.Where(x => x.IsCommentForComment).ToArray(); // Details of comment replies

        var popularity = upVotes * 2 - downVotes + comments.Length + commentForComments.Length;

        var blogUser = _DBContext.Users.Find(blog.CreatedById);

        var blogDetails = new BlogPostDetailsResponse
        {
            BlogId = blog.Id,
            Title = blog.Title,
            Body = blog.Body,
            BloggerName = blogUser?.FullName ?? "Unknown",
            BloggerImage = blogUser?.ImagePath ?? "Sample.svg",
            Address = blog.Location,
            Reaction = blog.Reaction,
            UpVotes = upVotes,
            DownVotes = downVotes,
            IsUpVotedByUser = user != null && reactions.Any(x => x.ReactionId == 1 && x.CreatedById == userId),
            IsDownVotedByUser = user != null && reactions.Any(x => x.ReactionId == 2 && x.CreatedById == userId),
            IsUpdated = blog.LastUpdatedDate.HasValue,
            CreatedDate = blog.CreatedDate,
            PopularityPoints = popularity,
            Images = _DBContext.BlogImages.Where(x => x.BlogId == blog.Id && x.IsActive).Select(x => x.ImagePath).ToList(),
            CommentCount = commentForComments.Length,
            UploadedDuration = GetUploadedTimePeriod(blog.CreatedDate),
            Comments = GetCommentsRecursive(false, true, blog.Id)
        };

        var result = new ResponseModel<BlogPostDetailsResponse>
        {
            Message = "Success",
            Result = blogDetails,
            Status = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 1
        };

        return Ok(result);
    }


    [Authorize]
    [HttpPost("upvote-downvote-blog")]
    public IActionResult UpVoteDownVoteBlog(ReactionActionRequest reactionModel)
    {
        var userId = _userService.UserId;
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            return Unauthorized(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.Unauthorized,
                TotalCount = 0,
                Status = "Unauthorized",
                Result = false
            });
        }

        var blog = _DBContext.Blogs.Find(reactionModel.BlogID);

        if (blog == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Blog not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Not Found",
                Result = false
            });
        }

        var existingReactions = _DBContext.Reactions
            .Where(x => x.CreatedById == user.Id && x.IsReactedForBlog && x.IsActive)
            .ToArray();

        if (existingReactions.Any())
        {
            _DBContext.Reactions.RemoveRange(existingReactions);
            _DBContext.SaveChanges();
        }

        var reaction = new Reaction
        {
            ReactionId = reactionModel.ReactionID ?? 0,
            BlogId = blog.Id,
            CommentId = null,
            IsReactedForBlog = true,
            IsReactedForComment = false,
            CreatedDate = DateTime.Now,
            CreatedById = user.Id,
            IsActive = true,
        };

        _DBContext.Reactions.Add(reaction);
        _DBContext.SaveChanges();

        return Ok(new ResponseModel<object>
        {
            Message = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 0,
            Status = "Success",
            Result = true
        });
    }

    [Authorize]
    [HttpPost("upvote-downvote-comment")]
    public IActionResult UpVoteDownVoteComment(ReactionActionRequest reactionModel)
    {
        var userId = _userService.UserId;
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            return Unauthorized(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.Unauthorized,
                TotalCount = 0,
                Status = "Unauthorized",
                Result = false
            });
        }

        var comment = _DBContext.Comments.Find(reactionModel.CommentID);

        if (comment == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Comment not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Not Found",
                Result = false
            });
        }

        var existingReactions = _DBContext.Reactions
            .Where(x => x.CreatedById == user.Id && x.CommentId == comment.Id && x.IsReactedForComment && x.IsActive)
            .ToArray();

        if (existingReactions.Any())
        {
            _DBContext.Reactions.RemoveRange(existingReactions);
            _DBContext.SaveChanges();
        }

        var reaction = new Reaction
        {
            ReactionId = reactionModel.ReactionID ?? 0,
            BlogId = null,
            CommentId = comment.Id,
            IsReactedForBlog = false,
            IsReactedForComment = true,
            CreatedDate = DateTime.Now,
            CreatedById = user.Id,
            IsActive = true,
        };

        _DBContext.Reactions.Add(reaction);
        _DBContext.SaveChanges();

        return Ok(new ResponseModel<object>
        {
            Message = "Success",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 0,
            Status = "Success",
            Result = true
        });
    }

    [Authorize]
    [HttpPost("add-blog-comment")]
    public IActionResult AddBlogComment(ReactionActionRequest commentRequest)
    {
        // Retrieve the user ID from the user service
        var userId = _userService.UserId;

        // Fetch the user from the database context
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            return Unauthorized(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.Unauthorized,
                TotalCount = 0,
                Status = "Unauthorized",
                Result = false
            });
        }

        // Fetch the blog post from the database context
        var blog = _DBContext.Blogs.Find(commentRequest.BlogID);

        if (blog == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Blog post not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Not Found",
                Result = false
            });
        }

        // Create a new comment for the blog post
        var comment = new Comment
        {
            BlogId = blog.Id,
            CommentId = null,
            Message = commentRequest.Comment ?? string.Empty,
            IsCommentForBlog = true,
            IsCommentForComment = false,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedById = user.Id,
        };

        // Add the new comment to the database context
        _DBContext.Comments.Add(comment);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Comment added successfully",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 0,
            Status = "Success",
            Result = true
        });
    }
    [Authorize]
    [HttpPost("add-comment-reply")]
    public IActionResult AddCommentReply(ReactionActionRequest commentRequest)
    {
        // Retrieve the user ID from the user service
        var userId = _userService.UserId;

        // Fetch the user from the database context
        var user = _DBContext.Users.Find(userId);

        if (user == null)
        {
            return Unauthorized(new ResponseModel<object>
            {
                Message = "User not found",
                StatusCode = HttpStatusCode.Unauthorized,
                TotalCount = 0,
                Status = "Unauthorized",
                Result = false
            });
        }

        // Fetch the parent comment from the database context
        var parentComment = _DBContext.Comments.Find(commentRequest.CommentID);

        if (parentComment == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Parent comment not found",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Not Found",
                Result = false
            });
        }

        // Create a new reply comment for the parent comment
        var replyComment = new Comment
        {
            BlogId = null, // Assuming this is a reply to a comment, so BlogId is not set
            CommentId = parentComment.Id,
            Message = commentRequest.Comment ?? string.Empty,
            IsCommentForBlog = false,
            IsCommentForComment = true,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedById = user.Id,
        };

        // Add the new reply comment to the database context
        _DBContext.Comments.Add(replyComment);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Reply added successfully",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 0,
            Status = "Success",
            Result = true
        });
    }


    [Authorize]
    [HttpDelete("remove-comment/{commentId:int}")]
    public IActionResult RemoveComment(int commentId)
    {
        // Retrieve the comment by its ID
        var comment = _DBContext.Comments.Find(commentId);

        if (comment == null)
        {
            // If the comment does not exist, return a not found response
            return NotFound(new ResponseModel<object>
            {
                Message = "Comment not found.",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Mark the comment as inactive to indicate it has been "deleted"
        comment.IsActive = false;

        // Update the comment in the database context
        _DBContext.Comments.Update(comment);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Comment successfully removed.",
            StatusCode = HttpStatusCode.OK,
            TotalCount = 0,
            Status = "Success",
            Result = true
        });
    }


    [Authorize]
    [HttpDelete("remove-blog-reactions/{blogId:int}")]
    public IActionResult DeleteBlogReactions(int blogId)
    {
        // Fetch the blog by its ID
        var blog = _DBContext.Blogs.Find(blogId);

        if (blog == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Blog not found.",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Retrieve all reactions for the specified blog
        var reactions = _DBContext.Reactions
                                   .Where(r => r.BlogId == blog.Id && r.IsReactedForBlog)
                                   .ToList();

        // Mark all reactions as inactive
        foreach (var reaction in reactions)
        {
            reaction.IsActive = false;
        }

        // Update reactions in the database context
        _DBContext.Reactions.UpdateRange(reactions);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Reactions successfully removed.",
            StatusCode = HttpStatusCode.OK,
            TotalCount = reactions.Count,
            Status = "Success",
            Result = true
        });
    }
    [Authorize]
    [HttpDelete("remove-comment-reactions/{commentId:int}")]
    public IActionResult DeleteCommentReactions(int commentId)
    {
        // Fetch the comment by its ID
        var comment = _DBContext.Comments.Find(commentId);

        if (comment == null)
        {
            return NotFound(new ResponseModel<object>
            {
                Message = "Comment not found.",
                StatusCode = HttpStatusCode.NotFound,
                TotalCount = 0,
                Status = "Failure",
                Result = false
            });
        }

        // Retrieve all reactions for the specified comment
        var reactions = _DBContext.Reactions
                                  .Where(r => r.CommentId == comment.Id && r.IsReactedForComment)
                                  .ToList();

        // Mark all reactions as inactive
        foreach (var reaction in reactions)
        {
            reaction.IsActive = false;
        }

        // Update reactions in the database context
        _DBContext.Reactions.UpdateRange(reactions);

        // Save changes to the database
        _DBContext.SaveChanges();

        // Return a success response
        return Ok(new ResponseModel<object>
        {
            Message = "Comment reactions successfully removed.",
            StatusCode = HttpStatusCode.OK,
            TotalCount = reactions.Count,
            Status = "Success",
            Result = true
        });
    }

    private string GetUploadedTimePeriod(DateTime createdDate)
    {
        var timeDifference = DateTime.Now - createdDate;
        if (timeDifference.TotalMinutes < 1)
        {
            return $"{(int)timeDifference.TotalSeconds} seconds ago";
        }
        else if (timeDifference.TotalHours < 1)
        {
            return $"{(int)timeDifference.TotalMinutes} minutes ago";
        }
        else if (timeDifference.TotalHours < 24)
        {
            return $"{(int)timeDifference.TotalHours} hours ago";
        }
        return createdDate.ToString("dd-MM-yyyy HH:mm");
    }
private List<PostComments> RetrievePostComments(int blogId, User currentUser)
{
    var comments = _DBContext.Comments
        .Where(comment => comment.BlogId == blogId && comment.IsActive && comment.IsCommentForBlog)
        .Select(comment => new PostComments
        {
            Comment = comment.Message,
            UpVotes = _DBContext.Reactions
                .Count(reaction => reaction.IsReactedForComment && reaction.IsActive && reaction.ReactionId == 1 && reaction.CommentId == comment.Id),
            DownVotes = _DBContext.Reactions
                .Count(reaction => reaction.IsReactedForComment && reaction.IsActive && reaction.ReactionId == 2 && reaction.CommentId == comment.Id),
            IsUpVotedByUser = currentUser != null && _DBContext.Reactions
                .Any(reaction => reaction.IsReactedForComment && reaction.IsActive && reaction.ReactionId == 1 && reaction.CreatedById == currentUser.Id && reaction.CommentId == comment.Id),
            IsDownVotedByUser = currentUser != null && _DBContext.Reactions
                .Any(reaction => reaction.IsReactedForComment && reaction.IsActive && reaction.ReactionId == 2 && reaction.CreatedById == currentUser.Id && reaction.CommentId == comment.Id),
            CommentId = comment.Id,
            CommentedBy = _DBContext.Users
                .Where(user => user.Id == comment.CreatedById)
                .Select(user => user.FullName)
                .FirstOrDefault(),
            ImagePath = _DBContext.Users
                .Where(user => user.Id == comment.CreatedById)
                .Select(user => user.ImagePath ?? "Sample.svg")
                .FirstOrDefault(),
            IsUpdated = comment.LastUpdatedDate.HasValue,
            CommentedDuration = GetUploadedTimePeriod(comment.CreatedDate)
        })
        .Take(1)
        .ToList();

    return comments;
}

    [NonAction]
    private List<PostComments> GetCommentsRecursive(bool isForComment, bool isForBlog, int? blogId = null, int? parentId = null)
    {
        // Retrieve the user ID from the user service
        var userId = _userService.UserId;

        // Fetch the user from the database context
        var user = _DBContext.Users.Find(userId);

        // Query for comments based on the provided parameters
        var comments = _DBContext.Comments
            .Where(x => x.BlogId == blogId &&
                        x.IsActive &&
                        x.IsCommentForBlog == isForBlog &&
                        x.IsCommentForComment == isForComment &&
                        x.CommentId == parentId)
            .Select(x => new PostComments
            {
                Comment = x.Message,
                UpVotes = _DBContext.Reactions.Count(z => z.IsReactedForComment && z.ReactionId == 1 && z.CommentId == x.Id),
                DownVotes = _DBContext.Reactions.Count(z => z.IsReactedForComment && z.ReactionId == 2 && z.CommentId == x.Id),
                IsUpVotedByUser = user != null && _DBContext.Reactions.Any(z => z.IsReactedForComment && z.ReactionId == 1 && z.CreatedById == user.Id && z.CommentId == x.Id),
                IsDownVotedByUser = user != null && _DBContext.Reactions.Any(z => z.IsReactedForComment && z.ReactionId == 2 && z.CreatedById == user.Id && z.CommentId == x.Id),
                CommentId = x.Id,
                CommentedBy = _DBContext.Users.Where(u => u.Id == x.CreatedById).Select(u => u.FullName).FirstOrDefault(),
                ImagePath = _DBContext.Users.Where(u => u.Id == x.CreatedById).Select(u => u.ImagePath).FirstOrDefault() ?? "sample-profile.png",
                IsUpdated = x.LastUpdatedDate != null,
                CommentedDuration = DateTime.Now.Hour - x.CreatedDate.Hour < 24
                    ? $"{(int)(DateTime.Now - x.CreatedDate).TotalHours} hours ago"
                    : x.CreatedDate.ToString("dd-MM-yyyy HH:mm"),
                Comments = GetCommentsRecursive(true, false, null, x.Id)
            })
            .ToList();

        return comments;
    }
}
