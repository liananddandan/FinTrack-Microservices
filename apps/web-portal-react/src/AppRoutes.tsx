import { Routes, Route, Navigate } from "react-router-dom"
import AuthGuard from "./components/AuthGuard"

import Login from "./pages/Login"
// import RegisterUser from "./pages/RegisterUser"
// import RegisterTenant from "./pages/RegisterTenant"
// import WaitingMembership from "./pages/WaitingMembership"
import Home from "./pages/Home"

export default function AppRoutes() {
  return (
    <Routes>
      {/* redirect */}
      <Route path="/" element={<Navigate to="/login" />} />

      {/* public */}
      <Route
        path="/login"
        element={
          <AuthGuard public>
            <Login />
          </AuthGuard>
        }
      />

      {/* <Route
        path="/register-user"
        element={
          <AuthGuard public>
            <RegisterUser />
          </AuthGuard>
        }
      /> */}

      {/* <Route
        path="/register-tenant"
        element={
          <AuthGuard public>
            <RegisterTenant />
          </AuthGuard>
        }
      /> */}

      {/* auth only */}
      {/* <Route
        path="/waiting-membership"
        element={
          <AuthGuard requireAuth>
            <WaitingMembership />
          </AuthGuard>
        }
      /> */}

      {/* auth + tenant */}
      <Route
        path="/home"
        element={
          <AuthGuard requireAuth requireTenant>
            <Home />
          </AuthGuard>
        }
      />
    </Routes>
  )
}