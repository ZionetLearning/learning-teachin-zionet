using Manager.Models.Classes.Requests;
using Manager.Models.Classes.Responses;
using Manager.Services.Clients.Accessor.Models.Classes;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for Classes domain
/// </summary>
public static class ClassesMapper
{
    #region GetClass Mappings

    /// <summary>
    /// Maps Accessor response to frontend GetClassResponse
    /// </summary>
    public static GetClassResponse ToFront(this GetClassAccessorResponse accessorResponse)
    {
        return new GetClassResponse
        {
            ClassId = accessorResponse.ClassId,
            Name = accessorResponse.Name,
            Members = accessorResponse.Members.Select(m => new ClassMemberDto
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
    /// Maps Accessor response to frontend GetAllClassesResponse
    /// </summary>
    public static GetAllClassesResponse ToFront(this GetAllClassesAccessorResponse accessorResponse)
    {
        return new GetAllClassesResponse
        {
            Classes = accessorResponse.Classes.Select(c => new ClassSummaryDto
            {
                ClassId = c.ClassId,
                Name = c.Name,
                Members = c.Members.Select(m => new ClassMemberDto
                {
                    MemberId = m.MemberId,
                    Name = m.Name,
                    Role = (int)m.Role
                }).ToList()
            }).ToList()
        };
    }

    #endregion

    #region GetMyClasses Mappings

    /// <summary>
    /// Maps Accessor response to frontend GetMyClassesResponse
    /// </summary>
    public static GetMyClassesResponse ToFront(this GetMyClassesAccessorResponse accessorResponse)
    {
        return new GetMyClassesResponse
        {
            Classes = accessorResponse.Classes.Select(c => new ClassSummaryDto
            {
                ClassId = c.ClassId,
                Name = c.Name,
                Members = c.Members.Select(m => new ClassMemberDto
                {
                    MemberId = m.MemberId,
                    Name = m.Name,
                    Role = (int)m.Role
                }).ToList()
            }).ToList()
        };
    }

    #endregion

    #region CreateClass Mappings

    /// <summary>
    /// Maps frontend CreateClassRequest to Accessor request
    /// </summary>
    public static CreateClassAccessorRequest ToAccessor(this CreateClassRequest request)
    {
        return new CreateClassAccessorRequest
        {
            Name = request.Name,
            Description = request.Description
        };
    }

    /// <summary>
    /// Maps Accessor response to frontend CreateClassResponse
    /// </summary>
    public static CreateClassResponse ToFront(this CreateClassAccessorResponse accessorResponse)
    {
        return new CreateClassResponse
        {
            ClassId = accessorResponse.ClassId,
            Name = accessorResponse.Name,
            Code = accessorResponse.Code,
            Description = accessorResponse.Description,
            CreatedAt = accessorResponse.CreatedAt
        };
    }

    #endregion

    #region AddMembers Mappings

    /// <summary>
    /// Maps frontend AddMembersRequest to Accessor request
    /// </summary>
    public static AddMembersAccessorRequest ToAccessor(this AddMembersRequest request)
    {
        return new AddMembersAccessorRequest
        {
            UserIds = request.UserIds,
            AddedBy = request.AddedBy
        };
    }

    #endregion

    #region RemoveMembers Mappings

    /// <summary>
    /// Maps frontend RemoveMembersRequest to Accessor request
    /// </summary>
    public static RemoveMembersAccessorRequest ToAccessor(this RemoveMembersRequest request)
    {
        return new RemoveMembersAccessorRequest
        {
            UserIds = request.UserIds
        };
    }

    #endregion
}
