import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { HiOutlineShieldCheck, HiOutlineSparkles } from "react-icons/hi2"
import { accountApi } from "../lib/accountApi"
import { platformAuthStore } from "../lib/platformAuthStore"

type LoginForm = {
    email: string
    password: string
}

const BOOTSTRAP_ADMIN = {
    email: "platform-admin@fintrack.local",
    password: "Admin123!",
    role: "SuperAdmin",
}

export default function Login() {
    const navigate = useNavigate()

    const [form, setForm] = useState<LoginForm>({
        email: "",
        password: "",
    })

    const [loading, setLoading] = useState(false)
    const [errorMessage, setErrorMessage] = useState("")

    function updateField<K extends keyof LoginForm>(key: K, value: LoginForm[K]) {
        setForm((prev) => ({
            ...prev,
            [key]: value,
        }))
    }

    function fillBootstrapAdmin() {
        setForm({
            email: BOOTSTRAP_ADMIN.email,
            password: BOOTSTRAP_ADMIN.password,
        })
        setErrorMessage("")
    }

    async function onLogin(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault()
        setErrorMessage("")

        if (!form.email.trim()) {
            setErrorMessage("Email is required.")
            return
        }

        if (!form.password.trim()) {
            setErrorMessage("Password is required.")
            return
        }

        setLoading(true)

        try {
            const loginResult = await accountApi.login({
                email: form.email.trim(),
                password: form.password,
            })

            if (
                !loginResult.tokens?.accessToken ||
                !loginResult.tokens?.refreshToken
            ) {
                throw new Error("Login token response is invalid.")
            }

            platformAuthStore.setAccountTokens(
                loginResult.tokens.accessToken,
                loginResult.tokens.refreshToken
            )

            const profile = await accountApi.getCurrentUser()
            platformAuthStore.setProfile(profile)

            const platformResult = await accountApi.selectPlatform()

            if (
                !platformResult.platformAccessToken ||
                !platformResult.platformRole
            ) {
                throw new Error("Platform access token response is invalid.")
            }

            platformAuthStore.setPlatformAccessToken(
                platformResult.platformAccessToken,
                platformResult.platformRole
            )

            navigate("/overview", { replace: true })
        } catch (err: unknown) {
            platformAuthStore.logout()

            if (typeof err === "object" && err !== null && "response" in err) {
                const maybeAxiosError = err as {
                    response?: {
                        data?: {
                            message?: string
                        }
                    }
                    message?: string
                }

                setErrorMessage(
                    maybeAxiosError.response?.data?.message ??
                    maybeAxiosError.message ??
                    "Login failed."
                )
            } else if (err instanceof Error) {
                setErrorMessage(err.message)
            } else {
                setErrorMessage("Login failed.")
            }
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="min-h-screen bg-slate-50 px-6 py-10">
            <div className="mx-auto grid max-w-6xl gap-10 lg:grid-cols-[1fr_1fr]">
                <section className="px-2 py-2 sm:px-4">
                    <div className="flex items-center gap-4">
                        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
                            <HiOutlineShieldCheck className="h-7 w-7" />
                        </div>

                        <div>
                            <p className="text-lg font-semibold text-slate-800">
                                FinTrack Platform Admin
                            </p>
                            <p className="mt-1 text-sm text-slate-500">
                                Global administration workspace
                            </p>
                        </div>
                    </div>

                    <div className="mt-8 flex items-center gap-2">
                        <HiOutlineSparkles className="h-5 w-5 text-indigo-600" />
                        <h1 className="text-2xl font-semibold text-slate-800">
                            Bootstrap administrator
                        </h1>
                    </div>

                    <p className="mt-3 text-sm leading-6 text-slate-500">
                        Use the default bootstrap super administrator account for initial platform access.
                    </p>

                    <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                        <div className="text-sm font-semibold text-slate-800">
                            Super admin account
                        </div>

                        <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
                            <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                                Email
                            </div>
                            <div className="mt-1 text-sm text-slate-800">
                                {BOOTSTRAP_ADMIN.email}
                            </div>

                            <div className="mt-4 text-xs font-medium uppercase tracking-wide text-slate-500">
                                Password
                            </div>
                            <div className="mt-1 text-sm text-slate-800">
                                {BOOTSTRAP_ADMIN.password}
                            </div>

                            <div className="mt-4 text-xs font-medium uppercase tracking-wide text-slate-500">
                                Platform role
                            </div>
                            <div className="mt-1 text-sm text-slate-800">
                                {BOOTSTRAP_ADMIN.role}
                            </div>

                            <button
                                type="button"
                                onClick={fillBootstrapAdmin}
                                className="mt-5 inline-flex h-10 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500"
                            >
                                Use bootstrap admin
                            </button>
                        </div>
                    </div>
                </section>

                <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
                    <div>
                        <h2 className="text-2xl font-semibold text-slate-800">
                            Platform sign in
                        </h2>
                        <p className="mt-2 text-sm leading-6 text-slate-500">
                            Sign in with your platform administrator credentials.
                        </p>
                    </div>

                    <form className="mt-8 space-y-5" onSubmit={onLogin}>
                        <div>
                            <label
                                htmlFor="email"
                                className="mb-2 block text-sm font-medium text-slate-700"
                            >
                                Email
                            </label>
                            <input
                                id="email"
                                type="email"
                                value={form.email}
                                onChange={(e) => updateField("email", e.target.value)}
                                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="password"
                                className="mb-2 block text-sm font-medium text-slate-700"
                            >
                                Password
                            </label>
                            <input
                                id="password"
                                type="password"
                                value={form.password}
                                onChange={(e) => updateField("password", e.target.value)}
                                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                            />
                        </div>

                        <button
                            type="submit"
                            disabled={loading}
                            className="inline-flex h-11 w-full items-center justify-center rounded-xl bg-indigo-600 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
                        >
                            {loading ? "Signing in..." : "Sign in"}
                        </button>

                        {errorMessage ? (
                            <div
                                className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                                role="alert"
                            >
                                {errorMessage}
                            </div>
                        ) : null}
                    </form>
                </section>
            </div>
        </div>
    )
}