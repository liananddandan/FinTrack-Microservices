import { defineStore } from "pinia";

type AuthState = {
  accessToken: string | null;
  tenantId: string | null;
};

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    accessToken: localStorage.getItem("accessToken"),
    tenantId: localStorage.getItem("tenantId"),
  }),
  actions: {
    setAccessToken(token: string) {
      this.accessToken = token;
      localStorage.setItem("accessToken", token);
    },
    setTenantId(tenantId: string) {
      this.tenantId = tenantId;
      localStorage.setItem("tenantId", tenantId);
    },
    logout() {
      this.accessToken = null;
      this.tenantId = null;
      localStorage.removeItem("accessToken");
      localStorage.removeItem("tenantId");
    },
  },
});