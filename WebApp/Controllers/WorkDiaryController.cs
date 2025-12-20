using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Services.WorkDailyService;
using WebApp.Services.WorkDailyService.Dto;
using WebApp.Utils;

namespace WebApp.Controllers;

[ApiController, Route("/api/work-diary")] [Authorize]
public class WorkDiaryController(IWorkDiaryAppService service) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of work diaries based on the provided query parameters.
    /// </summary>
    /// <param name="query">An instance of <see cref="DiaryQueryDto"/> containing filtering, paging, and sorting criteria for querying diaries.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with the list of diaries wrapped in a successful response.</returns>
    [HttpGet]
    public async Task<IActionResult> FindDiaries([FromQuery] DiaryQueryDto query)
       => (await service.FindDiaries(query)).ToActionResult();

    /// <summary>
    /// Creates a new work diary entry using the provided details.
    /// </summary>
    /// <param name="input">An instance of <see cref="DiaryDto"/> containing the details of the diary to be created, including contents, subject, and optional organization ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with the created diary entry wrapped in a successful response.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateDiary([FromBody] DiaryDto input)
      =>  (await service.CreateDiary(input)).ToActionResult();

    /// <summary>
    /// Updates the content and subject of an existing work diary entry based on the provided input.
    /// </summary>
    /// <param name="input">An instance of <see cref="DiaryEditDto"/> containing the ID of the diary to be updated, as well as the new subject and content details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the success or failure of the update operation.</returns>
    [HttpPut]
    public async Task<IActionResult> EditDiary([FromBody] DiaryEditDto input)
        => (await service.EditDiaryContent(input)).ToActionResult();

    /// <summary>
    /// Updates the status of an existing work diary based on the provided input.
    /// </summary>
    /// <param name="input">An instance of <see cref="DiaryUpdateStatusDto"/> containing the diary ID and the new work status to be applied.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the result of the status update operation.</returns>
    [HttpPut("status")]
    public async Task<IActionResult> UpdateDiaryStatus([FromBody] DiaryUpdateStatusDto input)
        => (await service.UpdateDiaryStatus(input)).ToActionResult();

    /// <summary>
    /// Retrieves a list of comments associated with a specific work diary.
    /// </summary>
    /// <param name="workDiaryId">The unique identifier of the work diary for which the comments are to be fetched.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with the list of comments wrapped in a successful response.</returns>
    [HttpGet("comment/{workDiaryId}")]
    public async Task<IActionResult> GetComment(string workDiaryId)
    {
        return (await service.GetComments(workDiaryId)).ToActionResult();
    }
    
    /// <summary>
    /// Adds a new comment to a specific work diary, allowing users to provide feedback or replies.
    /// </summary>
    /// <param name="input">An instance of <see cref="CommentDto"/> containing the comment's details, including content, the ID of the associated work diary, and an optional reply ID for replying to an existing comment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with the result of the comment creation process, wrapped in a successful response if completed successfully.</returns>
    [HttpPost("comment")]
    public async Task<IActionResult> AddComment(CommentDto input)
        => (await service.CreateComment(input)).ToActionResult();

    /// <summary>
    /// Updates the content of an existing comment.
    /// </summary>
    /// <param name="input">An instance of <see cref="EditCommentDto"/> containing the updated comment content and its identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the success or failure of the comment update.</returns>
    [HttpPut("comment")]
    public async Task<IActionResult> EditComment(EditCommentDto input)
    {
        var result = await service.EditComment(input);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a comment based on the provided details.
    /// </summary>
    /// <param name="input">An instance of <see cref="DeleteCommentDto"/> containing the ID of the comment to delete and a flag indicating whether the deletion is permanent.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains an <see cref="IActionResult"/> 
    /// indicating the success or failure of the delete operation.
    /// </returns>
    [HttpDelete("comment")]
    public async Task<IActionResult> DeleteComment(DeleteCommentDto input)
    {
        var result = await service.DeleteComment(input);
        return result.ToActionResult();
    }

    /// <summary>
    /// Physically remove a diary entry from database. <br/>
    /// This action is permanent and cannot be undone and only available to administrators.
    /// </summary>
    /// <param name="id">The identifier of the entry to be removed.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains an <see cref="IActionResult"/>
    /// indicating the success or failure of the delete operation
    /// </returns>
    [HttpDelete("remove/{id}")]
    [HasAuthority(Permissions.Admin)]
    public async Task<IActionResult> HardRemoveDiary(string id)
    {
        var result = await service.RemoveDiary(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Allow user to delete their own diary entry<br/>
    /// This action is a soft delete and the entry can be restored later by administrators.
    /// </summary>
    /// <param name="id">The identifier of the entry to be deleted.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains an <see cref="IActionResult"/>
    /// indicating the success or failure of the delete operation
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteDiary(string id)
    {
        var result = await service.DeleteDiary(id);
        return result.ToActionResult();
    }

}