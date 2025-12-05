using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.WorkDailyService.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;
using X.PagedList.Extensions;
using Z.EntityFramework.Plus;

namespace WebApp.Services.WorkDailyService;

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

    Task<ResponseEntity> UpdateDiaryStatus(DiaryUpdateStatusDto dto);
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
    Task<ResponseEntity> DeleteComment(DeleteCommentDto dto);
}

public class WorkDiaryAppService(IUserManager userManager,
                                 AppDbContext dbContext) : BaseAppService(userManager), IWorkDiaryAppService
{
    public async Task<ResponseEntity> CreateDiary(DiaryDto dto)
    {
        var diary = new UserWorkDiary
        {
            Content = dto.Contents,
            Subject = dto.Subject,
            UserId = UserId.ToGuid()
        };

        if (dto.OrganizationId is not null)
        {
            var foundOrg = await dbContext.Organizations.AnyAsync(o => o.Id == dto.OrganizationId);
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
        var diary = await dbContext.UserWorkDiaries.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
        if (diary is null) return ResponseEntity.Error404("Diary not found.");
        if (UserId.ToGuid() != diary.UserId) return ResponseEntity.Error400("You are not allowed to edit this diary.");
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
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(diary);
    }

    public async Task<ResponseEntity> FindDiaries(DiaryQueryDto input)
    {
        var query = dbContext.UserWorkDiaries.AsQueryable();
        if (input.Keyword is not null) query = query.Where(x => x.Subject.Contains(input.Keyword));
        if (input.OrganizationId is not null) query = query.Where(x => x.OrganizationId == input.OrganizationId);
        if (input.UserId is not null) query = query.Where(x => x.UserId == input.UserId);

        if (input.FromDate is not null)
        {
            query = query.Where(x => x.CreateAt >= input.FromDate);
        }

        if (input.ToDate is not null)
        {
            query = query.Where(x => x.CreateAt <= input.ToDate);
        }

        if (input.Status.IsNotEmpty())
        {
            query = query.Where(x => input.Status.Contains(x.WorkStatus));
        }

        var result = await query.AsNoTracking()
                                .AsSplitQuery()
                                .Cacheable()
                                .OrderByDescending(x => x.CreateAt)
                                .Select(x => new
                                {
                                    x.Id,
                                    x.Subject, x.WorkStatus, x.CreateAt, x.LastUpdateAt, x.CreateBy,
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
                                .ToPagedListAsync(input.Page, input.PageSize);

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
                              x.Id, x.Subject, x.Content, x.WorkStatus,
                              x.CreateAt, x.LastUpdateAt,
                              UserName = x.User != null ? x.User.Username : null,
                              x.UserId,
                              Comments = x.Comments.Select(c => new
                              {
                                  c.Id,
                                  c.Content,
                                  c.User.FullName,
                                  c.UserId,
                                  c.CreateAt
                              })
                          })
                          .FirstOrDefaultAsync();
        return diary is null ? ResponseEntity.Error404("Diary not found") : ResponseEntity.OkResult(diary);
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
                                          x.CreateAt, x.LastUpdateAt,
                                          x.UserId,
                                          x.User.Username,
                                          x.ReplyId
                                      })
                                      .ToListAsync();
        return ResponseEntity.OkResult(comments);
    }

    public async Task<ResponseEntity> EditComment(EditCommentDto dto)
    {
        var comment = await dbContext.UserWorkDiaryComments.FindAsync(dto.Id);
        if (comment is null)
            return ResponseEntity.Error404("Comment not found.");
        if (UserId.ToGuid() != comment.UserId)
            return ResponseEntity.Error400("You are not allowed to edit this comment.");
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
            return ResponseEntity.Error400("You are not allowed to delete this comment.");
        if (comment.Deleted)
            return ResponseEntity.Error400("Comment already deleted.");
        comment.Deleted = true;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }
}