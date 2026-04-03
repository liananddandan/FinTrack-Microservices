import { Routes, Route, Navigate } from "react-router-dom"
import AuthGuard from "../components/AuthGuard"
import AppInitializer from "../components/AppInitializer"

import Login from "../pages/Login"
import Home from "../pages/Home"
import Orders from "../pages/Orders"
import OrderDetail from "../pages/OrderDetail"

export default function AppRoutes() {

  return (
    <Routes>
      <Route element={<AppInitializer />}>
        <Route path="/" element={<Navigate to="/portal/login" replace />} />
        <Route path="/portal" element={<Navigate to="/portal/login" replace />} />
        <Route
          path="/portal/login"
          element={
            <AuthGuard public>
              <Login />
            </AuthGuard>
          }
        />


        <Route
          path="/portal/home"
          element={
            <AuthGuard requireAuth requireTenant>
              <Home />
            </AuthGuard>
          }
        />

        <Route
          path="/portal/orders"
          element={
            <AuthGuard requireAuth requireTenant>
              <Orders />
            </AuthGuard>
          }
        />

        <Route
          path="/portal/orders/:orderPublicId"
          element={
            <AuthGuard requireAuth requireTenant>
              <OrderDetail />
            </AuthGuard>
          }
        />
        <Route path="*" element={<Navigate to="/portal/login" replace />} />

      </Route>
    </Routes>
  )
}