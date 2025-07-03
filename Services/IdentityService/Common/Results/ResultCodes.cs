namespace IdentityService.Common.Results;

public static class ResultCodes
{
    public static class Tenant
    {
        // Identity 100******
        // RegisterTenant Failed 100 001 001
        public static string RegisterTenantRoleCreateFailed = "100001001";
        public static string RegisterTenantRoleGrantFailed = "100001002";
        public static string RegisterTenantException = "100001003";
        // RegisterTenant Success 100 001 999
        public static string RegisterTenantSuccess = "100001999";
    }

    public static class Token
    {
        // Generate token failed 100 002 001
        public static string VerifyTokenGenerateFailed = "100002001";
        public static string VerifyTokenProcessFinished = "100002998";
        public static string VerifyTokenGenerateSuccess = "100002999";
    }

    public static class User
    {
        // user faild 100 003 001
        public static string UserNotFound = "100003001";
        
        public static string UserGetByIdSuccess = "100003999";
    }

    public static string InternalError = "900900001";
}