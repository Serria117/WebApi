using Microsoft.AspNetCore.Authorization;

namespace WebApp.Authentication;

/// <summary>
/// Specifies that the user must have a specific permission to access a particular resource or action.
/// </summary>
/// <remarks>
/// The attribute enforces an authorization policy that requires the authenticated user to possess
/// the specified permission. If the user does not have the required permission, access to the resource
/// or action is denied. It integrates into the authorization pipeline by applying a policy matching
/// the specified permission string.
/// </remarks>
/// <param name="permission">
/// The specific permission required to access the target resource or action. For example, a permission
/// might correspond to a specific operation such as "ORG.CREATE" or "ORG.UPDATE".
/// </param>
public sealed class HasAuthorityAttribute(string permission)
    : AuthorizeAttribute(policy: permission);

/// <summary>
/// Specifies that the user must have all of the specified permissions to access a particular resource or action.
/// </summary>
/// <remarks>
/// The attribute verifies that the authenticated user possesses all the provided permissions.
/// If any one of the permissions is missing, access is denied. It modifies the authorization
/// pipeline by enforcing a policy in the format "HasAll:{permission1},{permission2},...".
/// </remarks>
/// <param name="permissions">
/// A variable-length array of permissions that the user must have. Each permission corresponds
/// to a specific action or role required for access.
/// </param>
/// <example>
/// This attribute can be used in controllers or action methods to restrict access, for example,
/// requiring multiple permissions such as "USER.CREATE" and "SYSTEM.ADMIN".
/// </example>
public sealed class HasAllAuthoritiesAttribute(params string[] permissions)
    : AuthorizeAttribute(policy: $"HasAll:{string.Join(",", permissions)}") { }

/// <summary>
/// Specifies that the user must have at least one of the specified permissions to access a particular resource or action.
/// </summary>
/// <remarks>
/// The attribute verifies that the authenticated user has at least one of the permissions listed.
/// If none of the provided permissions are present, access is denied. It modifies the authorization
/// pipeline by enforcing a policy in the format "HasAny:{permission1},{permission2},...".
/// </remarks>
/// <param name="permissions">
/// A variable-length array of permissions that the user can possess. At least one of these
/// permissions is required to gain access.
/// </param>
/// <example>
/// This attribute is useful when access to a resource or action should be allowed if the user has any one
/// of the specified permissions, such as "USER.VIEW" or "USER.SELF".
/// </example>
public sealed class HasAnyAuthorityAttribute(params string[] permissions)
    : AuthorizeAttribute(policy: $"HasAny:{string.Join(",", permissions)}") { }