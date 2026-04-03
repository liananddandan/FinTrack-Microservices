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

  async initialize() {
    await this.initializeProfile()
  },
}