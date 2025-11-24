using Manager.Models.Users;
using Manager.Services.Clients.Accessor.Models.Users;

namespace Manager.Mapping;

public static class UsersMapper
{
    public static CreateUserAccessorRequest ToAccessor(this UserModel request)
    {
        return new CreateUserAccessorRequest
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

    public static CreateUserResponse ToFront(this CreateUserAccessorRequest request)
    {
        return new CreateUserResponse
        {
            UserId = request.UserId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role
        };
    }

    public static UpdateUserAccessorRequest ToAccessor(this UpdateUserRequest request)
    {
        return new UpdateUserAccessorRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PreferredLanguageCode = request.PreferredLanguageCode,
            HebrewLevelValue = request.HebrewLevelValue,
            Role = request.Role,
            Interests = request.Interests,
            AcsUserId = request.AcsUserId,
            AvatarPath = request.AvatarPath,
            AvatarContentType = request.AvatarContentType,
            ClearAvatar = request.ClearAvatar
        };
    }

    public static GetUserResponse ToFront(this GetUserAccessorResponse response)
    {
        return new GetUserResponse
        {
            UserId = response.UserId,
            Email = response.Email,
            FirstName = response.FirstName,
            LastName = response.LastName,
            Role = response.Role,
            PreferredLanguageCode = response.PreferredLanguageCode,
            HebrewLevelValue = response.HebrewLevelValue,
            Interests = response.Interests,
            AcsUserId = response.AcsUserId,
            AvatarPath = response.AvatarPath,
            AvatarContentType = response.AvatarContentType,
            AvatarUpdatedAtUtc = response.AvatarUpdatedAtUtc
        };
    }

    public static IEnumerable<GetUserResponse> ToFront(this IEnumerable<GetUserAccessorResponse> responses)
    {
        return responses.Select(ToFront);
    }

    public static GetUsersForCallerAccessorRequest ToAccessor(this CallerContextDto context)
    {
        return new GetUsersForCallerAccessorRequest
        {
            CallerRole = context.CallerRole,
            CallerId = context.CallerId
        };
    }

    public static AssignStudentAccessorRequest ToAccessorAssign(this TeacherStudentMapDto map)
    {
        return new AssignStudentAccessorRequest
        {
            TeacherId = map.TeacherId,
            StudentId = map.StudentId
        };
    }

    public static UnassignStudentAccessorRequest ToAccessorUnassign(this TeacherStudentMapDto map)
    {
        return new UnassignStudentAccessorRequest
        {
            TeacherId = map.TeacherId,
            StudentId = map.StudentId
        };
    }

    public static UpdateUserLanguageAccessorRequest ToAccessor(this UpdateUserLanguageRequest request, Guid userId)
    {
        return new UpdateUserLanguageAccessorRequest
        {
            UserId = userId,
            Language = request.PreferredLanguage
        };
    }
}
