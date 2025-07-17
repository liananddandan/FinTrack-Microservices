namespace IdentityService.Common.Results;

public static class ResultCodes
{
    public static class Tenant
    {
        // Identity 100******
        // RegisterTenant Failed 100 001 001
        public const string RegisterTenantRoleCreateFailed = "100001001";
        public const string RegisterTenantRoleGrantFailed = "100001002";
        public const string RegisterTenantException = "100001003";
        public const string InvitationRecordNotFound = "100001004";
        public const string InvitationPublicIdInvalid = "100001005";
        public const string InvitationVersionInvalid = "100001006";
        public const string InvitationExpired = "100001007";
        public const string InvitationRevoked = "100001008";
        public const string InvitationHasAccepted = "100001009";
        public const string InvitationWithAInvalidTenant = "100001010";
        public const string InvitationCreateFailed = "100001011";
        // RegisterTenant Success 100 001 999
        public const string InvitationReceiveSuccess = "100001993";
        public const string InvitationRecordUpdateSuccess = "100001994";
        public const string InvitationRecordFoundByEmailSuccess = "100001995";
        public const string InvitationRecordFoundByPublicIdSuccess = "100001996";
        public const string InvitationRecordAddSuccess = "100001997";
        public const string InvitationUsersStartSuccess = "100001998";
        public const string RegisterTenantSuccess = "100001999";
    }

    public static class Token
    {
        // Generate token failed 100 002 001
        public const string VerifyTokenGenerateFailed = "100002001";
        public const string VerifyTokenGenerateInvalidTokenType = "100002002";
        public const string VerifyTokenFailed = "100002003";
        public const string RefreshJwtTokenFailedTokenInvalid = "100002004";
        public const string RefreshJwtTokenFailedTokenTypeInvalid = "100002005";
        public const string RefreshJwtTokenFailedClaimMissing = "100002006";
        public const string RefreshJwtTokenFailedClaimUserNotFound = "100002007";
        public const string RefreshJwtTokenFailedClaimTenantIdInvalid = "100002008";
        public const string RefreshJwtTokenFailedVersionInvalid = "100002009";
        public const string RefreshJwtTokenFailedRoleInvalid = "100002010";
        public const string JwtTokenInvalidForParse = "100002011";
        public const string JwtTokenVersionInvalid = "100002012";
        public const string JwtTokenClaimMissing = "100002013";
        public const string JwtTokenParseFailed = "100002014";
        public const string JwtTokenExpired = "100002015";
        
        public const string InvitationTokenParseSuccess = "100002992";
        public const string GenerateInvitationTokenSuccess = "100002993";
        public const string JwtTokenParseSuccess = "100002994";
        public const string RefreshJwtTokenSuccess = "100002995";
        public const string GenerateJwtTokenPairSuccess = "100002996";
        public const string GenerateJwtTokenSuccess = "100002997";
        public const string VerifyTokenSuccess = "100002998";
        public const string VerifyTokenGenerateSuccess = "100002999";
    }

    public static class User
    {
        // user faild 100 003 001
        public const string UserNotFound = "100003001";
        public const string UserEmailVerificationFailed = "100003002";
        public const string UserPublicIdInvalid = "100003003";
        public const string UserEmailNotVerified = "100003004";
        public const string UserEmailOrPasswordInvalid = "100003005";
        public const string UserCouldNotFindRole = "100003006";
        public const string UserSetPasswordFailed = "100003007";
        public const string UserResetPasswordBeforeSetPasswordFailed = "100003008";
        public const string UserTenantInfoMissed = "100003009";
        public const string UserWithoutAdminRolePermission = "100003010";
        public const string RoleCreatedFailed = "100003011";
        public const string RoleGrantUserFailed = "100003012";
        
        public const string UserRegisterSuccess = "100003991";
        public const string UserGetInfoSuccess = "100003992";
        public const string UserRefreshTokenSuccess = "100003993";
        public const string UserCheckPublicIdAndJwtVersionSuccess = "100003994";
        public const string UserChangePasswordSuccess = "100003995";
        public const string UserLoginSuccess = "100003996";
        public const string UserLoginSuccessButFirstLogin = "100003997";
        public const string UserEmailVerificationSuccess = "100003998";
        public const string UserGetByIdSuccess = "100003999";
    }

    public const string InternalError = "900900001";
}