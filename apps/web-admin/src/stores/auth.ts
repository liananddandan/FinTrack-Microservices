import { defineStore } from "pinia";
import { getCurrentUser } from "../api/account";

const ACCESS_TOKEN_KEY = "fintrack.accessToken";
const REFRESH_TOKEN_KEY = "fintrack.refreshToken";

export type LoginMembershipDto = {
  tenantPublicId: string;
  tenantName: string;
  role: string;
};

export type UserProfile = {
  userPublicId: string;
  email: string;
  userName?: string;
  memberships?: LoginMembershipDto[];
};

export const useAuthStore = defineStore("auth", {
  state: () => ({
    accessToken: localStorage.getItem(ACCESS_TOKEN_KEY) ?? "",
    refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) ?? "",
    memberships: [] as LoginMembershipDto[],
    profile: null as UserProfile | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.accessToken,
    userEmail: (state) => state.profile?.email ?? "",
    userName: (state) => state.profile?.userName ?? "",

    resolvedMemberships: (state) =>
      state.profile?.memberships?.length
        ? state.profile.memberships
        : state.memberships,

    currentMembership(): LoginMembershipDto | null {
      const memberships =
        this.profile?.memberships?.length
          ? this.profile.memberships
          : this.memberships;

      return memberships.length > 0 ? memberships[0] : null;
    },

    currentTenantName(): string {
      return this.currentMembership?.tenantName ?? "";
    },

    currentTenantPublicId(): string {
      return this.currentMembership?.tenantPublicId ?? "";
    },

    isAdmin(): boolean {
      return this.currentMembership?.role === "Admin";
    },
  },

  actions: {
    async initialize() {
      if (!this.accessToken) return;
      if (this.profile) return;

      try {
        const profile = await getCurrentUser();
        this.setProfile(profile);
      } catch {
        this.logout();
      }
    },

    setTokens(accessToken: string, refreshToken: string) {
      this.accessToken = accessToken;
      this.refreshToken = refreshToken;

      localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    },

    setMemberships(memberships: LoginMembershipDto[]) {
      this.memberships = memberships;
    },

    setProfile(profile: UserProfile) {
      this.profile = profile;
    },

    logout() {
      this.accessToken = "";
      this.refreshToken = "";
      this.memberships = [];
      this.profile = null;

      localStorage.removeItem(ACCESS_TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    },
  },
});