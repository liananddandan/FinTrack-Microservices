import { createHttpClients } from "@fintrack/web-shared"
import { platformAuthStore } from "./platformAuthStore"

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

async function navigateTo(path: string) {
  window.location.assign(path)
}

const clients = createHttpClients({
  baseURL: resolveBaseUrl(),
  scopedClientName: "platformHttp",
  loginPath: "/login",
  postRefreshPath: "/overview",
  getAuthState: () => ({
    accountAccessToken: platformAuthStore.getState().accountAccessToken,
    scopedAccessToken: platformAuthStore.getState().platformAccessToken,
    refreshToken: platformAuthStore.getState().refreshToken,
  }),
  setAccountTokens: (accessToken, refreshToken) =>
    platformAuthStore.setAccountTokens(accessToken, refreshToken),
  clearScopedAccessToken: () => platformAuthStore.clearPlatformAccessToken(),
  clearProfile: () => platformAuthStore.clearProfile(),
  logout: () => platformAuthStore.logout(),
  navigateTo,
  enableDebug: import.meta.env.DEV,
})

export const publicHttp = clients.publicHttp
export const accountHttp = clients.accountHttp
export const platformHttp = clients.scopedHttp