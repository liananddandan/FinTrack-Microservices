import { defineStore } from "pinia"

export type LoginMembershipDto = {
  tenantPublicId: string;
  tenantName: string;
  role: string;
};

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
    memberships: [] as LoginMembershipDto[],
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

    setMemberships(memberships: LoginMembershipDto[]) {
      this.memberships = memberships;
    },

    setProfile(profile: UserProfile) {
      this.profile = profile
    },

    setLoginSession(
      accessToken: string,
      refreshToken: string,
      memberships: LoginMembershipDto[]) {
       this.accessToken = accessToken;
      // this.accessToken = "bad-token-for-testing";
      this.refreshToken = refreshToken;
      this.memberships = memberships;

      localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    },

    logout() {
      this.accessToken = ""
      this.refreshToken = ""
      this.memberships = [];
      this.profile = null

      localStorage.removeItem(ACCESS_TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
    }
  }
})