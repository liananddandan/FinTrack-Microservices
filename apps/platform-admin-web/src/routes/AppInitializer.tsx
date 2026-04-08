import { useEffect, useState } from "react"
import { Outlet, Navigate } from "react-router-dom"
import { platformAuthStore } from "../lib/platformAuthStore"
import { accountApi } from "../lib/accountApi"

export default function AppInitializer() {
  const [loading, setLoading] = useState(true)
  const [authFailed, setAuthFailed] = useState(false)

  useEffect(() => {
    async function initialize() {
      const authState = platformAuthStore.getState()

      if (!authState.accountAccessToken || !authState.platformAccessToken) {
        setAuthFailed(true)
        setLoading(false)
        return
      }

      if (authState.profile) {
        setLoading(false)
        return
      }

      try {
        const profile = await accountApi.getCurrentUser()
        platformAuthStore.setProfile(profile)
      } catch (error) {
        console.error("[AppInitializer] failed to load current user profile", error)
        platformAuthStore.clearProfile()
        setAuthFailed(true)
      } finally {
        setLoading(false)
      }
    }

    void initialize()
  }, [])

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50">
        <div className="text-sm text-slate-500">Loading platform context...</div>
      </div>
    )
  }

  if (authFailed) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}