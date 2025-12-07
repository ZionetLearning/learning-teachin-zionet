using Accessor.Models.Users;
using Accessor.Models.Users.Requests;
using Accessor.Models.Users.Responses;

namespace Accessor.Mapping;

/// <summary>
/// Provides mapping methods between API DTOs and DB models for Users domain
/// </summary>
public static class UsersMapper
{
    #region CreateUser Mappings

    /// <summary>
    /// Maps API CreateUserRequest to DB UserModel
    /// </summary>
    public static UserModel ToDbModel(this CreateUserRequest request)
    {
        return new UserModel
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role,
            PreferredLanguageCode = request.PreferredLanguageCode,
            HebrewLevelValue = request.HebrewLevelValue,
            Interests = request.Interests
        };
    }

    /// <summary>
    /// Maps DB UserModel to API CreateUserResponse
    /// </summary>
    public static CreateUserResponse ToCreateResponse(this UserModel dbModel)
    {
        return new CreateUserResponse
        {
            UserId = dbModel.UserId,
            Email = dbModel.Email,
            FirstName = dbModel.FirstName,
            LastName = dbModel.LastName,
            Role = dbModel.Role,
            PreferredLanguageCode = dbModel.PreferredLanguageCode,
            HebrewLevelValue = dbModel.HebrewLevelValue,
            Interests = dbModel.Interests,
            AcsUserId = dbModel.AcsUserId
        };
    }

    #endregion

    #region GetUser Mappings

    /// <summary>
    /// Maps DB UserData to API GetUserResponse
    /// </summary>
    public static GetUserResponse ToResponse(this UserData dbModel)
    {
        return new GetUserResponse
        {
            UserId = dbModel.UserId,
            Email = dbModel.Email,
            FirstName = dbModel.FirstName,
            LastName = dbModel.LastName,
            Role = dbModel.Role,
            PreferredLanguageCode = dbModel.PreferredLanguageCode,
            HebrewLevelValue = dbModel.HebrewLevelValue,
            Interests = dbModel.Interests,
            AcsUserId = dbModel.AcsUserId,
            AvatarPath = dbModel.AvatarPath,
            AvatarContentType = dbModel.AvatarContentType,
            AvatarUpdatedAtUtc = dbModel.AvatarUpdatedAtUtc
        };
    }

    #endregion

    #region GetAllUsers / List Mappings

    /// <summary>
    /// Maps list of DB UserData to list of GetUserResponse (Manager expects List directly)
    /// </summary>
    public static List<GetUserResponse> ToResponseList(this IEnumerable<UserData> dbModels)
    {
        return dbModels.Select(u => u.ToResponse()).ToList();
    }

    #endregion

    #region UpdateUser Mappings

    /// <summary>
    /// Maps API UpdateUserRequest to DB UpdateUserModel
    /// </summary>
    public static UpdateUserModel ToDbModel(this UpdateUserRequest request)
    {
        return new UpdateUserModel
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PreferredLanguageCode = request.PreferredLanguageCode,
            HebrewLevelValue = request.HebrewLevelValue,
            Role = request.Role,
            Interests = request.Interests,
            AvatarPath = request.AvatarPath,
            AvatarContentType = request.AvatarContentType,
            ClearAvatar = request.ClearAvatar,
            AcsUserId = request.AcsUserId
        };
    }

    #endregion

    #region UpdateUserLanguage Mappings

    #endregion

    #region GetUserInterests Mappings

    /// <summary>
    /// Maps interests list to GetUserInterestsResponse
    /// </summary>
    public static GetUserInterestsResponse ToInterestsResponse(this IReadOnlyList<string> interests)
    {
        return new GetUserInterestsResponse
        {
            Interests = interests.ToList()
        };
    }

    #endregion
}

