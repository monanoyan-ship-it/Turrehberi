using ErkanTatilPlani.API.Controllers;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.API.Factories.Auth;

internal static class AuthUserInfoHelper
{
    internal static UserInfo BuildUserInfo(Visitor visitor, bool includeProfileFields = false)
    {
        var info = new UserInfo
        {
            Id = visitor.Id,
            FirstName = visitor.FirstName,
            LastName = visitor.LastName,
            Email = visitor.Email,
            UserTypeId = visitor.UserTypeId,
            UserTypeName = UserTypes.GetById(visitor.UserTypeId)?.SystemName ?? "Unknown",
            CompanyId = visitor.CompanyId,
            CompanyName = visitor.Company?.Name,
            CompanyStatusId = visitor.Company?.StatusId,
            CompanyStatusName = visitor.Company != null ? CompanyStatuses.GetById(visitor.Company.StatusId)?.SystemName : null,
            PreferredLanguage = visitor.PreferredLanguage
        };

        if (includeProfileFields)
        {
            info.Phone = visitor.Phone;
            info.Address = visitor.Address;
            info.BirthDate = visitor.BirthDate;
            info.AvatarUrl = visitor.AvatarUrl;
            info.EmailVerified = visitor.EmailVerified;
            info.NotificationPreference = visitor.NotificationPreference;
        }

        return info;
    }

    internal static LoginResponse BuildLoginResponse(string token, Visitor visitor)
    {
        return new LoginResponse
        {
            Token = token,
            User = BuildUserInfo(visitor)
        };
    }
}
