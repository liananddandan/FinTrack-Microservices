import type { ReactNode } from "react"
import { Navigate, useLocation } from "react-router-dom"
import { authStore } from "../lib/authStore"

type Props = {
  children: ReactNode
  requireAuth?: boolean
  requireTenant?: boolean
  public?: boolean
}

export default function AuthGuard({
  children,
  requireAuth,
  requireTenant,
  public: isPublic,
}: Props) {
  const location = useLocation()

  const { accountAccessToken, tenantAccessToken } = authStore.getState()

  if (isPublic) {
    return <>{children}</>
  }

  if (requireAuth && !accountAccessToken) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  if (requireTenant && !tenantAccessToken) {
    return <Navigate to="/waiting-membership" replace />
  }

  return <>{children}</>
}