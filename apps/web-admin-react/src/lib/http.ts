import { createHttpClients } from "@fintrack/web-shared"
import { authStore } from "./authStore"
import { navigateTo } from "./authManager"

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

const clients = createHttpClients({
  baseURL: resolveBaseUrl(),
  scopedClientName: "tenantHttp",
  loginPath: "/login",
  postRefreshPath: "/overview",
  getAuthState: () => ({
    accountAccessToken: authStore.getState().accountAccessToken,
    scopedAccessToken: authStore.getState().tenantAccessToken,
    refreshToken: authStore.getState().refreshToken,
  }),
  setAccountTokens: (accessToken, refreshToken) =>
    authStore.setAccountTokens(accessToken, refreshToken),
  clearScopedAccessToken: () => authStore.clearTenantAccessToken(),
  clearProfile: () => authStore.clearProfile(),
  logout: () => authStore.logout(),
  navigateTo,
  enableDebug: import.meta.env.DEV,
})

export const publicHttp = clients.publicHttp
export const accountHttp = clients.accountHttp
export const tenantHttp = clients.scopedHttp