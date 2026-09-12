using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PortfolioAdmin.Api.Data;

namespace PortfolioAdmin.Api.Attributes;

/// <summary>
/// 权限校验特性：打在 Controller 或 Action 上，要求用户拥有指定权限码
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
{
    private readonly string _permissionCode;

    /// <summary>
    /// 确保在 AuthorizeFilter 之后执行（AuthorizeFilter 默认 order=0）
    /// </summary>
    public int Order => 100;

    public RequirePermissionAttribute(string permissionCode)
    {
        _permissionCode = permissionCode;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var username = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine($"[RequirePermission] 拒绝：未认证用户 → {_permissionCode}");
            context.Result = new UnauthorizedResult();
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<PortfolioDbContext>();

        var hasPermission = await db.Users
            .Where(u => u.Username == username)
            .Join(db.RoleMenus, u => u.RoleId, rm => rm.RoleId, (u, rm) => rm.MenuId)
            .Join(db.Menus.Where(m => m.PermissionCode == _permissionCode),
                  menuId => menuId, m => m.Id, (menuId, m) => m)
            .AnyAsync();

        if (hasPermission)
        {
            Console.WriteLine($"[RequirePermission] 放行：{username} → {_permissionCode}");
        }
        else
        {
            Console.WriteLine($"[RequirePermission] 拒绝：{username} 无权限 → {_permissionCode}");
            context.Result = new ObjectResult(new { message = $"无操作权限：{_permissionCode}", code = 403 }) { StatusCode = 403 };
        }
    }
}
