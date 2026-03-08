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

export const tenantHttp = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
});

tenantHttp.interceptors.request.use((config) => {
  const auth = useAuthStore();

  if (auth.tenantAccessToken) {
    config.headers.Authorization = `Bearer ${auth.tenantAccessToken}`;
  }

  return config;
});

tenantHttp.interceptors.response.use(
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
          timeout: 10000,
        }
      );

      const tokenPair = refreshResponse.data?.data;

      if (!tokenPair?.accessToken || !tokenPair?.refreshToken) {
        throw new Error("Refresh token response is invalid.");
      }

      // 先更新 account token
      auth.setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken);

      // tenant token 失效后，当前阶段最稳妥是清空 tenant 上下文，要求重新选择 tenant
      auth.clearTenantAccessToken();

      await router.push("/waiting-membership");
      return Promise.reject(error);
    } catch (refreshError) {
      auth.logout();
      await router.push("/login");
      return Promise.reject(refreshError);
    }
  }
);