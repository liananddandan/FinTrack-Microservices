import { useEffect, useState } from "react"
import { Outlet } from "react-router-dom"
import { authStore } from "../lib/authStore"
import { authService } from "../lib/authService"
import { tenantContextStore } from "../lib/tenantContextStore"


export default function AppInitializer() {
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function initialize() {
      try {
        await tenantContextStore.initialize()

        if (!tenantContextStore.hasTenantContext) {
          setLoading(false)
          return (
            <div className="flex min-h-screen items-center justify-center bg-slate-50 p-8">
              <div className="rounded-2xl border border-amber-200 bg-amber-50 p-6 text-sm text-amber-800">
                Tenant context not found for current host.
              </div>
            </div>
          )
        }

        if (authStore.isAuthenticated) {
          await authService.initializeProfile()

          if (!authStore.hasTenantContext) {
            await authService.activateTenantForCurrentHost()
          }
        }

        setLoading(false)
      } catch {
        setLoading(false)
      }
    }

    void initialize()
  }, [])

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50">
        <div className="text-sm text-slate-500">
          {loading ? "Loading..." : "Redirecting..."}
        </div>
      </div>
    )
  }

  return <Outlet />
}