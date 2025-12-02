using Manager.Models.Tasks;
using Manager.Models.Tasks.Requests;
using Manager.Models.Tasks.Responses;
using Manager.Services.Clients.Accessor.Models.Tasks;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for Tasks domain
/// </summary>
public static class TasksMapper
{
    #region GetTask Mappings

    /// <summary>
    /// Maps Accessor response to frontend GetTaskResponse
    /// </summary>
    public static GetTaskResponse ToApiModel(this GetTaskAccessorResponse accessorResponse)
    {
        return new GetTaskResponse
        {
            Id = accessorResponse.Id,
            Name = accessorResponse.Name,
            Payload = accessorResponse.Payload
        };
    }

    #endregion

    #region GetTasks Mappings

    /// <summary>
    /// Maps Accessor response to frontend tasks list
    /// </summary>
    public static IReadOnlyList<TaskSummaryDto> ToApiModel(this GetTasksAccessorResponse accessorResponse)
    {
        return accessorResponse.Tasks.Select(t => new TaskSummaryDto
        {
            Id = t.Id,
            Name = t.Name
        }).ToList();
    }

    #endregion

    #region CreateTask Mappings

    /// <summary>
    /// Maps frontend CreateTaskRequest to Accessor request
    /// </summary>
    public static CreateTaskAccessorRequest ToAccessor(this CreateTaskRequest request)
    {
        return new CreateTaskAccessorRequest
        {
            Id = request.Id,
            Name = request.Name,
            Payload = request.Payload
        };
    }

    /// <summary>
    /// Maps Accessor response to frontend CreateTaskResponse
    /// </summary>
    public static CreateTaskResponse ToApiModel(this CreateTaskAccessorResponse accessorResponse)
    {
        return new CreateTaskResponse
        {
            Id = accessorResponse.Id,
            Status = accessorResponse.Message
        };
    }

    #endregion

    #region UpdateTaskName Mappings

    /// <summary>
    /// Maps frontend UpdateTaskNameRequest to Accessor request
    /// </summary>
    public static UpdateTaskNameAccessorRequest ToAccessor(this UpdateTaskNameRequest request, int id)
    {
        return new UpdateTaskNameAccessorRequest
        {
            Id = id,
            Name = request.Name
        };
    }

    /// <summary>
    /// Maps Accessor response to frontend UpdateTaskNameResponse
    /// </summary>
    public static UpdateTaskNameResponse ToApiModel(this UpdateTaskNameAccessorResponse accessorResponse)
    {
        return new UpdateTaskNameResponse
        {
            Message = "Task name updated"
        };
    }

    #endregion
}
