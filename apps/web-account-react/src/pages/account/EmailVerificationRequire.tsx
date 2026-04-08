import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { accountApi } from "../../lib/accountApi"

type Status = "idle" | "sending" | "success" | "error"

export default function EmailVerificationRequired() {
  const navigate = useNavigate()

  const [status, setStatus] = useState<Status>("idle")
  const [message, setMessage] = useState(
    "Please verify your email address before continuing."
  )

  const handleResend = async () => {
    try {
      setStatus("sending")
      setMessage("Sending verification email...")

      await accountApi.resendVerificationEmail()

      setStatus("success")
      setMessage(
        "A new verification email has been sent. Please check your inbox."
      )
    } catch (error: any) {
      const apiMessage =
        error?.response?.data?.message ||
        error?.message ||
        "Failed to resend verification email."

      setStatus("error")
      setMessage(apiMessage)
    }
  }

  const handleRefresh = async () => {
    try {
      const currentUser = await accountApi.getCurrentUser()

      if (currentUser.emailConfirmed) {
        navigate("/account/home")
        return
      }

      setStatus("error")
      setMessage(
        "Your email is still not verified. Please check your inbox and click the verification link first."
      )
    } catch {
      setStatus("error")
      setMessage("Failed to refresh account status.")
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <div className="space-y-4 text-center">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-amber-100 text-2xl text-amber-600">
            ✉
          </div>

          <div>
            <h1 className="text-2xl font-semibold text-slate-900">
              Verify Your Email
            </h1>
            <p className="mt-3 text-sm text-slate-600">{message}</p>
          </div>

          <div className="space-y-3 pt-2">
            <button
              type="button"
              onClick={handleResend}
              disabled={status === "sending"}
              className="inline-flex w-full items-center justify-center rounded-lg bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {status === "sending"
                ? "Sending..."
                : "Resend Verification Email"}
            </button>

            <button
              type="button"
              onClick={handleRefresh}
              className="inline-flex w-full items-center justify-center rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              I've Already Verified My Email
            </button>

            <Link
              to="/account/login"
              className="inline-flex w-full items-center justify-center rounded-lg text-sm font-medium text-slate-500 transition hover:text-slate-700"
            >
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}