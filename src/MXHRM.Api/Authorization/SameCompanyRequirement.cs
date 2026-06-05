using Microsoft.AspNetCore.Authorization;

namespace MXHRM.Api.Authorization;

// marker ว่างๆ — แค่บอกว่า "ต้องผ่านการเช็ค same-company"
public sealed class SameCompanyRequirement : IAuthorizationRequirement;