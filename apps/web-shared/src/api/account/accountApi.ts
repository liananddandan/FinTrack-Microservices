import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  LoginRequest,
  UserLoginResult,
  CurrentUserResult,
  RegisterUserRequest,
  RegisterTenantRequest,
  PlatformTokenDto,
} from "./types"

export function createAccountApi(params: {
  publicHttp: AxiosInstance
  accountHttp: AxiosInstance
  platformHttp?: AxiosInstance
}) {
  const { publicHttp, accountHttp, platformHttp } = params

  function unwrapApiResponse<T>(
    response: { data: ApiResponse<T> },
    defaultMessage: string
  ): T {
    const result = response.data

    if (result.data == null) {
      throw new Error(result.message || defaultMessage)
    }

    return result.data
  }

  function requirePlatformHttp(): AxiosInstance {
    if (!platformHttp) {
      throw new Error("platformHttp is not configured.")
    }

    return platformHttp
  }

  return {
    async registerTenant(request: RegisterTenantRequest) {
      const response = await publicHttp.post<ApiResponse<null>>(
        "/api/tenant/register",
        request
      )

      return response.data
    },

    async registerUser(request: RegisterUserRequest) {
      const response = await publicHttp.post<ApiResponse<null>>(
        "/api/account/register",
        request
      )

      return response.data
    },

    async login(request: LoginRequest): Promise<UserLoginResult> {
      const response = await publicHttp.post<ApiResponse<UserLoginResult>>(
        "/api/account/login",
        request
      )

      return unwrapApiResponse(response, "Login failed")
    },

    async getCurrentUser(): Promise<CurrentUserResult> {
      const response = await accountHttp.get<ApiResponse<CurrentUserResult>>(
        "/api/account/me"
      )

      return unwrapApiResponse(response, "Failed to fetch current user")
    },

    async selectTenant(): Promise<string> {
      const response = await accountHttp.post<ApiResponse<string>>(
        "/api/account/select-tenant",
        {}
      )

      return unwrapApiResponse(response, "Failed to select tenant")
    },

    async selectPlatform(): Promise<PlatformTokenDto> {
      const response = await accountHttp.post<ApiResponse<PlatformTokenDto>>(
        "/api/account/select-platform"
      )

      return unwrapApiResponse(response, "Failed to select platform")
    },

    async getPlatformOverview(): Promise<{ message: string }> {
      const client = requirePlatformHttp()

      const response = await client.get<ApiResponse<{ message: string }>>(
        "/api/platform/overview"
      )

      return unwrapApiResponse(response, "Failed to fetch platform overview")
    },

  }
}