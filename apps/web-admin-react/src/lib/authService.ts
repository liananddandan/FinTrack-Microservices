import { accountApi } from "./accountApi"
import { authStore } from "./authStore"

export const authService = {
  async initializeProfile() {
    const state = authStore.getState()

    if (!state.accountAccessToken) {
      return
    }

    if (state.profile) {
      return
    }

    const profile = await accountApi.getCurrentUser()
    authStore.setProfile(profile)
  },

  async activateTenantForCurrentHost(): Promise<boolean> {
    const state = authStore.getState()

    if (!state.accountAccessToken) {
      return false
    }

    if (state.tenantAccessToken) {
      return true
    }

    try {
      const tenantToken = await accountApi.selectTenant()
      authStore.setTenantAccessToken(tenantToken)
      return true
    } catch {
      authStore.clearTenantAccessToken()
      return false
    }
  },

  async initialize() {
    await this.initializeProfile()
    await this.activateTenantForCurrentHost()
  },
}