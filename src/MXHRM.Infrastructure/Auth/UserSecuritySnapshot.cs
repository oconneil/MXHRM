namespace MXHRM.Infrastructure.Auth;

// snapshot เล็กๆ ของ user ที่ใช้ตรวจ token — เก็บใน Redis แทนการ query DB ทุก request
public sealed record UserSecuritySnapshot(string SecurityStamp, bool IsActive);

public static class AuthCacheKeys
{
    // key เดียวกันต้องใช้ทั้งตอนอ่าน (Program.cs) และตอนลบ (UserService) → รวมไว้ที่เดียวกันความผิดพลาด
    public static string UserSecurity(string userId) => $"auth-stamp:{userId}";
}