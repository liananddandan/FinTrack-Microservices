import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios"
import type { ApiResponse } from "@fintrack/web-shared"
import { authStore } from "./authStore"

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

type JwtTokenPair = {
  accessToken: string
  refreshToken: string
}

function resolveBaseUrl(): string {
  const envBaseUrl = import.meta.env.VITE_API_BASE_URL

  if (envBaseUrl) {
    return envBaseUrl
  }

  const { protocol, hostname } = window.location

  if (import.meta.env.DEV) {
    return `${protocol}//${hostname}:5193`
  }

  return `${protocol}//${hostname}`
}

function redirectToLogin() {
  window.location.assign("/account/login")
}

const baseURL = resolveBaseUrl()

export const publicHttp = axios.create({
  baseURL,
  timeout: 15000,
})

export const accountHttp = axios.create({
  baseURL,
  timeout: 15000,
})

accountHttp.interceptors.request.use((config) => {
  const token = authStore.getState().accountAccessToken

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

accountHttp.interceptors.response.use(
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

    if (isRefreshRequest) {
      authStore.logout()
      redirectToLogin()
      return Promise.reject(error)
    }

    if (originalRequest._retry) {
      authStore.logout()
      redirectToLogin()
      return Promise.reject(error)
    }

    const refreshToken = authStore.getState().refreshToken

    if (!refreshToken) {
      authStore.logout()
      redirectToLogin()
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
      authStore.clearProfile()

      originalRequest.headers.Authorization = `Bearer ${tokenPair.accessToken}`
      return await accountHttp.request(originalRequest)
    } catch (refreshError) {
      authStore.logout()
      redirectToLogin()
      return Promise.reject(refreshError)
    }
  }
)