import { defineStore } from "pinia";
import { getCurrentUser, selectTenant } from "../api/account";

const ACCOUNT_ACCESS_TOKEN_KEY = "fintrack.accountAccessToken";
const TENANT_ACCESS_TOKEN_KEY = "fintrack.tenantAccessToken";
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
    accountAccessToken: localStorage.getItem(ACCOUNT_ACCESS_TOKEN_KEY) ?? "",
    tenantAccessToken: localStorage.getItem(TENANT_ACCESS_TOKEN_KEY) ?? "",
    refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) ?? "",
    memberships: [] as LoginMembershipDto[],
    profile: null as UserProfile | null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.accountAccessToken,
    hasTenantContext: (state) => !!state.tenantAccessToken,

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

      return memberships.length > 0 ? memberships[0] ?? null : null;
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
      if (!this.accountAccessToken) return;
      if (this.profile) return;

      try {
        const profile = await getCurrentUser();
        this.setProfile(profile);
      } catch {
        this.logout();
      }
    },

    clearProfile() {
      this.profile = null;
    },

    setAccountTokens(accessToken: string, refreshToken: string) {
      this.accountAccessToken = accessToken;
      this.refreshToken = refreshToken;

      localStorage.setItem(ACCOUNT_ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    },

    setTenantAccessToken(token: string) {
      this.tenantAccessToken = token;
      localStorage.setItem(TENANT_ACCESS_TOKEN_KEY, token);
    },

    clearTenantAccessToken() {
      this.tenantAccessToken = "";
      localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY);
    },

    setMemberships(memberships: LoginMembershipDto[]) {
      this.memberships = memberships;
    },

    setProfile(profile: UserProfile) {
      this.profile = profile;
    },

    async activateSingleTenantIfPossible() {
      const memberships = this.resolvedMemberships;

      if (memberships.length !== 1) {
        this.clearTenantAccessToken();
        return;
      }

      const firstMembership = memberships[0];

      if (!firstMembership) {
        this.clearTenantAccessToken();
        return;
      }

      const tenantToken = await selectTenant({
        tenantPublicId: firstMembership.tenantPublicId,
      });

      this.setTenantAccessToken(tenantToken);
    },

    logout() {
      this.accountAccessToken = "";
      this.tenantAccessToken = "";
      this.refreshToken = "";
      this.memberships = [];
      this.profile = null;

      localStorage.removeItem(ACCOUNT_ACCESS_TOKEN_KEY);
      localStorage.removeItem(TENANT_ACCESS_TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    },
  },
});