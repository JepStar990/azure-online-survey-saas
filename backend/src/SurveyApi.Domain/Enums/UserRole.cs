namespace SurveyApi.Domain.Enums;

/// <summary>
/// Roles assigned to users within a tenant.
/// Lower numeric values have higher privilege.
/// </summary>
public enum UserRole
{
    /// <summary>Full tenant administration: manage users, billing, settings.</summary>
    TenantAdmin = 0,

    /// <summary>Create and manage surveys.</summary>
    SurveyCreator = 1,

    /// <summary>View surveys and analytics but cannot edit.</summary>
    SurveyViewer = 2,

    /// <summary>External user who only takes surveys.</summary>
    Respondent = 3
}
