import { useEffect, useState } from "react"
import {
    HiOutlineQueueList,
    HiOutlinePlus,
    HiOutlineTrash,
    HiOutlineCube,
} from "react-icons/hi2"
import {
    createProductCategory,
    deleteProductCategory,
    getProductCategories,
    type ProductCategoryItem,
} from "../api/product-category"

type CreateCategoryFormState = {
    name: string
    displayOrder: string
}

export default function Menu() {
    const [loading, setLoading] = useState(false)
    const [submitting, setSubmitting] = useState(false)
    const [categories, setCategories] = useState<ProductCategoryItem[]>([])
    const [errorMessage, setErrorMessage] = useState("")
    const [successMessage, setSuccessMessage] = useState("")

    const [form, setForm] = useState<CreateCategoryFormState>({
        name: "",
        displayOrder: "1",
    })

    useEffect(() => {
        void loadCategories()
    }, [])

    async function loadCategories() {
        setLoading(true)
        setErrorMessage("")

        try {
            const result = await getProductCategories()
            setCategories(result)
        } catch (error: unknown) {
            if (error instanceof Error) {
                setErrorMessage(error.message || "Failed to load categories.")
            } else {
                setErrorMessage("Failed to load categories.")
            }
        } finally {
            setLoading(false)
        }
    }

    function updateForm<K extends keyof CreateCategoryFormState>(
        key: K,
        value: CreateCategoryFormState[K]
    ) {
        setForm((prev) => ({
            ...prev,
            [key]: value,
        }))
    }

    async function handleCreateCategory(e: React.FormEvent) {
        e.preventDefault()

        setSubmitting(true)
        setErrorMessage("")
        setSuccessMessage("")

        try {
            await createProductCategory({
                name: form.name.trim(),
                displayOrder: Number(form.displayOrder || 1),
            })

            setSuccessMessage("Category created successfully.")
            setForm({
                name: "",
                displayOrder: "1",
            })

            await loadCategories()
        } catch (error: unknown) {
            if (error instanceof Error) {
                setErrorMessage(error.message || "Failed to create category.")
            } else {
                setErrorMessage("Failed to create category.")
            }
        } finally {
            setSubmitting(false)
        }
    }

    async function handleDeleteCategory(publicId: string) {
        const confirmed = window.confirm(
            "Are you sure you want to delete this category?"
        )

        if (!confirmed) {
            return
        }

        setErrorMessage("")
        setSuccessMessage("")

        try {
            await deleteProductCategory(publicId)
            setSuccessMessage("Category deleted successfully.")
            await loadCategories()
        } catch (error: unknown) {
            if (error instanceof Error) {
                setErrorMessage(error.message || "Failed to delete category.")
            } else {
                setErrorMessage("Failed to delete category.")
            }
        }
    }

    return (
        <div className="mx-auto flex max-w-6xl flex-col gap-6">
            {/* Header */}
            <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
                <div className="flex items-start gap-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                        <HiOutlineQueueList className="h-6 w-6" />
                    </div>

                    <div>
                        <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
                            Menu
                        </h1>
                        <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                            Manage menu categories and products for the current tenant. Start
                            by creating categories, then add products under each category.
                        </p>
                    </div>
                </div>
            </section>

            {errorMessage ? (
                <div
                    className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                    role="alert"
                >
                    {errorMessage}
                </div>
            ) : null}

            {successMessage ? (
                <div
                    className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
                    role="status"
                >
                    {successMessage}
                </div>
            ) : null}

            {/* Create category */}
            <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
                <div className="mb-4 flex items-center gap-2 text-slate-800">
                    <HiOutlinePlus className="h-5 w-5 text-slate-500" />
                    <h2 className="text-base font-semibold">Create category</h2>
                </div>

                <form onSubmit={(e) => void handleCreateCategory(e)} className="space-y-4">
                    <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_220px]">
                        <div>
                            <label className="mb-2 block text-sm font-medium text-slate-700">
                                Category name
                            </label>
                            <input
                                type="text"
                                value={form.name}
                                onChange={(e) => updateForm("name", e.target.value)}
                                placeholder="e.g. Coffee, Tea, Dessert"
                                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                            />
                        </div>

                        <div>
                            <label className="mb-2 block text-sm font-medium text-slate-700">
                                Order in menu
                            </label>
                            <input
                                type="number"
                                min={1}
                                value={form.displayOrder}
                                onChange={(e) => updateForm("displayOrder", e.target.value)}
                                placeholder="1"
                                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                            />
                            <p className="mt-2 text-xs text-slate-500">
                                Smaller numbers appear first.
                            </p>
                        </div>
                    </div>

                    <div className="flex justify-end">
                        <button
                            type="submit"
                            disabled={submitting}
                            className="inline-flex h-11 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
                        >
                            {submitting ? "Creating..." : "Create"}
                        </button>
                    </div>
                </form>
            </section>

            {/* Category list */}
            <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
                <div className="mb-4 flex items-center justify-between gap-4">
                    <div>
                        <h2 className="text-base font-semibold text-slate-800">
                            Categories
                        </h2>
                        <p className="mt-1 text-sm text-slate-500">
                            Current menu categories for this tenant.
                        </p>
                    </div>
                </div>

                {loading ? (
                    <div className="text-sm text-slate-500">Loading categories...</div>
                ) : categories.length === 0 ? (
                    <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
                        No categories yet. Create your first category to start building the
                        menu.
                    </div>
                ) : (
                    <div className="grid gap-3">
                        {categories.map((category) => (
                            <div
                                key={category.publicId}
                                className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
                            >
                                <div className="min-w-0">
                                    <div className="flex items-center gap-3">
                                        <div className="truncate text-sm font-semibold text-slate-800">
                                            {category.name}
                                        </div>

                                        <span
                                            className={[
                                                "inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium",
                                                category.isActive
                                                    ? "bg-emerald-50 text-emerald-700"
                                                    : "bg-slate-200 text-slate-600",
                                            ].join(" ")}
                                        >
                                            {category.isActive ? "Active" : "Inactive"}
                                        </span>
                                    </div>

                                    <div className="mt-1 text-sm text-slate-500">
                                        Display order: {category.displayOrder}
                                    </div>
                                </div>

                                <div className="flex items-center gap-2">
                                    <button
                                        type="button"
                                        onClick={() => void handleDeleteCategory(category.publicId)}
                                        className="inline-flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm font-medium text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
                                    >
                                        <HiOutlineTrash className="h-4 w-4" />
                                        Delete
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </section>

            {/* Product placeholder */}
            <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
                <div className="mb-4 flex items-center gap-2 text-slate-800">
                    <HiOutlineCube className="h-5 w-5 text-slate-500" />
                    <h2 className="text-base font-semibold">Products</h2>
                </div>

                <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
                    Product management is coming next. You can now create and manage menu
                    categories first.
                </div>
            </section>
        </div>
    )
}