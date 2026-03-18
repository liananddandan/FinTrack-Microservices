import { useEffect, useState } from "react"
import { Navigate } from "react-router-dom"
import { useAuth } from "../../hooks/useAuth"

type AuthGuardProps = {
  children: React.ReactNode
}

export default function AuthGuard({children}: AuthGuardProps) {
  const auth = useAuth()

  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function init() {
      await auth.initialize()
      setLoading(false)
    }

    void init()
  }, [])

  if (loading) {
    return <div>Loading...</div>
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (!auth.hasTenantContext) {
    return <Navigate to="/login" replace />
  }

  if (!auth.isAdmin) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}