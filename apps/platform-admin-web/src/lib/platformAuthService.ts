import { accountApi } from "./accountApi"
import { platformAuthStore } from "./platformAuthStore"

export const platformAuthService = {
  async initialize() {
    const state = platformAuthStore.getState()

    if (!state.accountAccessToken) {
      return
    }

    if (!state.profile) {
      try {
        const profile = await accountApi.getCurrentUser()
        platformAuthStore.setProfile(profile)
      } catch {
        platformAuthStore.logout()
        return
      }
    }

    if (!state.platformAccessToken) {
      try {
        const result = await accountApi.selectPlatform()
        platformAuthStore.setPlatformAccessToken(
          result.platformAccessToken,
          result.platformRole
        )
      } catch {
        platformAuthStore.clearPlatformAccessToken()
      }
    }
  },

  async initializeProfile() {
    const state = platformAuthStore.getState()

    if (!state.accountAccessToken || state.profile) {
      return
    }

    try {
      const profile = await accountApi.getCurrentUser()
      platformAuthStore.setProfile(profile)
    } catch {
      platformAuthStore.logout()
    }
  },

  async activatePlatformContext(): Promise<boolean> {
    const state = platformAuthStore.getState()

    if (!state.accountAccessToken) {
      return false
    }

    try {
      const result = await accountApi.selectPlatform()
      platformAuthStore.setPlatformAccessToken(
        result.platformAccessToken,
        result.platformRole
      )
      return true
    } catch {
      platformAuthStore.clearPlatformAccessToken()
      return false
    }
  },
}