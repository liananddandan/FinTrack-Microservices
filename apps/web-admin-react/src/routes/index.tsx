import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"

import Login from "../pages/Login"
import Overview from "../pages/Overview"
import Transactions from "../pages/Transactions"
import Members from "../pages/Members"
import Invitations from "../pages/Invitations"
import AuditLogs from "../pages/AuditLogs"

import AuthGuard from "./guards/AuthGuard"
import AdminLayout from "../components/AdminLayout"

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>

        {/* public */}
        <Route path="/login" element={<Login />} />

        {/* protected */}
        <Route
          path="/admin"
          element={
            <AuthGuard>
              <AdminLayout />
            </AuthGuard>
          }
        >
          <Route path="overview" element={<Overview />} />

          {/* 先占位，后面再实现 */}
          <Route path="transactions" element={<Transactions />} />
          <Route path="members" element={<Members />} />
          <Route path="invitations" element={<Invitations />} />
          <Route path="audit-logs" element={<AuditLogs />} />
        </Route>

        {/* 默认跳转 */}
        <Route path="/" element={<Navigate to="/admin/overview" replace />} />

        {/* 兜底 */}
        <Route path="*" element={<Navigate to="/admin/overview" replace />} />

      </Routes>
    </BrowserRouter>
  )
}