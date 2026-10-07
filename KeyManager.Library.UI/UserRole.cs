using System.Security.Principal;
using System.Security;

namespace Leosac.KeyManager.Library.UI
{
    public enum UserRole
    {
        None,
        User,
        Administrator
    }

    public static class UserRoleContext
    {
        public static bool IsEnabled { get; private set; }
        public static string UserName { get; private set; } = string.Empty;
        public static UserRole Role { get; private set; } = UserRole.User;
        public static bool IsAdministrator => !IsEnabled || Role == UserRole.Administrator;

        public static void Initialize(KMSettings? settings)
        {
            IsEnabled = settings?.UserRolesEnabled == true;
            UserName = WindowsIdentity.GetCurrent().Name;
            Role = UserRole.User;

            if (!IsEnabled)
                return;

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (IsInConfiguredGroup(principal, settings?.AdministratorsGroup))
            {
                Role = UserRole.Administrator;
                return;
            }

            if (IsInConfiguredGroup(principal, settings?.UsersGroup))
            {
                Role = UserRole.User;
                return;
            }

            Role = principal.IsInRole(WindowsBuiltInRole.Administrator)
                ? UserRole.Administrator
                : string.IsNullOrEmpty(settings?.UsersGroup) ? UserRole.User : UserRole.None;
        }

        private static bool IsInConfiguredGroup(WindowsPrincipal principal, string? groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return false;

            try
            {
                return principal.IsInRole(groupName);
            }
            catch (SecurityException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
