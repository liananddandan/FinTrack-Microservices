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
    userEmail: authStore.userEmail,
    userName: authStore.userName,
    resolvedMemberships: authStore.resolvedMemberships,
    adminMemberships: authStore.adminMemberships,
    initialize: () => authService.initialize(),
    initializeProfile: () => authService.initializeProfile(),
    setAccountTokens: (accessToken: string, refreshToken: string) =>
      authStore.setAccountTokens(accessToken, refreshToken),
    clearAccountTokens: () => authStore.clearAccountTokens(),
    setMemberships: (memberships: typeof snapshot.memberships) =>
      authStore.setMemberships(memberships),
    setProfile: (profile: typeof snapshot.profile) =>
      authStore.setProfile(profile),
    clearProfile: () => authStore.clearProfile(),
    logout: () => authStore.logout(),
  }
}