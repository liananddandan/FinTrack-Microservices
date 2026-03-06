import { defineStore } from "pinia";

type AuthState = {
  accessToken: string | null;
  refreshToken: string | null;
  userEmail: string | null;
};

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    accessToken: localStorage.getItem("portal.accessToken"),
    refreshToken: localStorage.getItem("portal.refreshToken"),
    userEmail: localStorage.getItem("portal.userEmail"),
  }),

  actions: {
    setAuth(payload: { accessToken: string; refreshToken?: string; userEmail?: string }) {
      this.accessToken = payload.accessToken;
      this.refreshToken = payload.refreshToken ?? null;
      this.userEmail = payload.userEmail ?? null;

      localStorage.setItem("portal.accessToken", this.accessToken);
      if (this.refreshToken) {
        localStorage.setItem("portal.refreshToken", this.refreshToken);
      }
      if (this.userEmail) {
        localStorage.setItem("portal.userEmail", this.userEmail);
      }
    },

    logout() {
      this.accessToken = null;
      this.refreshToken = null;
      this.userEmail = null;

      localStorage.removeItem("portal.accessToken");
      localStorage.removeItem("portal.refreshToken");
      localStorage.removeItem("portal.userEmail");
    },
  },
});