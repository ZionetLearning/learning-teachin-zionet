using Manager.Models;
using Manager.Models.Games;
using Manager.Services.Clients.Accessor.Models.Games;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for Games domain
/// </summary>
public static class GamesMapper
{
    #region SubmitAttempt Mappings

    /// <summary>
    /// Maps frontend SubmitAttemptRequest to Accessor request
    /// </summary>
    public static SubmitAttemptAccessorRequest ToAccessor(this SubmitAttemptRequest request, Guid studentId)
    {
        return new SubmitAttemptAccessorRequest
        {
            StudentId = studentId,
            ExerciseId = request.ExerciseId,
            GivenAnswer = request.GivenAnswer
        };
    }

    /// <summary>
    /// Maps Accessor response to frontend SubmitAttemptResponse
    /// </summary>
    public static SubmitAttemptResponse ToApiModel(this SubmitAttemptAccessorResponse accessorResponse)
    {
        return new SubmitAttemptResponse
        {
            AttemptId = accessorResponse.AttemptId,
            ExerciseId = accessorResponse.ExerciseId,
            StudentId = accessorResponse.StudentId,
            GameType = accessorResponse.GameType.ToString(),
            Difficulty = accessorResponse.Difficulty,
            Status = accessorResponse.Status,
            CorrectAnswer = accessorResponse.CorrectAnswer,
            AttemptNumber = accessorResponse.AttemptNumber,
            Accuracy = accessorResponse.Accuracy
        };
    }

    #endregion

    #region GetHistory Mappings

    /// <summary>
    /// Maps Accessor GetHistoryAccessorResponse to frontend GetHistoryResponse
    /// </summary>
    public static GetHistoryResponse ToApiModel(this GetHistoryAccessorResponse accessorResponse)
    {
        return new GetHistoryResponse
        {
            Summary = accessorResponse.Summary,
            Detailed = accessorResponse.Detailed
        };
    }

    #endregion

    #region GetMistakes Mappings

    /// <summary>
    /// Maps Accessor GetMistakesAccessorResponse to frontend GetMistakesResponse
    /// </summary>
    public static GetMistakesResponse ToApiModel(this GetMistakesAccessorResponse accessorResponse)
    {
        return new GetMistakesResponse
        {
            Items = accessorResponse.Items,
            Page = accessorResponse.Page,
            PageSize = accessorResponse.PageSize,
            TotalCount = accessorResponse.TotalCount
        };
    }

    /// <summary>
    /// Maps PagedResult to GetMistakesResponse
    /// </summary>
    public static GetMistakesResponse ToApiModel(this PagedResult<ExerciseMistakes> pagedResult)
    {
        return new GetMistakesResponse
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    #endregion

    #region GetAllHistories Mappings

    /// <summary>
    /// Maps Accessor GetAllHistoriesAccessorResponse to frontend GetAllHistoriesResponse
    /// </summary>
    public static GetAllHistoriesResponse ToApiModel(this GetAllHistoriesAccessorResponse accessorResponse)
    {
        return new GetAllHistoriesResponse
        {
            Items = accessorResponse.Items,
            Page = accessorResponse.Page,
            PageSize = accessorResponse.PageSize,
            TotalCount = accessorResponse.TotalCount
        };
    }

    /// <summary>
    /// Maps PagedResult to GetAllHistoriesResponse
    /// </summary>
    public static GetAllHistoriesResponse ToApiModel(this PagedResult<StudentExerciseHistory> pagedResult)
    {
        return new GetAllHistoriesResponse
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    #endregion

    #region Helper Mappings for Accessor Communication

    /// <summary>
    /// Maps PagedResult to Accessor response format for GetMistakes
    /// </summary>
    public static GetMistakesAccessorResponse ToAccessor(this PagedResult<ExerciseMistakes> pagedResult)
    {
        return new GetMistakesAccessorResponse
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    /// <summary>
    /// Maps PagedResult to Accessor response format for GetAllHistories
    /// </summary>
    public static GetAllHistoriesAccessorResponse ToAccessor(this PagedResult<StudentExerciseHistory> pagedResult)
    {
        return new GetAllHistoriesAccessorResponse
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    #endregion
}
