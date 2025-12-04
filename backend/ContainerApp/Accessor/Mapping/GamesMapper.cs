using Accessor.Models.Games;
using Accessor.Models.Games.Responses;

namespace Accessor.Mapping;

/// <summary>
/// Provides mapping methods between internal DTOs and API Response DTOs for Games domain
/// Follows the pattern: Service returns DB models/internal DTOs → Endpoint maps to Response DTOs
/// </summary>
public static class GamesMapper
{
    #region SubmitAttempt Mappings

    /// <summary>
    /// Maps DB GameAttempt to API SubmitAttemptResponse
    /// </summary>
    public static SubmitAttemptResponse ToSubmitAttemptResponse(this GameAttempt dbModel)
    {
        return new SubmitAttemptResponse
        {
            AttemptId = dbModel.AttemptId,
            ExerciseId = dbModel.ExerciseId,
            StudentId = dbModel.StudentId,
            GameType = dbModel.GameType,
            Difficulty = dbModel.Difficulty,
            Status = dbModel.Status,
            CorrectAnswer = dbModel.CorrectAnswer,
            AttemptNumber = dbModel.AttemptNumber,
            Accuracy = dbModel.Accuracy
        };
    }

    #endregion

    #region GetHistory Mappings

    /// <summary>
    /// Maps internal GameHistoryDto to API GetHistoryResponse
    /// </summary>
    public static GetHistoryResponse ToGetHistoryResponse(this GameHistoryDto internalDto)
    {
        if (internalDto.IsSummary && internalDto.Summary is not null)
        {
            return new GetHistoryResponse
            {
                Summary = new PagedResponseResult<SummaryHistoryResponseDto>
                {
                    Items = internalDto.Summary.Items.Select(ToSummaryHistoryResponseDto).ToList(),
                    Page = internalDto.Summary.Page,
                    PageSize = internalDto.Summary.PageSize,
                    TotalCount = internalDto.Summary.TotalCount
                }
            };
        }

        if (internalDto.IsDetailed && internalDto.Detailed is not null)
        {
            return new GetHistoryResponse
            {
                Detailed = new PagedResponseResult<AttemptHistoryResponseDto>
                {
                    Items = internalDto.Detailed.Items.Select(ToAttemptHistoryResponseDto).ToList(),
                    Page = internalDto.Detailed.Page,
                    PageSize = internalDto.Detailed.PageSize,
                    TotalCount = internalDto.Detailed.TotalCount
                }
            };
        }

        // Default to empty detailed
        return new GetHistoryResponse
        {
            Detailed = new PagedResponseResult<AttemptHistoryResponseDto>
            {
                Items = Array.Empty<AttemptHistoryResponseDto>(),
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            }
        };
    }

    /// <summary>
    /// Maps internal AttemptHistoryDto to API AttemptHistoryResponseDto
    /// </summary>
    private static AttemptHistoryResponseDto ToAttemptHistoryResponseDto(this AttemptHistoryDto internalDto)
    {
        return new AttemptHistoryResponseDto
        {
            ExerciseId = internalDto.ExerciseId,
            AttemptId = internalDto.AttemptId,
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            GivenAnswer = internalDto.GivenAnswer,
            CorrectAnswer = internalDto.CorrectAnswer,
            Status = internalDto.Status,
            Accuracy = internalDto.Accuracy,
            CreatedAt = internalDto.CreatedAt
        };
    }

    /// <summary>
    /// Maps internal SummaryHistoryDto to API SummaryHistoryResponseDto
    /// </summary>
    private static SummaryHistoryResponseDto ToSummaryHistoryResponseDto(this SummaryHistoryDto internalDto)
    {
        return new SummaryHistoryResponseDto
        {
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            AttemptsCount = internalDto.AttemptsCount,
            TotalSuccesses = internalDto.TotalSuccesses,
            TotalFailures = internalDto.TotalFailures
        };
    }

    #endregion

    #region GetAttemptDetails Mappings

    /// <summary>
    /// Maps internal AttemptHistoryDto to API GetAttemptDetailsResponse
    /// </summary>
    public static GetAttemptDetailsResponse ToGetAttemptDetailsResponse(this AttemptHistoryDto internalDto)
    {
        return new GetAttemptDetailsResponse
        {
            ExerciseId = internalDto.ExerciseId,
            AttemptId = internalDto.AttemptId,
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            GivenAnswer = internalDto.GivenAnswer,
            CorrectAnswer = internalDto.CorrectAnswer,
            Status = internalDto.Status,
            Accuracy = internalDto.Accuracy,
            CreatedAt = internalDto.CreatedAt
        };
    }

    #endregion

    #region GetLastAttempt Mappings

    /// <summary>
    /// Maps internal AttemptHistoryDto to API GetLastAttemptResponse
    /// </summary>
    public static GetLastAttemptResponse ToGetLastAttemptResponse(this AttemptHistoryDto internalDto)
    {
        return new GetLastAttemptResponse
        {
            ExerciseId = internalDto.ExerciseId,
            AttemptId = internalDto.AttemptId,
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            GivenAnswer = internalDto.GivenAnswer,
            CorrectAnswer = internalDto.CorrectAnswer,
            Status = internalDto.Status,
            Accuracy = internalDto.Accuracy,
            CreatedAt = internalDto.CreatedAt
        };
    }

    #endregion

    #region GetMistakes Mappings

    /// <summary>
    /// Maps internal PagedResult of MistakeDto to API GetMistakesResponse
    /// </summary>
    public static GetMistakesResponse ToGetMistakesResponse(this PagedResult<MistakeDto> internalDto)
    {
        return new GetMistakesResponse
        {
            Items = internalDto.Items.Select(ToMistakeResponseDto).ToList(),
            Page = internalDto.Page,
            PageSize = internalDto.PageSize,
            TotalCount = internalDto.TotalCount
        };
    }

    /// <summary>
    /// Maps internal MistakeDto to API MistakeResponseDto
    /// </summary>
    private static MistakeResponseDto ToMistakeResponseDto(this MistakeDto internalDto)
    {
        return new MistakeResponseDto
        {
            ExerciseId = internalDto.ExerciseId,
            AttemptId = internalDto.AttemptId,
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            CorrectAnswer = internalDto.CorrectAnswer,
            Mistakes = internalDto.Mistakes.Select(m => new MistakeAttemptResponseDto
            {
                AttemptId = m.AttemptId,
                WrongAnswer = m.WrongAnswer,
                Accuracy = m.Accuracy,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }

    #endregion

    #region GetAllHistories Mappings

    /// <summary>
    /// Maps internal PagedResult of SummaryHistoryWithStudentDto to API GetAllHistoriesResponse
    /// </summary>
    public static GetAllHistoriesResponse ToGetAllHistoriesResponse(this PagedResult<SummaryHistoryWithStudentDto> internalDto)
    {
        return new GetAllHistoriesResponse
        {
            Items = internalDto.Items.Select(ToSummaryHistoryWithStudentResponseDto).ToList(),
            Page = internalDto.Page,
            PageSize = internalDto.PageSize,
            TotalCount = internalDto.TotalCount
        };
    }

    /// <summary>
    /// Maps internal SummaryHistoryWithStudentDto to API SummaryHistoryWithStudentResponseDto
    /// </summary>
    private static SummaryHistoryWithStudentResponseDto ToSummaryHistoryWithStudentResponseDto(this SummaryHistoryWithStudentDto internalDto)
    {
        return new SummaryHistoryWithStudentResponseDto
        {
            StudentId = internalDto.StudentId,
            GameType = internalDto.GameType,
            Difficulty = internalDto.Difficulty,
            AttemptsCount = internalDto.AttemptsCount,
            TotalSuccesses = internalDto.TotalSuccesses,
            TotalFailures = internalDto.TotalFailures,
            StudentFirstName = internalDto.StudentFirstName,
            StudentLastName = internalDto.StudentLastName,
            Timestamp = internalDto.Timestamp
        };
    }

    #endregion

    #region SaveGeneratedSentences Mappings

    /// <summary>
    /// Maps internal list of GeneratedSentenceResultDto to API SaveGeneratedSentencesResponse
    /// </summary>
    public static SaveGeneratedSentencesResponse ToSaveGeneratedSentencesResponse(this IEnumerable<GeneratedSentenceResultDto> internalDtos)
    {
        return new SaveGeneratedSentencesResponse
        {
            Sentences = internalDtos.ToList()
        };
    }

    #endregion
}
