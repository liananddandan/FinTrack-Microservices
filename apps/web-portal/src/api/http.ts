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

const baseURL = import.meta.env.VITE_API_BASE_URL;

export const publicHttp = axios.create({
  baseURL,
  timeout: 15000,
});

export const accountHttp = axios.create({
  baseURL,
  timeout: 15000,
});

export const tenantHttp = axios.create({
  baseURL,
  timeout: 15000,
});

accountHttp.interceptors.request.use((config) => {
  const auth = useAuthStore();

  if (auth.accountAccessToken) {
    config.headers.Authorization = `Bearer ${auth.accountAccessToken}`;
  }

  return config;
});

tenantHttp.interceptors.request.use((config) => {
  const auth = useAuthStore();
  console.log("tenant token in interceptor:", auth.tenantAccessToken);

  if (auth.tenantAccessToken) {
    config.headers.Authorization = `Bearer ${auth.tenantAccessToken}`;
  }
  console.log("final auth header:", config.headers.Authorization);

  return config;
});

const attach401Handler = (client: typeof accountHttp) => {
  client.interceptors.response.use(
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
        const refreshResponse = await publicHttp.get<ApiResponse<JwtTokenPair>>(
          "/api/account/refresh-token",
          {
            headers: {
              Authorization: `Bearer ${auth.refreshToken}`,
            },
          }
        );

        const tokenPair = refreshResponse.data?.data;

        if (!tokenPair?.accessToken || !tokenPair?.refreshToken) {
          throw new Error("Refresh token response is invalid.");
        }

        auth.setAccountTokens(tokenPair.accessToken, tokenPair.refreshToken);

        // tenant token 需要重新选择 tenant 才能重新拿到
        auth.clearTenantAccessToken();
        auth.clearProfile();

        await router.push("/waiting-membership");
        return Promise.reject(error);
      } catch (refreshError) {
        auth.logout();
        await router.push("/login");
        return Promise.reject(refreshError);
      }
    }
  );
};

attach401Handler(accountHttp);
attach401Handler(tenantHttp);