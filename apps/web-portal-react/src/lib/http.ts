import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios"
import { authManager, navigateTo } from "./authManager"
import { authStore } from "./authStore"

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

export type ApiResponse<T> = {
  code: string
  message: string
  data: T
}

export type JwtTokenPair = {
  accessToken: string
  refreshToken: string
}

const rawBaseURL = import.meta.env.VITE_API_BASE_URL
const baseURL = rawBaseURL || ""

console.log("[env] MODE =", import.meta.env.MODE)
console.log("[env] DEV =", import.meta.env.DEV)
console.log("[env] PROD =", import.meta.env.PROD)
console.log("[env] VITE_API_BASE_URL =", rawBaseURL)
console.log("[http] resolved baseURL =", baseURL)

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

function attachDebugInterceptors(
  client: typeof publicHttp,
  clientName: string
) {
  client.interceptors.request.use(
    (config) => {
      const fullUrl = buildDebugUrl(config)

      console.log(`[${clientName}][request]`, {
        method: config.method,
        baseURL: config.baseURL,
        url: config.url,
        fullUrl,
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

export const publicHttp = axios.create({
  baseURL,
  timeout: 15000,
})

export const accountHttp = axios.create({
  baseURL,
  timeout: 15000,
})

export const tenantHttp = axios.create({
  baseURL,
  timeout: 15000,
})

attachDebugInterceptors(publicHttp, "publicHttp")
attachDebugInterceptors(accountHttp, "accountHttp")
attachDebugInterceptors(tenantHttp, "tenantHttp")

accountHttp.interceptors.request.use((config) => {
  const token = authStore.getState().accountAccessToken

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  console.log("[accountHttp][auth]", {
    hasAccountToken: !!token,
    fullUrl: buildDebugUrl(config),
  })

  return config
})

tenantHttp.interceptors.request.use((config) => {
  const token = authStore.getState().tenantAccessToken

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  console.log("[tenantHttp][auth]", {
    hasTenantToken: !!token,
    fullUrl: buildDebugUrl(config),
  })

  return config
})

const attach401Handler = (client: typeof accountHttp, clientName: string) => {
  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as RetryableRequestConfig | undefined

      if (!originalRequest) {
        console.error(`[${clientName}][401 handler] missing originalRequest`)
        return Promise.reject(error)
      }

      const status = error.response?.status
      const requestUrl = originalRequest.url ?? ""
      const isRefreshRequest = requestUrl.includes("/api/account/refresh-token")

      console.log(`[${clientName}][401 handler]`, {
        status,
        requestUrl,
        fullUrl: buildDebugUrl(originalRequest),
        isRefreshRequest,
        retry: originalRequest._retry,
      })

      if (status !== 401) {
        return Promise.reject(error)
      }

      if (isRefreshRequest) {
        authManager.logout()
        await navigateTo("/login")
        return Promise.reject(error)
      }

      if (originalRequest._retry) {
        authManager.logout()
        await navigateTo("/login")
        return Promise.reject(error)
      }

      const refreshToken = authStore.getState().refreshToken

      if (!refreshToken) {
        authManager.logout()
        await navigateTo("/login")
        return Promise.reject(error)
      }

      originalRequest._retry = true

      try {
        const refreshResponse = await publicHttp.get<ApiResponse<JwtTokenPair>>(
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

        authStore.setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken)
        authStore.clearTenantAccessToken()
        authStore.clearProfile()

        await navigateTo("/waiting-membership")
        return Promise.reject(error)
      } catch (refreshError) {
        authManager.logout()
        await navigateTo("/login")
        return Promise.reject(refreshError)
      }
    }
  )
}

attach401Handler(accountHttp, "accountHttp")
attach401Handler(tenantHttp, "tenantHttp")