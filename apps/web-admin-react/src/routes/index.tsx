import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"

import Login from "../pages/Login"
import Overview from "../pages/Overview"
import Transactions from "../pages/Transactions"
import Members from "../pages/Members"
import Invitations from "../pages/Invitations"
import AuditLogs from "../pages/AuditLogs"
import TransactionDetail from "../pages/TransactionDetail"
import Menu from "../pages/Menu"
import OrderDetail from "../pages/OrderDetail"
import Orders from "../pages/Orders"

import AuthGuard from "./guards/AuthGuard"
import AdminLayout from "../components/AdminLayout"

export default function AppRouter() {
  return (
    <BrowserRouter basename="/admin">
      <Routes>
        {/* public */}
        <Route path="/login" element={<Login />} />

        {/* protected */}
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
          <Route path="transactions" element={<Transactions />} />
          <Route path="transactions/:transactionPublicId" element={<TransactionDetail />} />
          <Route path="orders" element={<Orders />} />
          <Route path="orders/:orderPublicId" element={<OrderDetail />} />
          <Route path="members" element={<Members />} />
          <Route path="invitations" element={<Invitations />} />
          <Route path="audit-logs" element={<AuditLogs />} />
          <Route path="menu" element={<Menu />} />
        </Route>

        {/* fallback */}
        <Route path="*" element={<Navigate to="/overview" replace />} />
      </Routes>
    </BrowserRouter>
  )
}