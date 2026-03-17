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

const baseURL = import.meta.env.VITE_API_BASE_URL

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

accountHttp.interceptors.request.use((config) => {
    const token = authStore.getState().accountAccessToken

    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }

    return config
})

tenantHttp.interceptors.request.use((config) => {
    const token = authStore.getState().tenantAccessToken

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

attach401Handler(accountHttp)
attach401Handler(tenantHttp)