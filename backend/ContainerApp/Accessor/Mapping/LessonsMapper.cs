using Accessor.Models.Lessons;
using Accessor.Models.Lessons.Requests;
using Accessor.Models.Lessons.Responses;

namespace Accessor.Mapping;

/// <summary>
/// Provides mapping methods between API DTOs and DB models for Lessons domain
/// </summary>
public static class LessonsMapper
{
    #region GetLesson Mappings

    /// <summary>
    /// Maps DB Lesson to API LessonResponse
    /// </summary>
    public static LessonResponse ToResponse(this Lesson dbModel)
    {
        return new LessonResponse
        {
            LessonId = dbModel.LessonId,
            Title = dbModel.Title,
            Description = dbModel.Description,
            ContentSections = dbModel.ContentSections.Select(cs => new ContentSectionDto
            {
                Heading = cs.Heading,
                Body = cs.Body
            }).ToList(),
            TeacherId = dbModel.TeacherId,
            CreatedAt = dbModel.CreatedAt,
            ModifiedAt = dbModel.ModifiedAt
        };
    }

    /// <summary>
    /// Maps DB Lesson to API LessonSummaryResponse (without full content)
    /// </summary>
    public static LessonSummaryResponse ToSummaryResponse(this Lesson dbModel)
    {
        return new LessonSummaryResponse
        {
            LessonId = dbModel.LessonId,
            Title = dbModel.Title,
            Description = dbModel.Description,
            TeacherId = dbModel.TeacherId,
            CreatedAt = dbModel.CreatedAt,
            ModifiedAt = dbModel.ModifiedAt
        };
    }

    #endregion

    #region GetLessons Mappings

    /// <summary>
    /// Maps list of DB Lesson to list of LessonResponse
    /// </summary>
    public static List<LessonResponse> ToResponseList(this IEnumerable<Lesson> dbModels)
    {
        return dbModels.Select(l => l.ToResponse()).ToList();
    }

    /// <summary>
    /// Maps list of DB Lesson to list of LessonSummaryResponse
    /// </summary>
    public static List<LessonSummaryResponse> ToSummaryResponseList(this IEnumerable<Lesson> dbModels)
    {
        return dbModels.Select(l => l.ToSummaryResponse()).ToList();
    }

    #endregion

    #region CreateLesson Mappings

    /// <summary>
    /// Maps API CreateLessonRequest to DB Lesson model
    /// </summary>
    public static Lesson ToDbModel(this CreateLessonRequest request)
    {
        return new Lesson
        {
            LessonId = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ContentSections = request.ContentSections.Select(cs => new ContentSection
            {
                Heading = cs.Heading,
                Body = cs.Body
            }).ToList(),
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region UpdateLesson Mappings

    /// <summary>
    /// Updates existing DB Lesson model with values from UpdateLessonRequest
    /// </summary>
    public static void UpdateFromRequest(this Lesson dbModel, UpdateLessonRequest request)
    {
        dbModel.Title = request.Title;
        dbModel.Description = request.Description;
        dbModel.ContentSections = request.ContentSections.Select(cs => new ContentSection
        {
            Heading = cs.Heading,
            Body = cs.Body
        }).ToList();
        dbModel.ModifiedAt = DateTime.UtcNow;
    }

    #endregion
}

