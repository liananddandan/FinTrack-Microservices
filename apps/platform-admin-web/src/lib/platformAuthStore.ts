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

console.log("[platformAuthStore] initial state", {
  accountAccessToken: !!state.accountAccessToken,
  platformAccessToken: !!state.platformAccessToken,
  refreshToken: !!state.refreshToken,
  platformRole: state.platformRole,
  profile: state.profile,
})

const listeners = new Set<() => void>()

function notify() {
    console.log("[platformAuthStore] notify", {
    accountAccessToken: !!state.accountAccessToken,
    platformAccessToken: !!state.platformAccessToken,
    platformRole: state.platformRole,
    userEmail: state.profile?.email ?? "",
    userName: state.profile?.userName ?? "",
  })
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
    console.log("[platformAuthStore] setAccountTokens", {
      hasAccessToken: !!accessToken,
      hasRefreshToken: !!refreshToken,
    })
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
    console.log("[platformAuthStore] setPlatformAccessToken", {
      hasPlatformToken: !!token,
      platformRole,
    })
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
    console.log("[platformAuthStore] setProfile", profile)

    state = {
      ...state,
      profile,
    }
    notify()
  },

  clearProfile() {
    console.log("[platformAuthStore] clearProfile")

    state = {
      ...state,
      profile: null,
    }
    notify()
  },

  logout() {
    console.log("[platformAuthStore] logout")

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