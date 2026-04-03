import type { CurrentUserResult } from "@fintrack/web-shared"

const ACCOUNT_ACCESS_TOKEN_KEY = "platform.accountAccessToken"
const PLATFORM_ACCESS_TOKEN_KEY = "platform.platformAccessToken"
const REFRESH_TOKEN_KEY = "platform.refreshToken"
const PLATFORM_ROLE_KEY = "platform.platformRole"

type AuthState = {
  accountAccessToken: string
  platformAccessToken: string
  refreshToken: string
  platformRole: string
  profile: CurrentUserResult | null
}

let state: AuthState = {
  accountAccessToken: localStorage.getItem(ACCOUNT_ACCESS_TOKEN_KEY) ?? "",
  platformAccessToken: localStorage.getItem(PLATFORM_ACCESS_TOKEN_KEY) ?? "",
  refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) ?? "",
  platformRole: localStorage.getItem(PLATFORM_ROLE_KEY) ?? "",
  profile: null,
}

const listeners = new Set<() => void>()

function notify() {
  listeners.forEach((listener) => listener())
}

export const platformAuthStore = {
  subscribe(listener: () => void) {
    listeners.add(listener)

    return () => {
      listeners.delete(listener)
    }
  },

  getState(): AuthState {
    return state
  },

  get isAuthenticated() {
    return !!state.accountAccessToken
  },

  get hasPlatformContext() {
    return !!state.platformAccessToken
  },

  get platformRole() {
    return state.platformRole
  },

  get userEmail() {
    return state.profile?.email ?? ""
  },

  get userName() {
    return state.profile?.userName ?? ""
  },

  setAccountTokens(accessToken: string, refreshToken: string) {
    state = {
      ...state,
      accountAccessToken: accessToken,
      refreshToken,
    }

    localStorage.setItem(ACCOUNT_ACCESS_TOKEN_KEY, accessToken)
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
    notify()
  },

  setPlatformAccessToken(token: string, platformRole: string) {
    state = {
      ...state,
      platformAccessToken: token,
      platformRole,
    }

    localStorage.setItem(PLATFORM_ACCESS_TOKEN_KEY, token)
    localStorage.setItem(PLATFORM_ROLE_KEY, platformRole)
    notify()
  },

  clearPlatformAccessToken() {
    state = {
      ...state,
      platformAccessToken: "",
      platformRole: "",
    }

    localStorage.removeItem(PLATFORM_ACCESS_TOKEN_KEY)
    localStorage.removeItem(PLATFORM_ROLE_KEY)
    notify()
  },

  setProfile(profile: CurrentUserResult | null) {
    state = {
      ...state,
      profile,
    }
    notify()
  },

  clearProfile() {
    state = {
      ...state,
      profile: null,
    }
    notify()
  },

  logout() {
    state = {
      accountAccessToken: "",
      platformAccessToken: "",
      refreshToken: "",
      platformRole: "",
      profile: null,
    }

    localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(PLATFORM_ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(PLATFORM_ROLE_KEY)
    notify()
  },
}