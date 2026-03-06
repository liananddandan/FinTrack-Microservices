import { defineStore } from "pinia"

const ACCESS_TOKEN_KEY = "fintrack.accessToken"
const REFRESH_TOKEN_KEY = "fintrack.refreshToken"

export type UserProfile = {
  userPublicId: string
  email: string
  userName?: string
  memberships?: Array<{
    tenantPublicId: string
    tenantName: string
    role: string
  }>
}

export const useAuthStore = defineStore("auth", {
  state: () => ({
    accessToken: localStorage.getItem(ACCESS_TOKEN_KEY) ?? "",
    refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) ?? "",
    profile: null as UserProfile | null
  }),

  getters: {
    userEmail: (state) => state.profile?.email ?? "",
    memberships: (state) => state.profile?.memberships ?? []
  },

  actions: {
    setTokens(accessToken: string, refreshToken: string) {
      this.accessToken = accessToken
      this.refreshToken = refreshToken

      localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
    },

    setProfile(profile: UserProfile) {
      this.profile = profile
    },

    logout() {
      this.accessToken = ""
      this.refreshToken = ""
      this.profile = null

      localStorage.removeItem(ACCESS_TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
    }
  }
})