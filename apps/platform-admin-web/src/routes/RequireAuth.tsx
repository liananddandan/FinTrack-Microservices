import { Navigate, Outlet } from "react-router-dom"
import { platformAuthStore } from "../lib/platformAuthStore"

export default function RequireAuth() {
  if (!platformAuthStore.isAuthenticated || !platformAuthStore.hasPlatformContext) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}