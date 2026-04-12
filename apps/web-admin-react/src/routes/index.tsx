import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"

import Login from "../pages/Login"
import Overview from "../pages/Overview"
import Members from "../pages/Members"
import Invitations from "../pages/Invitations"
import AuditLogs from "../pages/AuditLogs"
import Menu from "../pages/Menu"
import OrderDetail from "../pages/OrderDetail"
import Orders from "../pages/Orders"

import AuthGuard from "./guards/AuthGuard"
import AdminLayout from "../components/AdminLayout"
import AppInitializer from "../components/AppInitializer"
import PaymentsPage from "../pages/PaymentsPage"

export default function AppRouter() {
  return (
    <BrowserRouter basename="/admin">
      <Routes>
        <Route element={<AppInitializer />}>
          <Route path="/login" element={<Login />} />

          <Route
            path="/"
            element={
              <AuthGuard>
                <AdminLayout />
              </AuthGuard>
            }
          >
            <Route index element={<Navigate to="overview" replace />} />
            <Route path="overview" element={<Overview />} />
            <Route path="orders" element={<Orders />} />
            <Route path="orders/:orderPublicId" element={<OrderDetail />} />
            <Route path="members" element={<Members />} />
            <Route path="invitations" element={<Invitations />} />
            <Route path="audit-logs" element={<AuditLogs />} />
            <Route path="menu" element={<Menu />} />
            <Route path="payments" element={<PaymentsPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/login" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}