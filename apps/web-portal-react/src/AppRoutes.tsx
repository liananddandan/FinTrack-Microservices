import { Routes, Route, Navigate } from "react-router-dom"
import AuthGuard from "./components/AuthGuard"

import Login from "./pages/Login"
import RegisterUser from "./pages/RegisterUser"
import RegisterTenant from "./pages/RegisterTenant"
import WaitingMembership from "./pages/WaitingMembership"
import Home from "./pages/Home"
import RecordIncome from "./pages/RecordIncome"
import MyTransactions from "./pages/MyTransactions"
import TransactionDetail from "./pages/TransactionDetail"
import NewProcurement from "./pages/NewProcurement"
import AcceptInvitation from "./pages/AcceptInvitation"
import LandingPage from "./pages/LandingPage"
import Profile from "./pages/Profile"

export default function AppRoutes() {
    return (
        <Routes>
            {/* redirect */}
            <Route path="/" element={<LandingPage />} />

            <Route path="/portal" element={<Navigate to="/portal/login" replace />} />

            {/* public */}
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

            {/* auth only */}
            <Route
                path="/portal/waiting-membership"
                element={
                    <AuthGuard requireAuth>
                        <WaitingMembership />
                    </AuthGuard>
                }
            />

            {/* auth + tenant */}
            <Route
                path="/portal/home"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <Home />
                    </AuthGuard>
                }
            />

            <Route
                path="/portal/record-income"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <RecordIncome />
                    </AuthGuard>
                }
            />
            <Route
                path="/portal/my-transactions"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <MyTransactions />
                    </AuthGuard>
                }
            />
            <Route
                path="/portal/transactions/:transactionPublicId"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <TransactionDetail />
                    </AuthGuard>
                }
            />
            <Route
                path="/portal/procurements/new"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <NewProcurement />
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
                path="/portal/profile"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <Profile />
                    </AuthGuard>
                }
            />
        </Routes>
    )
}