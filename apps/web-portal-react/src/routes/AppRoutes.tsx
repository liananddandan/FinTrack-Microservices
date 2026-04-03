import { Routes, Route, Navigate } from "react-router-dom"
import AuthGuard from "../components/AuthGuard"
import AppInitializer from "../components/AppInitializer"

import Login from "../pages/Login"
import RegisterUser from "../pages/RegisterUser"
import RegisterTenant from "../pages/RegisterTenant"
import WaitingMembership from "../pages/WaitingMembership"
import Home from "../pages/Home"
import AcceptInvitation from "../pages/AcceptInvitation"
import LandingPage from "../pages/LandingPage"
import Profile from "../pages/Profile"
import Orders from "../pages/Orders"
import OrderDetail from "../pages/OrderDetail"

function getHostName() {
  return window.location.hostname.toLowerCase()
}

function isLandingHost() {
  const host = getHostName()

  return (
    host === "fintrack.chenlis.com" ||
    host === "fintrack.chenlis.local" ||
    host === "localhost"
  )
}

export default function AppRoutes() {
  const landingHost = isLandingHost()

  return (
    <Routes>
      <Route element={<AppInitializer />}>
        {landingHost && <Route path="/" element={<LandingPage />} />}

        {!landingHost && (
          <Route path="/" element={<Navigate to="/portal/login" replace />} />
        )}

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
          path="/portal/register-user"
          element={
            <AuthGuard public>
              <RegisterUser />
            </AuthGuard>
          }
        />

        <Route
          path="/portal/register-tenant"
          element={
            <AuthGuard public>
              <RegisterTenant />
            </AuthGuard>
          }
        />

        <Route
          path="/portal/invitations/accept"
          element={
            <AuthGuard public>
              <AcceptInvitation />
            </AuthGuard>
          }
        />

        <Route
          path="/portal/waiting-membership"
          element={
            <AuthGuard requireAuth>
              <WaitingMembership />
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
          path="/portal/profile"
          element={
            <AuthGuard requireAuth requireTenant>
              <Profile />
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

        <Route
          path="*"
          element={<Navigate to={landingHost ? "/" : "/portal/login"} replace />}
        />
      </Route>
    </Routes>
  )
}