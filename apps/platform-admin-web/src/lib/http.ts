import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios"
import { platformAuthStore } from "./authStore"
import type { ApiResponse } from "../api/types"

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

type JwtTokenPair = {
  accessToken: string
  refreshToken: string
}

let navigate: ((path: string) => void) | null = null

export function registerNavigate(fn: (path: string) => void) {
  navigate = fn
}

async function navigateTo(path: string) {
  if (navigate) {
    navigate(path)
  }
}

const baseURL = import.meta.env.VITE_API_BASE_URL || ""

export const publicHttp = axios.create({
  baseURL,
  timeout: 15000,
})

export const accountHttp = axios.create({
  baseURL,
  timeout: 15000,
})

export const platformHttp = axios.create({
  baseURL,
  timeout: 15000,
})

accountHttp.interceptors.request.use((config) => {
  const token = platformAuthStore.getState().accountAccessToken

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

platformHttp.interceptors.request.use((config) => {
  const token = platformAuthStore.getState().platformAccessToken

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

const attach401Handler = (client: typeof accountHttp) => {
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

      if (isRefreshRequest) {
        platformAuthStore.logout()
        await navigateTo("/login")
        return Promise.reject(error)
      }

      if (originalRequest._retry) {
        platformAuthStore.logout()
        await navigateTo("/login")
        return Promise.reject(error)
      }

      const refreshToken = platformAuthStore.getState().refreshToken

      if (!refreshToken) {
        platformAuthStore.logout()
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

        platformAuthStore.setAccountTokens(
          tokenPair.accessToken,
          tokenPair.refreshToken
        )

        platformAuthStore.clearPlatformAccessToken()
        platformAuthStore.clearProfile()

        await navigateTo("/login")
        return Promise.reject(error)
      } catch (refreshError) {
        platformAuthStore.logout()
        await navigateTo("/login")
        return Promise.reject(refreshError)
      }
    }
  )
}

attach401Handler(accountHttp)
attach401Handler(platformHttp)