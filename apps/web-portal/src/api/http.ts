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

const anonymousPaths = [
  "/api/account/login",
  "/api/account/register",
  "/api/tenant/register",
  "/api/account/confirm-email",
  "/api/tenant/invitations/resolve",
  "/api/tenant/invitations/accept",
];

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
});

http.interceptors.request.use((config) => {
  const auth = useAuthStore();

  if (auth.accountAccessToken) {
    config.headers.Authorization = `Bearer ${auth.accountAccessToken}`;
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
    const isAnonymousRequest = anonymousPaths.some((path) =>
      requestUrl.includes(path)
    );

    if (status !== 401) {
      return Promise.reject(error);
    }

    // refresh 自己失败，直接登出
    if (isRefreshRequest) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(error);
    }

    // 匿名接口失败，交给页面自己处理
    if (isAnonymousRequest) {
      return Promise.reject(error);
    }

    // 已经重试过，直接登出
    if (originalRequest._retry) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(error);
    }

    // 没 refresh token，直接登出
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
          timeout: 10000,
        }
      );

      const tokenPair = refreshResponse.data?.data;

      if (!tokenPair?.accessToken || !tokenPair?.refreshToken) {
        throw new Error("Refresh token response is invalid.");
      }

      auth.setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken);

      originalRequest.headers = originalRequest.headers ?? {};
      originalRequest.headers.Authorization = `Bearer ${tokenPair.accessToken}`;

      return http(originalRequest);
    } catch (refreshError) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(refreshError);
    }
  }
);