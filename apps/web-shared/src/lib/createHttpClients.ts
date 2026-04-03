import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from "axios"
import type { ApiResponse } from "../api/types"
import type { JwtTokenPair } from "../api/account/types"

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

export type AuthSnapshot = {
  accountAccessToken: string
  scopedAccessToken: string
  refreshToken: string
}

export type CreateHttpClientsOptions = {
  baseURL: string
  timeout?: number
  scopedClientName?: string
  loginPath: string
  postRefreshPath: string

  getAuthState: () => AuthSnapshot

  setAccountTokens: (accessToken: string, refreshToken: string) => void
  clearScopedAccessToken: () => void
  clearProfile: () => void
  logout: () => void

  navigateTo: (path: string) => void | Promise<void>

  refreshTokenRequest?: (
    publicHttp: AxiosInstance,
    refreshToken: string
  ) => Promise<JwtTokenPair>

  enableDebug?: boolean
}

export type HttpClients = {
  publicHttp: AxiosInstance
  accountHttp: AxiosInstance
  scopedHttp: AxiosInstance
}

function buildDebugUrl(config: InternalAxiosRequestConfig): string {
  const finalBaseURL = config.baseURL ?? ""
  const finalUrl = config.url ?? ""

  if (!finalBaseURL) {
    return finalUrl
  }

  const normalizedBase = finalBaseURL.endsWith("/")
    ? finalBaseURL.slice(0, -1)
    : finalBaseURL

  const normalizedUrl = finalUrl.startsWith("/")
    ? finalUrl
    : `/${finalUrl}`

  return `${normalizedBase}${normalizedUrl}`
}

function attachDebugInterceptors(client: AxiosInstance, clientName: string) {
  client.interceptors.request.use(
    (config) => {
      console.log(`[${clientName}][request]`, {
        method: config.method,
        baseURL: config.baseURL,
        url: config.url,
        fullUrl: buildDebugUrl(config),
        headers: config.headers,
        params: config.params,
        data: config.data,
      })
      return config
    },
    (error) => {
      console.error(`[${clientName}][request error]`, error)
      return Promise.reject(error)
    }
  )

  client.interceptors.response.use(
    (response) => {
      console.log(`[${clientName}][response]`, {
        status: response.status,
        configUrl: response.config?.url,
        baseURL: response.config?.baseURL,
        fullUrl: response.config ? buildDebugUrl(response.config) : undefined,
        data: response.data,
      })
      return response
    },
    (error: AxiosError) => {
      console.error(`[${clientName}][response error]`, {
        message: error.message,
        status: error.response?.status,
        configUrl: error.config?.url,
        baseURL: error.config?.baseURL,
        fullUrl: error.config ? buildDebugUrl(error.config) : undefined,
        responseData: error.response?.data,
      })
      return Promise.reject(error)
    }
  )
}

export function createHttpClients(options: CreateHttpClientsOptions): HttpClients {
  const {
    baseURL,
    timeout = 15000,
    scopedClientName = "scopedHttp",
    loginPath,
    postRefreshPath,
    getAuthState,
    setAccountTokens,
    clearScopedAccessToken,
    clearProfile,
    logout,
    navigateTo,
    enableDebug = false,
  } = options

  const publicHttp = axios.create({
    baseURL,
    timeout,
  })

  const accountHttp = axios.create({
    baseURL,
    timeout,
  })

  const scopedHttp = axios.create({
    baseURL,
    timeout,
  })

  if (enableDebug) {
    attachDebugInterceptors(publicHttp, "publicHttp")
    attachDebugInterceptors(accountHttp, "accountHttp")
    attachDebugInterceptors(scopedHttp, scopedClientName)
  }

  accountHttp.interceptors.request.use((config) => {
    const token = getAuthState().accountAccessToken

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  })

  scopedHttp.interceptors.request.use((config) => {
    const token = getAuthState().scopedAccessToken

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  })

  const performRefresh =
    options.refreshTokenRequest ??
    (async (client: AxiosInstance, refreshToken: string) => {
      const refreshResponse = await client.get<ApiResponse<JwtTokenPair>>(
        "/api/account/refresh-token",
        {
          headers: {
            Authorization: `Bearer ${refreshToken}`,
          },
        }
      )

      const tokenPair = refreshResponse.data?.data

      if (!tokenPair?.accessToken || !tokenPair?.refreshToken) {
        throw new Error("Refresh token response is invalid.")
      }

      return tokenPair
    })

  const attach401Handler = (client: AxiosInstance) => {
    client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as RetryableRequestConfig | undefined

        if (!originalRequest) {
          return Promise.reject(error)
        }

        const status = error.response?.status
        const requestUrl = originalRequest.url ?? ""
        const isRefreshRequest = requestUrl.includes("/api/account/refresh-token")

        if (status !== 401) {
          return Promise.reject(error)
        }

        if (isRefreshRequest || originalRequest._retry) {
          logout()
          await navigateTo(loginPath)
          return Promise.reject(error)
        }

        const refreshToken = getAuthState().refreshToken

        if (!refreshToken) {
          logout()
          await navigateTo(loginPath)
          return Promise.reject(error)
        }

        originalRequest._retry = true

        try {
          const tokenPair = await performRefresh(publicHttp, refreshToken)

          setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken)
          clearScopedAccessToken()
          clearProfile()

          await navigateTo(postRefreshPath)
          return Promise.reject(error)
        } catch (refreshError) {
          logout()
          await navigateTo(loginPath)
          return Promise.reject(refreshError)
        }
      }
    )
  }

  attach401Handler(accountHttp)
  attach401Handler(scopedHttp)

  return {
    publicHttp,
    accountHttp,
    scopedHttp,
  }
}