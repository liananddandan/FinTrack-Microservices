import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import router from "../router";
import { useAuthStore } from "../stores/auth";

type RetryableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean;
};

type ApiResponse<T> = {
  code: string;
  message: string;
  data: T;
};

type JwtTokenPair = {
  accessToken: string;
  refreshToken: string;
};

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15000,
});

http.interceptors.request.use((config) => {
  const auth = useAuthStore();
  console.log("tenantAccessToken:", auth.tenantAccessToken);

  if (auth.tenantAccessToken) {
    config.headers.Authorization = `Bearer ${auth.tenantAccessToken}`;
  }

  return config;
});

http.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const auth = useAuthStore();
    const originalRequest = error.config as RetryableRequestConfig | undefined;

    if (!originalRequest) {
      return Promise.reject(error);
    }

    const status = error.response?.status;
    const requestUrl = originalRequest.url ?? "";
    const isRefreshRequest = requestUrl.includes("/api/account/refresh-token");

    if (status !== 401) {
      return Promise.reject(error);
    }

    if (isRefreshRequest) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(error);
    }

    if (originalRequest._retry) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(error);
    }

    if (!auth.refreshToken) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const refreshResponse = await axios.get<ApiResponse<JwtTokenPair>>(
        `${import.meta.env.VITE_API_BASE_URL}/api/account/refresh-token`,
        {
          headers: {
            Authorization: `Bearer ${auth.refreshToken}`,
          },
          timeout: 15000,
        }
      );

      const tokenPair = refreshResponse.data?.data;

      if (!tokenPair?.accessToken || !tokenPair?.refreshToken) {
        throw new Error("Refresh token response is invalid.");
      }

      // 先刷新 account token
      auth.setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken);

      // admin 后台此时 tenant token 已失效，清空上下文，回登录页最稳
      auth.clearTenantAccessToken();
      auth.clearProfile();
      await router.push("/login");

      return Promise.reject(error);
    } catch (refreshError) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(refreshError);
    }
  }
);