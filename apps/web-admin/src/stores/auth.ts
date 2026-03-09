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

    adminMemberships(): LoginMembershipDto[] {
      const memberships =
        this.profile?.memberships?.length
          ? this.profile.memberships
          : this.memberships;

      return memberships.filter((m) => m.role === "Admin");
    },

    currentMembership(): LoginMembershipDto | null {
      const memberships = this.adminMemberships;
      return memberships.length > 0 ? memberships[0] ?? null : null;
    },

    currentTenantName(): string {
      return this.currentMembership?.tenantName ?? "";
    },

    currentTenantPublicId(): string {
      return this.currentMembership?.tenantPublicId ?? "";
    },

    isAdmin(): boolean {
      return this.adminMemberships.length > 0;
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

    setProfile(profile: UserProfile | null) {
      this.profile = profile;
    },

    clearProfile() {
      this.profile = null;
    },

    async activateSingleAdminTenantIfPossible() {
      const adminMemberships = this.adminMemberships;

      if (adminMemberships.length !== 1) {
        this.clearTenantAccessToken();
        return false;
      }

      const firstMembership = adminMemberships[0];

      if (!firstMembership) {
        this.clearTenantAccessToken();
        return;
      }

      const tenantToken = await selectTenant({
        tenantPublicId: firstMembership.tenantPublicId,
      });

      this.setTenantAccessToken(tenantToken);
      return true;
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