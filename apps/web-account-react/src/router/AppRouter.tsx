import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"
import Landing from "../pages/LandingPage"
import Login from "../pages/account/Login"
import Register from "../pages/account/Register"
import VerifyEmail from "../pages/account/VerifyEmail"
import AcceptInvitation from "../pages/account/AcceptInvitation"
import RegisterTenant from "../pages/account/RegisterTenant"
import Home from "../pages/account/Home"
import AccountProfile from "../pages/account/AccountProfile"

export default function AppRouter() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Landing />} />
                <Route path="/account" element={<Navigate to="/account/login" replace />} />
                <Route path="/account/login" element={<Login />} />
                <Route path="/account/home" element={<Home />} />
                <Route path="/account/register-user" element={<Register />} />
                <Route path="/account/verify-email" element={<VerifyEmail />} />
                <Route path="/account/accept-invitation" element={<AcceptInvitation />} />
                <Route path="/account/register-tenant" element={<RegisterTenant />} />
                <Route path="/account/profile" element={<AccountProfile />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </BrowserRouter>
    )
}