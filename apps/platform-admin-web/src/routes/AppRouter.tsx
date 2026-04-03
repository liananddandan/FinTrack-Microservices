import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"
import Login from "../pages/Login"
import Overview from "../pages/Overview"
import Tenants from "../pages/Tenants"
import TenantConfig from "../pages/TenantConfig"
import RequireAuth from "./RequireAuth"
import AppShell from "../components/AppShell"

export default function AppRouter() {
  return (
    <BrowserRouter basename="/admin">
      <Routes>
        <Route path="/login" element={<Login />} />

        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/overview" element={<Overview />} />
            <Route path="/tenants" element={<Tenants />} />
            <Route path="/tenants/:tenantPublicId" element={<TenantConfig />} />
          </Route>
        </Route>

        <Route path="/" element={<Navigate to="/overview" replace />} />
        <Route path="*" element={<Navigate to="/overview" replace />} />
      </Routes>
    </BrowserRouter>
  )
}