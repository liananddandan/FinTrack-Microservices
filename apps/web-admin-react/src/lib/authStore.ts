import type { CurrentUserResult, LoginMembershipDto } from "@fintrack/web-shared"

const ACCOUNT_ACCESS_TOKEN_KEY = "fintrack.accountAccessToken"
const TENANT_ACCESS_TOKEN_KEY = "fintrack.tenantAccessToken"
const REFRESH_TOKEN_KEY = "fintrack.refreshToken"

type AuthState = {
  accountAccessToken: string
  tenantAccessToken: string
  refreshToken: string
  memberships: LoginMembershipDto[]
  profile: CurrentUserResult | null
}

let state: AuthState = {
  accountAccessToken: localStorage.getItem(ACCOUNT_ACCESS_TOKEN_KEY) ?? "",
  tenantAccessToken: localStorage.getItem(TENANT_ACCESS_TOKEN_KEY) ?? "",
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

  get hasTenantContext() {
    return !!state.tenantAccessToken
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

  get currentMembership(): LoginMembershipDto | null {
    if (this.adminMemberships.length === 1) {
      return this.adminMemberships[0]
    }

    if (this.resolvedMemberships.length === 1) {
      return this.resolvedMemberships[0]
    }

    return null
  },

  get currentTenantName() {
    return this.currentMembership?.tenantName ?? ""
  },

  get currentTenantPublicId() {
    return this.currentMembership?.tenantPublicId ?? ""
  },

  get isAdmin() {
    return this.adminMemberships.length > 0
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

  setTenantAccessToken(token: string) {
    state = {
      ...state,
      tenantAccessToken: token,
    }

    localStorage.setItem(TENANT_ACCESS_TOKEN_KEY, token)
    notify()
  },

  clearTenantAccessToken() {
    state = {
      ...state,
      tenantAccessToken: "",
    }

    localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY)
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

  logout() {
    state = {
      accountAccessToken: "",
      tenantAccessToken: "",
      refreshToken: "",
      memberships: [],
      profile: null,
    }

    localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    notify()
  },

  clearProfile() {
    state = {
      ...state,
      profile: null,
    }

    notify()
  },
}