import { useEffect, useState } from "react"
import { authStore } from "../lib/authStore"

export function useAuth() {
  const [snapshot, setSnapshot] = useState(authStore.getState())

  useEffect(() => {
    const unsubscribe = authStore.subscribe(() => {
      setSnapshot(authStore.getState())
    })

    return unsubscribe
  }, [])

  return {
    ...snapshot,
    isAuthenticated: authStore.isAuthenticated,
    hasTenantContext: authStore.hasTenantContext,
    userEmail: authStore.userEmail,
    userName: authStore.userName,
    resolvedMemberships: authStore.resolvedMemberships,
    currentMembership: authStore.currentMembership,
    currentTenantName: authStore.currentTenantName,
    currentTenantPublicId: authStore.currentTenantPublicId,
    isAdmin: authStore.isAdmin,
    initialize: authStore.initialize.bind(authStore),
    clearProfile: authStore.clearProfile.bind(authStore),
    setAccountTokens: authStore.setAccountTokens.bind(authStore),
    setTenantAccessToken: authStore.setTenantAccessToken.bind(authStore),
    clearTenantAccessToken: authStore.clearTenantAccessToken.bind(authStore),
    setMemberships: authStore.setMemberships.bind(authStore),
    setProfile: authStore.setProfile.bind(authStore),
    activateSingleTenantIfPossible: authStore.activateSingleTenantIfPossible.bind(authStore),
    logout: authStore.logout.bind(authStore),
  }
}