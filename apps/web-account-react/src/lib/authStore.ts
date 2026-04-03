import type { CurrentUserResult, LoginMembershipDto } from "@fintrack/web-shared"

const ACCOUNT_ACCESS_TOKEN_KEY = "fintrack.account.accountAccessToken"
const REFRESH_TOKEN_KEY = "fintrack.account.refreshToken"

type AuthState = {
  accountAccessToken: string
  refreshToken: string
  memberships: LoginMembershipDto[]
  profile: CurrentUserResult | null
}

let state: AuthState = {
  accountAccessToken: localStorage.getItem(ACCOUNT_ACCESS_TOKEN_KEY) ?? "",
  refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) ?? "",
  memberships: [],
  profile: null,
}

const listeners = new Set<() => void>()

function notify() {
  listeners.forEach((listener) => listener())
}

export const authStore = {
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

  get userEmail() {
    return state.profile?.email ?? ""
  },

  get userName() {
    return state.profile?.userName ?? ""
  },

  get resolvedMemberships(): LoginMembershipDto[] {
    return state.profile?.memberships?.length
      ? state.profile.memberships
      : state.memberships
  },

  get adminMemberships(): LoginMembershipDto[] {
    return this.resolvedMemberships.filter((m) => m.role === "Admin")
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

  clearAccountTokens() {
    state = {
      ...state,
      accountAccessToken: "",
      refreshToken: "",
    }

    localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    notify()
  },

  setMemberships(memberships: LoginMembershipDto[]) {
    state = {
      ...state,
      memberships,
    }

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
      refreshToken: "",
      memberships: [],
      profile: null,
    }

    localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    notify()
  },
}