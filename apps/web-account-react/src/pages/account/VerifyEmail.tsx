import { useEffect, useRef, useState } from "react"
import { Link, useNavigate, useSearchParams } from "react-router-dom"
import { accountApi } from "../../lib/accountApi"

type VerifyStatus = "loading" | "success" | "error"

export default function VerifyEmail() {
    const [searchParams] = useSearchParams()
    const navigate = useNavigate()
    const hasRequestedRef = useRef(false)

    const [status, setStatus] = useState<VerifyStatus>("loading")
    const [message, setMessage] = useState("Verifying your email address...")

    useEffect(() => {
        if (hasRequestedRef.current) return
        hasRequestedRef.current = true

        const token = searchParams.get("token")

        if (!token) {
            setStatus("error")
            setMessage("Verification token is missing.")
            return
        }

        const runVerification = async () => {
            try {
                await accountApi.verifyEmail({ token })

                setStatus("success")
                setMessage("Your email has been verified successfully.")

                setTimeout(() => {
                    navigate("/account/login")
                }, 3000)
            } catch (error: any) {
                const apiMessage =
                    error?.response?.data?.message ||
                    error?.message ||
                    "Unable to verify your email. The link may be invalid or expired."

                setStatus("error")
                setMessage(apiMessage)
            }
        }

        runVerification()
    }, [navigate, searchParams])

    return (
        <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
            <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
                <div className="mb-6 text-center">
                    <h1 className="text-2xl font-semibold text-slate-900">
                        Email Verification
                    </h1>
                </div>

                {status === "loading" && (
                    <div className="space-y-4 text-center">
                        <div className="mx-auto h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-slate-900" />
                        <p className="text-sm text-slate-600">{message}</p>
                    </div>
                )}

                {status === "success" && (
                    <div className="space-y-4 text-center">
                        <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100 text-xl text-green-600">
                            ✓
                        </div>

                        <div>
                            <h2 className="text-lg font-medium text-slate-900">
                                Verification Successful
                            </h2>
                            <p className="mt-2 text-sm text-slate-600">{message}</p>
                            <p className="mt-2 text-xs text-slate-500">
                                Redirecting to login page...
                            </p>
                        </div>

                        <Link
                            to="/account/login"
                            className="inline-flex w-full items-center justify-center rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
                        >
                            Go to Login
                        </Link>
                    </div>
                )}

                {status === "error" && (
                    <div className="space-y-4 text-center">
                        <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100 text-xl text-red-600">
                            !
                        </div>

                        <div>
                            <h2 className="text-lg font-medium text-slate-900">
                                Verification Failed
                            </h2>
                            <p className="mt-2 text-sm text-slate-600">{message}</p>
                        </div>

                        <div className="space-y-2">
                            <Link
                                to="/account/login"
                                className="inline-flex w-full items-center justify-center rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
                            >
                                Back to Login
                            </Link>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}