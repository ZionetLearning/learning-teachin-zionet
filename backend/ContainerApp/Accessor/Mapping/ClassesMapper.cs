using Accessor.Models.Classes;
using Accessor.Models.Classes.Requests;
using Accessor.Models.Classes.Responses;

namespace Accessor.Mapping;

/// <summary>
/// Provides mapping methods between API DTOs and DB models for Classes domain
/// </summary>
public static class ClassesMapper
{
    #region GetClass Mappings

    /// <summary>
    /// Maps DB ClassDto to API GetClassResponse
    /// </summary>
    public static GetClassResponse ToResponse(this ClassDto dbModel)
    {
        return new GetClassResponse
        {
            ClassId = dbModel.ClassId,
            Name = dbModel.Name,
            Members = dbModel.Members.Select(m => new MemberResponseDto
            {
                MemberId = m.MemberId,
                Name = m.Name,
                Role = (int)m.Role
            }).ToList()
        };
    }

    #endregion

    #region GetAllClasses Mappings

    /// <summary>
    /// Maps list of DB ClassDto to list of ClassSummaryDto (Manager expects List directly)
    /// </summary>
    public static List<ClassSummaryDto> ToResponse(this List<ClassDto> dbModels)
    {
        return dbModels.Select(c => new ClassSummaryDto
        {
            ClassId = c.ClassId,
            Name = c.Name,
            Members = c.Members.Select(m => new MemberResponseDto
            {
                MemberId = m.MemberId,
                Name = m.Name,
                Role = (int)m.Role
            }).ToList()
        }).ToList();
    }

    #endregion

    #region GetMyClasses Mappings

    /// <summary>
    /// Maps list of DB ClassDto to list of ClassSummaryDto (Manager expects List directly)
    /// </summary>
    public static List<ClassSummaryDto> ToMyClassesResponse(this List<ClassDto> dbModels)
    {
        return dbModels.Select(c => new ClassSummaryDto
        {
            ClassId = c.ClassId,
            Name = c.Name,
            Members = c.Members.Select(m => new MemberResponseDto
            {
                MemberId = m.MemberId,
                Name = m.Name,
                Role = (int)m.Role
            }).ToList()
        }).ToList();
    }

    #endregion

    #region CreateClass Mappings

    /// <summary>
    /// Maps API CreateClassRequest to DB Class model
    /// </summary>
    public static Class ToDbModel(this CreateClassRequest request)
    {
        return new Class
        {
            ClassId = Guid.NewGuid(),
            Name = request.Name,
            Code = string.Empty,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Maps DB Class model to API CreateClassResponse
    /// </summary>
    public static CreateClassResponse ToResponse(this Class dbModel)
    {
        return new CreateClassResponse
        {
            ClassId = dbModel.ClassId,
            Name = dbModel.Name,
            Code = dbModel.Code ?? string.Empty,
            Description = dbModel.Description,
            CreatedAt = dbModel.CreatedAt
        };
    }

    #endregion
}
