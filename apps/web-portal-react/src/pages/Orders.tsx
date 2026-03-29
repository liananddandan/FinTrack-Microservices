import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { getOrders, type OrderListItemDto } from "../api/order"
import {
    HiOutlineClipboardDocumentList,
    HiOutlineFunnel,
    HiOutlineArrowPath,
    HiOutlineArrowRight,
    HiOutlineArrowLeft,
} from "react-icons/hi2"

type QueryState = {
    status: string
    pageNumber: number
    pageSize: number
}

function Badge({
    children,
    tone = "default",
}: {
    children: React.ReactNode
    tone?: "default" | "success" | "warning" | "danger"
}) {
    const className =
        tone === "success"
            ? "bg-emerald-50 text-emerald-700 border-emerald-200"
            : tone === "warning"
                ? "bg-amber-50 text-amber-700 border-amber-200"
                : tone === "danger"
                    ? "bg-rose-50 text-rose-700 border-rose-200"
                    : "bg-slate-100 text-slate-700 border-slate-200"

    return (
        <span
            className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-medium ${className}`}
        >
            {children}
        </span>
    )
}

export default function MyOrders() {
    const navigate = useNavigate()

    const [loading, setLoading] = useState(false)
    const [items, setItems] = useState<OrderListItemDto[]>([])
    const [totalCount, setTotalCount] = useState(0)
    const [errorMessage, setErrorMessage] = useState("")

    const [query, setQuery] = useState<QueryState>({
        status: "",
        pageNumber: 1,
        pageSize: 10,
    })

    useEffect(() => {
        void load(query)
    }, [])

    async function load(currentQuery: QueryState) {
        setLoading(true)
        setErrorMessage("")

        try {
            const result = await getOrders({
                createdByMe: true,
                status: currentQuery.status || undefined,
                pageNumber: currentQuery.pageNumber,
                pageSize: currentQuery.pageSize,
            })

            setItems(result.items)
            setTotalCount(result.totalCount)
        } catch (err: unknown) {
            if (err instanceof Error) {
                setErrorMessage(err.message || "Failed to load orders.")
            } else {
                setErrorMessage("Failed to load orders.")
            }
        } finally {
            setLoading(false)
        }
    }

    function updateQuery<K extends keyof QueryState>(
        key: K,
        value: QueryState[K]
    ) {
        setQuery((prev) => ({
            ...prev,
            [key]: value,
        }))
    }

    async function handleSearch() {
        const nextQuery = {
            ...query,
            pageNumber: 1,
        }

        setQuery(nextQuery)
        await load(nextQuery)
    }

    async function handlePageChange(pageNumber: number) {
        const nextQuery = {
            ...query,
            pageNumber,
        }

        setQuery(nextQuery)
        await load(nextQuery)
    }

    function resetFilters() {
        const nextQuery: QueryState = {
            status: "",
            pageNumber: 1,
            pageSize: 10,
        }

        setQuery(nextQuery)
        void load(nextQuery)
    }

    function formatAmount(value: number) {
        return new Intl.NumberFormat("en-NZ", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
        }).format(value)
    }

    function formatDateTime(value: string) {
        if (!value) return "-"
        const date = new Date(value)
        if (Number.isNaN(date.getTime())) return value
        return date.toLocaleString()
    }

    function statusTone(status: string) {
        switch (status) {
            case "Completed":
                return "success"
            case "Pending":
                return "warning"
            case "Cancelled":
                return "danger"
            default:
                return "default"
        }
    }

    function paymentTone(status: string) {
        switch (status) {
            case "Paid":
                return "success"
            case "Pending":
                return "warning"
            case "Failed":
                return "danger"
            default:
                return "default"
        }
    }

    function goOrderDetail(row: OrderListItemDto) {
        navigate(`/portal/orders/${row.publicId}`)
    }

    const totalPages = Math.max(1, Math.ceil(totalCount / query.pageSize))

    return (
        <div className="min-h-screen bg-slate-50 px-6 py-10">
            <div className="mx-auto flex max-w-6xl flex-col gap-6">
                <div className="flex justify-start">
                    <button
                        type="button"
                        onClick={() => navigate("/portal")}
                        className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
                    >
                        <HiOutlineArrowLeft className="h-4 w-4" />
                        Back to dashboard
                    </button>
                </div>
                <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
                    <div className="flex items-start gap-4">
                        <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                            <HiOutlineClipboardDocumentList className="h-6 w-6" />
                        </div>

                        <div>
                            <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
                                My orders
                            </h1>
                            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                                Review the orders you created in the current workspace.
                            </p>
                        </div>
                    </div>
                </section>

                <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
                    <div className="mb-4 flex items-center gap-2 text-slate-800">
                        <HiOutlineFunnel className="h-5 w-5 text-slate-500" />
                        <h2 className="text-base font-semibold">Filters</h2>
                    </div>

                    <div className="grid gap-3 md:grid-cols-[1fr_auto_auto]">
                        <select
                            value={query.status}
                            onChange={(e) => updateQuery("status", e.target.value)}
                            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                        >
                            <option value="">Order status</option>
                            <option value="Pending">Pending</option>
                            <option value="Completed">Completed</option>
                            <option value="Cancelled">Cancelled</option>
                        </select>

                        <button
                            type="button"
                            onClick={() => void handleSearch()}
                            className="inline-flex h-11 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500"
                        >
                            Search
                        </button>

                        <button
                            type="button"
                            onClick={resetFilters}
                            className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
                        >
                            <HiOutlineArrowPath className="h-4 w-4" />
                            Reset
                        </button>
                    </div>
                </section>

                {loading ? (
                    <div className="text-sm text-slate-500">Loading orders...</div>
                ) : null}

                {errorMessage ? (
                    <div
                        className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                        role="alert"
                    >
                        {errorMessage}
                    </div>
                ) : null}

                <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
                    <div className="mb-4">
                        <h2 className="text-base font-semibold text-slate-800">
                            Order list
                        </h2>
                        <p className="mt-1 text-sm text-slate-500">
                            Page {query.pageNumber} of {totalPages} · Total {totalCount}
                        </p>
                    </div>

                    <div className="overflow-x-auto rounded-2xl border border-slate-200">
                        <table className="min-w-full border-collapse text-left">
                            <thead className="bg-slate-50">
                                <tr className="text-sm text-slate-600">
                                    <th className="px-4 py-3 font-medium">Order</th>
                                    <th className="px-4 py-3 font-medium">Customer</th>
                                    <th className="px-4 py-3 font-medium">Amount</th>
                                    <th className="px-4 py-3 font-medium">Status</th>
                                    <th className="px-4 py-3 font-medium">Payment</th>
                                    <th className="px-4 py-3 font-medium">Created at</th>
                                    <th className="px-4 py-3 font-medium">View</th>
                                </tr>
                            </thead>

                            <tbody>
                                {items.length === 0 ? (
                                    <tr>
                                        <td
                                            colSpan={7}
                                            className="px-4 py-10 text-center text-sm text-slate-500"
                                        >
                                            No orders found.
                                        </td>
                                    </tr>
                                ) : (
                                    items.map((row) => (
                                        <tr
                                            key={row.publicId}
                                            className="border-t border-slate-200 hover:bg-slate-50"
                                        >
                                            <td className="px-4 py-4 text-sm font-medium text-slate-800">
                                                {row.orderNumber}
                                            </td>

                                            <td className="px-4 py-4 text-sm text-slate-600">
                                                {row.customerName || "Walk-in"}
                                            </td>

                                            <td className="px-4 py-4 text-sm font-medium text-slate-800">
                                                {formatAmount(row.totalAmount)}
                                            </td>

                                            <td className="px-4 py-4">
                                                <Badge tone={statusTone(row.status)}>
                                                    {row.status}
                                                </Badge>
                                            </td>

                                            <td className="px-4 py-4">
                                                <Badge tone={paymentTone(row.paymentStatus)}>
                                                    {row.paymentStatus}
                                                </Badge>
                                            </td>

                                            <td className="px-4 py-4 text-sm text-slate-600">
                                                {formatDateTime(row.createdAt)}
                                            </td>

                                            <td className="px-4 py-4">
                                                <button
                                                    type="button"
                                                    onClick={() => goOrderDetail(row)}
                                                    className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
                                                >
                                                    View
                                                    <HiOutlineArrowRight className="h-4 w-4" />
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>

                    <div className="mt-5 flex items-center justify-between gap-4">
                        <button
                            type="button"
                            disabled={query.pageNumber <= 1}
                            onClick={() => void handlePageChange(query.pageNumber - 1)}
                            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            Previous
                        </button>

                        <span className="text-sm text-slate-500">
                            Page {query.pageNumber} of {totalPages}
                        </span>

                        <button
                            type="button"
                            disabled={query.pageNumber >= totalPages}
                            onClick={() => void handlePageChange(query.pageNumber + 1)}
                            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            Next
                        </button>
                    </div>
                </section>
            </div>
        </div>
    )
}