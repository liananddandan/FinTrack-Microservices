import { useEffect, useState } from "react"
import { platformAuthStore } from "./platformAuthStore"
import { platformAuthService } from "./platformAuthService"

export function usePlatformAuth() {
  const [snapshot, setSnapshot] = useState(platformAuthStore.getState())

  useEffect(() => {
    const unsubscribe = platformAuthStore.subscribe(() => {
      setSnapshot(platformAuthStore.getState())
    })

    return () => {
      unsubscribe()
    }
  }, [])

  return {
    ...snapshot,
    isAuthenticated: platformAuthStore.isAuthenticated,
    hasPlatformContext: platformAuthStore.hasPlatformContext,
    platformRole: platformAuthStore.platformRole,
    userEmail: platformAuthStore.userEmail,
    userName: platformAuthStore.userName,
    initialize: () => platformAuthService.initialize(),
    initializeProfile: () => platformAuthService.initializeProfile(),
    activatePlatformContext: () => platformAuthService.activatePlatformContext(),
    setAccountTokens: (accessToken: string, refreshToken: string) =>
      platformAuthStore.setAccountTokens(accessToken, refreshToken),
    setPlatformAccessToken: (token: string, platformRole: string) =>
      platformAuthStore.setPlatformAccessToken(token, platformRole),
    clearPlatformAccessToken: () => platformAuthStore.clearPlatformAccessToken(),
    setProfile: (profile: typeof snapshot.profile) =>
      platformAuthStore.setProfile(profile),
    clearProfile: () => platformAuthStore.clearProfile(),
    logout: () => platformAuthStore.logout(),
  }
}