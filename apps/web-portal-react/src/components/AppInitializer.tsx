import { useEffect, useState } from "react"
import { Outlet } from "react-router-dom"
import { authStore } from "../lib/authStore"
import { tenantContextStore } from "../lib/tenantContextStore"

export default function AppInitializer() {
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function initialize() {
      try {
        await tenantContextStore.initialize()

        if (authStore.isAuthenticated) {
          await authStore.initializeProfile()

          if (tenantContextStore.hasTenantContext && !authStore.hasTenantContext) {
            await authStore.activateTenantForCurrentHost()
          }
        }
      } finally {
        setLoading(false)
      }
    }

    void initialize()
  }, [])

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50">
        <div className="text-sm text-slate-500">Loading...</div>
      </div>
    )
  }

  return <Outlet />
}