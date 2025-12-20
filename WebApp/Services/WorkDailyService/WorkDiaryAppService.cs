using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.NotificationService;
using WebApp.Services.UserService;
using WebApp.Services.WorkDailyService.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;
using X.PagedList.Extensions;
using Z.EntityFramework.Plus;

namespace WebApp.Services.WorkDailyService;

/// <summary>
/// Interface defining the application service for managing work diaries and their related operations.
/// </summary>
public interface IWorkDiaryAppService
{
    /// <summary>
    /// Creates a new diary entry in the system with the provided details.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the diary to be created, such as content, subject, and optional organization ID.</param>
    /// <returns>A <see cref="ResponseEntity"/> indicating the result of the operation.</returns>
    Task<ResponseEntity> CreateDiary(DiaryDto dto);

    /// <summary>
    /// Updates the content and subject of an existing diary entry based on the provided details.
    /// </summary>
    /// <param name="dto">The data transfer object containing the ID of the diary to be updated, along with the new subject and content.</param>
    /// <returns>A <see cref="ResponseEntity"/> indicating the outcome of the operation.</returns>
    Task<ResponseEntity> EditDiaryContent(DiaryEditDto dto);

    /// <summary>
    /// Updates the status of an existing diary entry with the specified details.
    /// </summary>
    /// <param name="dto">The data transfer object containing the ID of the diary to be updated and the new work status.</param>
    /// <returns>A <see cref="ResponseEntity"/> indicating the outcome of the update operation.</returns>
    Task<ResponseEntity> UpdateDiaryStatus(DiaryUpdateStatusDto dto);

    /// <summary>
    /// Retrieves a list of diaries based on the provided query criteria.
    /// </summary>
    /// <param name="input">The query parameters, including keywords, date range, pagination details, and optional filters like user ID or organization ID.</param>
    /// <returns>A <see cref="ResponseEntity"/> containing the resulting diaries and associated metadata.</returns>
    Task<ResponseEntity> FindDiaries(DiaryQueryDto input);

    Task<ResponseEntity> GetDiaryById(string id);

    /// <summary>
    /// Creates a new comment associated with a specific work diary.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the comment, including its content, the associated work diary ID, and an optional reply ID for nested comments.</param>
    /// <returns>A <see cref="ResponseEntity"/> representing the outcome of the operation, including the newly created comment details if successful.</returns>
    Task<ResponseEntity> CreateComment(CommentDto dto);

    /// <summary>
    /// Retrieves a list of comments associated with a specific work diary.
    /// </summary>
    /// <param name="workDiaryId">The unique identifier of the work diary for which the comments are to be fetched.</param>
    /// <returns>A <see cref="ResponseEntity"/> containing the list of comments, including their details such as content, creation date, user information, and reply ID.</returns>
    Task<ResponseEntity> GetComments(string workDiaryId);

    Task<ResponseEntity> EditComment(EditCommentDto dto);

    /// <summary>
    /// Soft-deletes a user work diary comment.
    /// </summary>
    /// <param name="dto">A <see cref="DeleteCommentDto"/> containing the identifier of the comment to delete.</param>
    /// <returns>
    /// A <see cref="ResponseEntity"/> describing the result:<br/>
    /// - Returns <c>200 OK</c> when the comment is successfully marked as deleted.<br/>
    /// - Returns <c>401 Unauthorized</c> when the current user is not the comment owner.<br/>
    /// - Returns <c>400 Bad Request</c> when the comment is already deleted.<br/>
    /// - Returns <c>404 Not Found</c> when the comment does not exist.
    /// </returns>
    /// <remarks>
    /// This method performs a soft delete by setting <c>Deleted</c> to <c>true</c> on <see cref="UserWorkDiaryComment"/> and saving the change.
    /// The current user's identity (via the service's <c>UserId</c>) must match the comment's <see cref="UserWorkDiaryComment.UserId"/> to allow deletion.
    /// </remarks>
    Task<ResponseEntity> DeleteComment(DeleteCommentDto dto);

    /// <summary>
    /// Deletes the diary entry with the specified identifier.  
    /// </summary>
    /// <param name="id">The unique identifier of the diary entry to delete. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous delete operation. 
    /// The task result contains a <see cref="ResponseEntity">ResponseEntity</see> indicating
    /// the outcome of the operation.</returns>
    Task<ResponseEntity> DeleteDiary(string id);

    /// <summary>
    /// Removes the diary entry from database with the specified identifier.<br/>
    /// This operation permanently deletes the diary and its associated comments from the database.
    /// </summary>
    /// <remarks>
    /// This operation should be accessed by high-privilege users only, as it permanently deletes data.
    /// </remarks>
    /// <param name="id">The unique identifier of the diary entry to remove. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a ResponseEntity indicating the
    /// outcome of the removal operation.</returns>
    Task<ResponseEntity> RemoveDiary(string id);
}

public class WorkDiaryAppService(IUserManager userManager,
                                 INotificationAppService notificationService,
                                 AppDbContext dbContext) : BaseAppService(userManager), IWorkDiaryAppService
{
    public async Task<ResponseEntity> CreateDiary(DiaryDto dto)
    {
        if (UserId.ToGuid() == Guid.Empty)
            throw new UnauthorizedAccessException(); //Forces the user to be logged in
        var diary = new UserWorkDiary
        {
            Content = dto.Content,
            DueDate = dto.DueDate,
            Subject = dto.Subject,
            Priority = dto.Priority,
            UserId = UserId.ToGuid()
        };

        if (dto.OrganizationId is not null)
        {
            var foundOrg = await dbContext.Organizations.AnyAsync(o => o.Id == dto.OrganizationId && !o.Deleted);
            if (foundOrg)
            {
                diary.OrganizationId = dto.OrganizationId;
            }
        }

        await dbContext.AddAsync(diary);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> EditDiaryContent(DiaryEditDto dto)
    {
        var diary = await dbContext.UserWorkDiaries
                                   .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
        if (diary is null)
            return ResponseEntity.Error404("Diary not found.");

        if (UserId.ToGuid() != diary.UserId)
            return ResponseEntity.Error401("You are not allowed to edit this diary.");

        if (diary.Content.ToMd5() == dto.Contents.ToMd5())
            return ResponseEntity.Ok("No change were made.");

        diary.Content = dto.Contents;
        diary.Subject = dto.Subject;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(diary);
    }

    public async Task<ResponseEntity> UpdateDiaryStatus(DiaryUpdateStatusDto dto)
    {
        var diary = await dbContext.UserWorkDiaries.FindAsync(dto.Id);
        if (diary is null) return ResponseEntity.Error404("Diary not found");
        diary.WorkStatus = dto.WorkStatus;
        diary.DueDate = dto.DueDate;
        diary.Priority = dto.Priority;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(diary);
    }

    public async Task<ResponseEntity> FindDiaries(DiaryQueryDto input)
    {
        var query = dbContext.UserWorkDiaries
                             .Where(x => !x.Deleted)
                             .WhereIf(input.Keyword is not null, x => x.Subject.Contains(input.Keyword!))
                             .WhereIf(input.OrganizationId is not null, x => x.OrganizationId == input.OrganizationId)
                             .WhereIf(input.UserId is not null, x => x.UserId == input.UserId)
                             .WhereIf(input.FromDate is not null, x => x.CreateAt >= input.FromDate!)
                             .WhereIf(input.ToDate is not null, x => x.CreateAt <= input.ToDate!)
                             .WhereIf(input.Status.IsNotEmpty(), x => input.Status.Contains(x.WorkStatus))
                             .AsQueryable();

        var result = await query.AsNoTracking()
                                .AsSplitQuery()
                                .OrderByDescending(x => x.CreateAt)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.Subject,
                                    x.WorkStatus,
                                    x.CreateAt,
                                    x.LastUpdateAt,
                                    x.CreateBy,
                                    x.DueDate,
                                    x.Priority,
                                    User = x.User != null ? x.User.FullName : null,
                                    Organization = x.Organization == null
                                        ? null
                                        : new
                                        {
                                            x.Organization.Id,
                                            x.Organization.ShortName,
                                            x.Organization.TaxId
                                        }
                                })
                                .Cacheable()
                                .ToPagedListAsync(input.Page, input.Size);

        return ResponseEntity.OkResult(result);
    }

    public async Task<ResponseEntity> GetDiaryById(string id)
    {
        var diary = await dbContext
                              .UserWorkDiaries
                              .Where(x => x.Id == id && !x.Deleted)
                              .Include(x => x.Comments.Where(c => !c.Deleted))
                              .Include(x => x.Organization)
                              .Include(x => x.User)
                              .AsSplitQuery()
                              .AsNoTracking()
                              .Select(x => new
                              {
                                  Organization = new
                                  {
                                      Id = x.OrganizationId,
                                      TaxId = x.Organization != null ? x.Organization.TaxId : null,
                                      FullName = x.Organization != null ? x.Organization.FullName : null,
                                      ShortName = x.Organization != null ? x.Organization.ShortName : null
                                  },
                                  x.Id,
                                  x.Subject,
                                  x.Content,
                                  x.WorkStatus,
                                  x.DueDate,
                                  x.Priority,
                                  x.CreateAt,
                                  x.LastUpdateAt,
                                  UserName = x.User != null ? x.User.Username : null,
                                  x.UserId,
                                  Comments = x.Comments.Select(c => new
                                  {
                                      c.Id,
                                      c.Content,
                                      c.User.FullName,
                                      c.UserId,
                                      c.ReplyId,
                                      c.CreateAt
                                  }).OrderBy(c => c.ReplyId).ThenByDescending(c => c.CreateAt).ToList()
                              })
                          .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                          .FirstOrDefaultAsync();
        return diary is null ? ResponseEntity.Error404("Diary not found") : ResponseEntity.OkResult(diary);
    }

    public async Task<ResponseEntity> DeleteDiary(string id)
    {
        var diary = await dbContext.UserWorkDiaries.FindAsync(id);
        if (diary is null) return ResponseEntity.Error404("Record not found.");
        if (UserId.ToGuid() != diary.UserId)
            return ResponseEntity.Error401("You are not allowed to delete this diary.");
        diary.Deleted = true;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> CreateComment(CommentDto dto)
    {
        var diary = await dbContext.UserWorkDiaries.FindAsync(dto.WorkDiaryId);

        if (diary is null) return ResponseEntity.Error404("Record not found.");
        var comment = new UserWorkDiaryComment
        {
            WorkDiaryId = dto.WorkDiaryId,
            Content = dto.Content,
            UserId = UserId.ToGuid(),
            ReplyId = dto.ReplyId
        };

        await dbContext.AddAsync(comment);
        await dbContext.SaveChangesAsync();

        return ResponseEntity.OkResult(comment);
    }

    public async Task<ResponseEntity> GetComments(string workDiaryId)
    {
        var comments = await dbContext.UserWorkDiaryComments
                                      .Where(x => x.WorkDiaryId == workDiaryId
                                                  && !x.Deleted)
                                      .Include(x => x.User)
                                      .OrderByDescending(x => x.CreateAt)
                                      .Select(x => new
                                      {
                                          x.Id,
                                          x.Content,
                                          x.CreateAt,
                                          x.LastUpdateAt,
                                          x.UserId,
                                          x.User.Username,
                                          x.ReplyId
                                      })
                                      .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                      .ToListAsync();
        return ResponseEntity.OkResult(comments);
    }

    public async Task<ResponseEntity> EditComment(EditCommentDto dto)
    {
        var comment = await dbContext.UserWorkDiaryComments.FindAsync(dto.Id);
        if (comment is null)
            return ResponseEntity.Error404("Comment not found.");
        if (UserId.ToGuid() != comment.UserId)
            return ResponseEntity.Error401("You are not allowed to edit this comment.");
        if (comment.Content.ToMd5() == dto.Content.ToMd5())
            return ResponseEntity.Ok("No change were made.");
        comment.Content = dto.Content;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(comment.Content);
    }

    public async Task<ResponseEntity> DeleteComment(DeleteCommentDto dto)
    {
        var comment = await dbContext.UserWorkDiaryComments.FindAsync(dto.Id);
        if (comment is null)
            return ResponseEntity.Error404("Comment not found.");
        if (UserId.ToGuid() != comment.UserId)
            return ResponseEntity.Error401("You are not allowed to delete this comment.");
        if (comment.Deleted)
            return ResponseEntity.Error400("Comment already deleted.");
        comment.Deleted = true;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> RemoveDiary(string id)
    {
        var diary = await dbContext.UserWorkDiaries.Include(x => x.Comments)
                                                   .FirstOrDefaultAsync(x => x.Id == id);
        if (diary is null)
        {
            return ResponseEntity.Error404("Record not found.");
        }
        if (diary.Comments.IsNotEmpty())
        {
            dbContext.UserWorkDiaryComments.RemoveRange(diary.Comments);
        }
        dbContext.UserWorkDiaries.Remove(diary);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok($"Đã xóa bản ghi số: {id}");
    }
}