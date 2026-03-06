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
  baseURL: import.meta.env.VITE_API_BASE,
  timeout: 10000,
});

http.interceptors.request.use((config) => {
  const auth = useAuthStore();

  if (auth.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
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

    if (status !== 401 || isRefreshRequest) {
      return Promise.reject(error);
    }

    if (originalRequest._retry) {
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
        `${import.meta.env.VITE_API_BASE}/api/account/refresh-token`,
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

      auth.setTokens(tokenPair.accessToken, tokenPair.refreshToken);

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