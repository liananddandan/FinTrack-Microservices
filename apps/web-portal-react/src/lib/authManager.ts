type UserProfile = {
  id: string
  email: string
  fullName?: string
}

type AuthState = {
  accountAccessToken: string | null
  refreshToken: string | null
  tenantAccessToken: string | null
  profile: UserProfile | null
}

const ACCOUNT_ACCESS_TOKEN_KEY = "account_access_token"
const REFRESH_TOKEN_KEY = "refresh_token"
const TENANT_ACCESS_TOKEN_KEY = "tenant_access_token"
const PROFILE_KEY = "profile"

let navigateHandler: ((path: string) => void | Promise<void>) | null = null

export function setNavigateHandler(handler: (path: string) => void | Promise<void>) {
  navigateHandler = handler
}

export async function navigateTo(path: string) {
  if (navigateHandler) {
    await navigateHandler(path)
  } else {
    window.location.href = path
  }
}

function getJson<T>(key: string): T | null {
  const raw = localStorage.getItem(key)
  if (!raw) return null

  try {
    return JSON.parse(raw) as T
  } catch {
    return null
  }
}

function setJson(key: string, value: unknown) {
  localStorage.setItem(key, JSON.stringify(value))
}

export const authManager = {
  getAccountAccessToken(): string | null {
    return localStorage.getItem(ACCOUNT_ACCESS_TOKEN_KEY)
  },

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY)
  },

  getTenantAccessToken(): string | null {
    return localStorage.getItem(TENANT_ACCESS_TOKEN_KEY)
  },

  getProfile(): UserProfile | null {
    return getJson<UserProfile>(PROFILE_KEY)
  },

  setAccountTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem(ACCOUNT_ACCESS_TOKEN_KEY, accessToken)
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  },

  setTenantAccessToken(token: string) {
    localStorage.setItem(TENANT_ACCESS_TOKEN_KEY, token)
  },

  clearTenantAccessToken() {
    localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY)
  },

  clearProfile() {
    localStorage.removeItem(PROFILE_KEY)
  },

  setProfile(profile: UserProfile) {
    setJson(PROFILE_KEY, profile)
  },

  logout() {
    localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(PROFILE_KEY)
  },

  getState(): AuthState {
    return {
      accountAccessToken: this.getAccountAccessToken(),
      refreshToken: this.getRefreshToken(),
      tenantAccessToken: this.getTenantAccessToken(),
      profile: this.getProfile(),
    }
  },
}