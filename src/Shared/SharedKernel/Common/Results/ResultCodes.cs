namespace SharedKernel.Common.Results;

public static class ResultCodes
{
    public static class Tenant
    {
        // Identity 100******
        // RegisterTenant Failed 100 001 001
        public const string RegisterTenantParameterError = "100001001";
        public const string RegisterTenantExistedError = "100001002";
        public const string RegisterTenantCreateError = "100001003";
        public const string RegisterTenantException = "100001004";
        public const string GetTenantMembersParameterError = "100001005";
        public const string GetTenantMembersException= "100001006";
        public const string CreateInvitationParameterError= "100001007";
        public const string CreateInvitationTenantNotFound = "100001008";
        public const string CreateInvitationUserNotFound = "100001009";
        public const string CreateInvitationUserAlreadyExists = "100001010";
        public const string CreateInvitationInviterNotFound = "100001011";
        public const string CreateInvitationException = "100001012";
        public const string InvitationInvalidPublicId = "100001013";
        public const string InvitationNotFound = "100001014";
        public const string ResolveInvitationNotFound = "100001015";
        public const string ResolveInvitationVersionInvalid = "100001016";
        public const string ResolveInvitationStatusInvalid = "100001017";
        public const string ResolveInvitationExpired = "100001018";
        public const string AcceptInvitationNotFound = "100001019";
        public const string AcceptInvitationVersionInvalid = "100001020";
        public const string AcceptInvitationStatusInvalid = "100001021";
        public const string AcceptInvitationExpired = "100001022";
        public const string AcceptInvitationUserNotFound = "100001023";
        public const string AcceptInvitationMembershipExists = "100001024";
        public const string GetTenantInvitationsParameterError = "100001025";
        public const string GetTenantInvitationsException = "100001026";
        public const string ResendInvitationParameterError = "100001027";
        public const string ResendInvitationNotFound = "100001028";
        public const string ResendInvitationStatusInvalid = "100001029";
        public const string ResendInvitationExpired = "100001030";
        public const string ResendInvitationMembershipExists = "100001031";
        public const string ResendInvitationException = "100001032";
        public const string MemberNotFound = "100001033";
        public const string MemberAlreadyRemoved = "100001034";
        public const string MemberNotInTenant = "100001035";
        public const string CannotRemoveSelf = "100001036";
        public const string ChangeMemberRoleParameterError = "100001037";
        public const string ChangeMemberRoleInvalidRole = "100001038";
        public const string ChangeMemberRoleInactiveMembership = "100001039";
        public const string CannotChangeOwnRole = "100001040";
        public const string ChangeMemberRoleNoChange = "100001041";
        public const string CannotDemoteLastAdmin = "100001042";
        // RegisterTenant Success 100 001 999
        public const string ChangeMemberRoleSuccess = "100001990";
        public const string MemberRemoved = "100001991";
        public const string ResendInvitationSuccess = "100001992";
        public const string GetTenantInvitationsSuccess = "100001993";
        public const string AcceptInvitationSuccess = "100001994";
        public const string ResolveInvitationSuccess = "100001995";
        public const string InvitationSuccess = "100001996";
        public const string CreateInvitationSuccess = "100001997";
        public const string GetTenantMembersSuccess = "100001998";
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
    
    public static class Transaction
    {
        public const string TransactionCreateFailed = "100004001";
        public const string TransactionNotFound = "100004002";
        public const string TransactionNotBelongToCurrentUser = "100004003";
        public const string TransactionQueryFailed = "100004004";
        public const string ProcurementCreateSuccess = "100004101";
        public const string ProcurementUpdateSuccess = "100004102";
        public const string ProcurementSubmitSuccess = "100004103";
        public const string ProcurementApproveSuccess = "100004104";
        public const string ProcurementRejectSuccess = "100004105";
        public const string TransactionQueryByPageSuccess = "100004997";
        public const string TransactionQuerySuccess = "100004998";
        public const string TransactionCreateSuccess = "100004999";
    }

    public static class Account
    {
        public const string LoginParameterError = "100005001";
        public const string LoginInvalidCredential = "100005002";
        public const string LoginNoTenant = "100005003";
        public const string LoginException = "100005004";
        public const string UserNotFound = "100005005";
        public const string GetUserInfoException = "100005006";
        public const string LoginMultipleTenantsNotSupported = "100005007";
        public const string RegisterUserParameterError = "100005008";
        public const string RegisterUserEmailExists = "100005009";
        public const string RegisterUserCreateFailed = "100005011";
        public const string RegisterUserException = "100005012";
        public const string SelectTenantParameterError = "100005013";
        public const string SelectTenantUserNotFound = "100005014";
        public const string SelectTenantMembershipNotFound = "100005015";
        public const string SelectTenantException = "100005016";
        
        public const string SelectTenantSuccess = "100005096";
        public const string RegisterUserSuccess = "100005097";
        public const string GetUserInfoSuccess = "100005098";
        public const string LoginSuccess = "100005099";
    }

    public static class ProductCategory
    {
        public const string CreateParameterError = "100006001";
        public const string CreateDuplicatedName = "100006002";
        public const string UpdateParameterError = "100006003";
        public const string UpdateDuplicatedName = "100006004";
        public const string NotFound = "100006005";
        public const string DeleteHasProducts = "100006006";

        public const string DeleteSuccess = "100006997";
        public const string GetListSuccess = "100006998";
        public const string UpdateSuccess = "100006999";
        public const string CreateSuccess = "100006996";
    }
    
    public const string InternalError = "900900001";
    public const string Forbidden = "900900002";
}