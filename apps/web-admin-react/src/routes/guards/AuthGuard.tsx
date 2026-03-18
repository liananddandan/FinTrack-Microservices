import { Children, useEffect, useState } from "react"
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

  // ⛔ 初始化中（等价 await auth.initialize）
  if (loading) {
    return <div>Loading...</div>
  }

  // ⛔ 未登录
  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // ⛔ 没 tenant
  if (!auth.hasTenantContext) {
    return <Navigate to="/login" replace />
  }

  // ⛔ 不是 admin
  if (!auth.isAdmin) {
    return <Navigate to="/login" replace />
  }

  // ✅ 通过
  return <>{children}</>
}