import { useEffect, useState } from "react"
import { authStore } from "../lib/authStore"
import { authService } from "../lib/authService"

export function useAuth() {
  const [snapshot, setSnapshot] = useState(authStore.getState())

  useEffect(() => {
    const unsubscribe = authStore.subscribe(() => {
      setSnapshot(authStore.getState())
    })

    return () => {
      unsubscribe()
    }
  }, [])

  return {
    ...snapshot,
    isAuthenticated: authStore.isAuthenticated,
    hasTenantContext: authStore.hasTenantContext,
    userEmail: authStore.userEmail,
    userName: authStore.userName,
    resolvedMemberships: authStore.resolvedMemberships,
    adminMemberships: authStore.adminMemberships,
    currentMembership: authStore.currentMembership,
    currentTenantName: authStore.currentTenantName,
    currentTenantPublicId: authStore.currentTenantPublicId,
    isAdmin: authStore.isAdmin,

    initialize: () => authService.initialize(),
    initializeProfile: () => authService.initializeProfile(),
    activateTenantForCurrentHost: () => authService.activateTenantForCurrentHost(),

    setAccountTokens: (accessToken: string, refreshToken: string) =>
      authStore.setAccountTokens(accessToken, refreshToken),
    setTenantAccessToken: (token: string) =>
      authStore.setTenantAccessToken(token),
    clearTenantAccessToken: () => authStore.clearTenantAccessToken(),
    setMemberships: (memberships: typeof snapshot.memberships) =>
      authStore.setMemberships(memberships),
    setProfile: (profile: typeof snapshot.profile) =>
      authStore.setProfile(profile),
    clearProfile: () => authStore.clearProfile(),
    logout: () => authStore.logout(),
  }
}