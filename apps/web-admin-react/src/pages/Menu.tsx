import { useEffect, useMemo, useState } from "react"
import {
  HiOutlineQueueList,
  HiOutlinePlus,
  HiOutlineTrash,
  HiOutlineCube,
  HiOutlineXMark,
  HiOutlinePencilSquare,
} from "react-icons/hi2"
import { productCategoryApi } from "../lib/productCategoryApi"
import type {ProductCategoryItem, ProductItem} from "@fintrack/web-shared"
import { productApi } from "../lib/productApi"

type CategoryFormState = {
  name: string
  displayOrder: string
  isActive: boolean
}

type ProductFormState = {
  name: string
  description: string
  price: string
  displayOrder: string
  imageUrl: string
  isAvailable: boolean
}

function Modal({
  title,
  open,
  onClose,
  children,
}: {
  title: string
  open: boolean
  onClose: () => void
  children: React.ReactNode
}) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
      <div className="w-full max-w-xl rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-lg font-semibold text-slate-800">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-xl p-2 text-slate-500 transition hover:bg-slate-100 hover:text-slate-700"
          >
            <HiOutlineXMark className="h-5 w-5" />
          </button>
        </div>

        <div className="mt-5">{children}</div>
      </div>
    </div>
  )
}

const emptyCategoryForm: CategoryFormState = {
  name: "",
  displayOrder: "1",
  isActive: true,
}

const emptyProductForm: ProductFormState = {
  name: "",
  description: "",
  price: "",
  displayOrder: "1",
  imageUrl: "",
  isAvailable: true,
}

export default function Menu() {
  const [categoriesLoading, setCategoriesLoading] = useState(false)
  const [productsLoading, setProductsLoading] = useState(false)

  const [categorySubmitting, setCategorySubmitting] = useState(false)
  const [productSubmitting, setProductSubmitting] = useState(false)

  const [categories, setCategories] = useState<ProductCategoryItem[]>([])
  const [products, setProducts] = useState<ProductItem[]>([])

  const [selectedCategoryId, setSelectedCategoryId] = useState("")

  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")

  const [categoryModalOpen, setCategoryModalOpen] = useState(false)
  const [productModalOpen, setProductModalOpen] = useState(false)

  const [editingCategory, setEditingCategory] =
    useState<ProductCategoryItem | null>(null)
  const [editingProduct, setEditingProduct] = useState<ProductItem | null>(null)

  const [categoryForm, setCategoryForm] =
    useState<CategoryFormState>(emptyCategoryForm)

  const [productForm, setProductForm] =
    useState<ProductFormState>(emptyProductForm)

  useEffect(() => {
    void initialize()
  }, [])

  useEffect(() => {
    if (selectedCategoryId) {
      void loadProducts(selectedCategoryId)
    } else {
      setProducts([])
    }
  }, [selectedCategoryId])

  async function initialize() {
    await loadCategories()
  }

  async function loadCategories() {
    setCategoriesLoading(true)
    setErrorMessage("")

    try {
      const result = await productCategoryApi.getProductCategories()
      const sorted = [...result].sort((a, b) => a.displayOrder - b.displayOrder)

      setCategories(sorted)

      if (sorted.length > 0) {
        setSelectedCategoryId((prev) =>
          prev && sorted.some((x) => x.publicId === prev)
            ? prev
            : sorted[0].publicId
        )
      } else {
        setSelectedCategoryId("")
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load categories.")
      } else {
        setErrorMessage("Failed to load categories.")
      }
    } finally {
      setCategoriesLoading(false)
    }
  }

  async function loadProducts(categoryPublicId: string) {
    setProductsLoading(true)
    setErrorMessage("")

    try {
      const result = await productApi.getProductsByCategory(categoryPublicId)
      const sorted = [...result].sort((a, b) => a.displayOrder - b.displayOrder)
      setProducts(sorted)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load products.")
      } else {
        setErrorMessage("Failed to load products.")
      }
    } finally {
      setProductsLoading(false)
    }
  }

  function openCreateCategoryModal() {
    setEditingCategory(null)
    setCategoryForm(emptyCategoryForm)
    setCategoryModalOpen(true)
  }

  function openEditCategoryModal(category: ProductCategoryItem) {
    setEditingCategory(category)
    setCategoryForm({
      name: category.name,
      displayOrder: String(category.displayOrder),
      isActive: category.isActive,
    })
    setCategoryModalOpen(true)
  }

  function openCreateProductModal() {
    setEditingProduct(null)
    setProductForm(emptyProductForm)
    setProductModalOpen(true)
  }

  function openEditProductModal(product: ProductItem) {
    setEditingProduct(product)
    setProductForm({
      name: product.name,
      description: product.description ?? "",
      price: String(product.price),
      displayOrder: String(product.displayOrder),
      imageUrl: product.imageUrl ?? "",
      isAvailable: product.isAvailable,
    })
    setProductModalOpen(true)
  }

  function updateCategoryForm<K extends keyof CategoryFormState>(
    key: K,
    value: CategoryFormState[K]
  ) {
    setCategoryForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function updateProductForm<K extends keyof ProductFormState>(
    key: K,
    value: ProductFormState[K]
  ) {
    setProductForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function handleSaveCategory(e: React.FormEvent) {
    e.preventDefault()

    setCategorySubmitting(true)
    setErrorMessage("")
    setSuccessMessage("")

    try {
      const payload = {
        name: categoryForm.name.trim(),
        displayOrder: Number(categoryForm.displayOrder || 1),
        isActive: categoryForm.isActive,
      }

      if (editingCategory) {
        await productCategoryApi.updateProductCategory(editingCategory.publicId, payload)
        setSuccessMessage("Category updated successfully.")
      } else {
        await productCategoryApi.createProductCategory({
          name: payload.name,
          displayOrder: payload.displayOrder,
        })
        setSuccessMessage("Category created successfully.")
      }

      setCategoryForm(emptyCategoryForm)
      setEditingCategory(null)
      setCategoryModalOpen(false)

      await loadCategories()
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to save category.")
      } else {
        setErrorMessage("Failed to save category.")
      }
    } finally {
      setCategorySubmitting(false)
    }
  }

  async function handleDeleteCategory(publicId: string) {
    const confirmed = window.confirm(
      "Are you sure you want to delete this category?"
    )

    if (!confirmed) return

    setErrorMessage("")
    setSuccessMessage("")

    try {
      await productCategoryApi.deleteProductCategory(publicId)
      setSuccessMessage("Category deleted successfully.")

      if (selectedCategoryId === publicId) {
        setSelectedCategoryId("")
      }

      await loadCategories()
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to delete category.")
      } else {
        setErrorMessage("Failed to delete category.")
      }
    }
  }

  async function handleSaveProduct(e: React.FormEvent) {
    e.preventDefault()

    if (!selectedCategoryId) {
      setErrorMessage("Please select a category first.")
      return
    }

    setProductSubmitting(true)
    setErrorMessage("")
    setSuccessMessage("")

    try {
      const payload = {
        categoryPublicId: selectedCategoryId,
        name: productForm.name.trim(),
        description: productForm.description.trim() || null,
        price: Number(productForm.price || 0),
        imageUrl: productForm.imageUrl.trim() || null,
        displayOrder: productForm.displayOrder
          ? Number(productForm.displayOrder)
          : null,
        isAvailable: productForm.isAvailable,
      }

      if (editingProduct) {
        await productApi.updateProduct(editingProduct.publicId, payload)
        setSuccessMessage("Product updated successfully.")
      } else {
        await productApi.createProduct({
          categoryPublicId: payload.categoryPublicId,
          name: payload.name,
          description: payload.description,
          price: payload.price,
          imageUrl: payload.imageUrl,
          displayOrder: payload.displayOrder,
        })
        setSuccessMessage("Product created successfully.")
      }

      setProductForm(emptyProductForm)
      setEditingProduct(null)
      setProductModalOpen(false)

      await loadProducts(selectedCategoryId)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to save product.")
      } else {
        setErrorMessage("Failed to save product.")
      }
    } finally {
      setProductSubmitting(false)
    }
  }

  async function handleDeleteProduct(publicId: string) {
    const confirmed = window.confirm(
      "Are you sure you want to delete this product?"
    )

    if (!confirmed) return

    setErrorMessage("")
    setSuccessMessage("")

    try {
      await productApi.deleteProduct(publicId)
      setSuccessMessage("Product deleted successfully.")

      if (selectedCategoryId) {
        await loadProducts(selectedCategoryId)
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to delete product.")
      } else {
        setErrorMessage("Failed to delete product.")
      }
    }
  }

  const selectedCategory = useMemo(
    () => categories.find((x) => x.publicId === selectedCategoryId) ?? null,
    [categories, selectedCategoryId]
  )

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-6">
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
              Manage categories and products for the current tenant menu.
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

      <section className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
        <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
          <div className="mb-4 flex items-center justify-between gap-4">
            <div>
              <h2 className="text-base font-semibold text-slate-800">
                Categories
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                Select a category to manage products.
              </p>
            </div>

            <button
              type="button"
              onClick={openCreateCategoryModal}
              className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
            >
              <HiOutlinePlus className="h-4 w-4" />
              Add
            </button>
          </div>

          {categoriesLoading ? (
            <div className="text-sm text-slate-500">Loading categories...</div>
          ) : categories.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
              No categories yet.
            </div>
          ) : (
            <div className="space-y-3">
              {categories.map((category) => {
                const isSelected = category.publicId === selectedCategoryId

                return (
                  <div
                    key={category.publicId}
                    className={[
                      "rounded-2xl border px-4 py-4 transition",
                      isSelected
                        ? "border-indigo-300 bg-indigo-50"
                        : "border-slate-200 bg-slate-50 hover:border-slate-300",
                    ].join(" ")}
                  >
                    <button
                      type="button"
                      onClick={() => setSelectedCategoryId(category.publicId)}
                      className="w-full text-left"
                    >
                      <div className="flex items-center justify-between gap-3">
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
                        Order: {category.displayOrder}
                      </div>
                    </button>

                    <div className="mt-4 flex items-center justify-end gap-2">
                      <button
                        type="button"
                        onClick={() => openEditCategoryModal(category)}
                        className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 transition hover:border-indigo-300 hover:text-indigo-600"
                      >
                        <HiOutlinePencilSquare className="h-4 w-4" />
                        Edit
                      </button>

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
                )
              })}
            </div>
          )}
        </section>

        <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
          <div className="mb-4 flex items-center justify-between gap-4">
            <div className="flex items-center gap-2 text-slate-800">
              <HiOutlineCube className="h-5 w-5 text-slate-500" />
              <div>
                <h2 className="text-base font-semibold">
                  {selectedCategory
                    ? `Products · ${selectedCategory.name}`
                    : "Products"}
                </h2>
                <p className="mt-1 text-sm text-slate-500">
                  {selectedCategory
                    ? "Manage products under the selected category."
                    : "Select a category first."}
                </p>
              </div>
            </div>

            <button
              type="button"
              onClick={openCreateProductModal}
              disabled={!selectedCategory}
              className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <HiOutlinePlus className="h-4 w-4" />
              Add product
            </button>
          </div>

          {!selectedCategory ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
              Select a category first to manage products.
            </div>
          ) : productsLoading ? (
            <div className="text-sm text-slate-500">Loading products...</div>
          ) : products.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
              No products in this category yet.
            </div>
          ) : (
            <div className="overflow-x-auto rounded-2xl border border-slate-200">
              <table className="min-w-full border-collapse text-left">
                <thead className="bg-slate-50">
                  <tr className="text-sm text-slate-600">
                    <th className="px-4 py-3 font-medium">Product</th>
                    <th className="px-4 py-3 font-medium">Price</th>
                    <th className="px-4 py-3 font-medium">Order</th>
                    <th className="px-4 py-3 font-medium">Status</th>
                    <th className="px-4 py-3 font-medium">Actions</th>
                  </tr>
                </thead>

                <tbody>
                  {products.map((product) => (
                    <tr
                      key={product.publicId}
                      className="border-t border-slate-200 hover:bg-slate-50"
                    >
                      <td className="px-4 py-4">
                        <div className="text-sm font-semibold text-slate-800">
                          {product.name}
                        </div>
                        {product.description ? (
                          <div className="mt-1 text-sm text-slate-500">
                            {product.description}
                          </div>
                        ) : null}
                      </td>

                      <td className="px-4 py-4 text-sm text-slate-700">
                        ${product.price.toFixed(2)}
                      </td>

                      <td className="px-4 py-4 text-sm text-slate-700">
                        {product.displayOrder}
                      </td>

                      <td className="px-4 py-4">
                        <span
                          className={[
                            "inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium",
                            product.isAvailable
                              ? "bg-emerald-50 text-emerald-700"
                              : "bg-slate-200 text-slate-600",
                          ].join(" ")}
                        >
                          {product.isAvailable ? "Available" : "Unavailable"}
                        </span>
                      </td>

                      <td className="px-4 py-4">
                        <div className="flex items-center gap-2">
                          <button
                            type="button"
                            onClick={() => openEditProductModal(product)}
                            className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 transition hover:border-indigo-300 hover:text-indigo-600"
                          >
                            <HiOutlinePencilSquare className="h-4 w-4" />
                            Edit
                          </button>

                          <button
                            type="button"
                            onClick={() => void handleDeleteProduct(product.publicId)}
                            className="inline-flex items-center gap-2 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm font-medium text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
                          >
                            <HiOutlineTrash className="h-4 w-4" />
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </section>

      <Modal
        title={editingCategory ? "Edit category" : "Create category"}
        open={categoryModalOpen}
        onClose={() => {
          setCategoryModalOpen(false)
          setEditingCategory(null)
          setCategoryForm(emptyCategoryForm)
        }}
      >
        <form onSubmit={(e) => void handleSaveCategory(e)} className="space-y-4">
          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Category name
            </label>
            <input
              type="text"
              value={categoryForm.name}
              onChange={(e) => updateCategoryForm("name", e.target.value)}
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
              value={categoryForm.displayOrder}
              onChange={(e) => updateCategoryForm("displayOrder", e.target.value)}
              placeholder="1"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>

          {editingCategory ? (
            <label className="flex items-center gap-3 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={categoryForm.isActive}
                onChange={(e) => updateCategoryForm("isActive", e.target.checked)}
                className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              />
              Active
            </label>
          ) : null}

          <div className="flex justify-end gap-3">
            <button
              type="button"
              onClick={() => {
                setCategoryModalOpen(false)
                setEditingCategory(null)
                setCategoryForm(emptyCategoryForm)
              }}
              className="inline-flex h-11 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={categorySubmitting}
              className="inline-flex h-11 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {categorySubmitting
                ? "Saving..."
                : editingCategory
                ? "Save changes"
                : "Create"}
            </button>
          </div>
        </form>
      </Modal>

      <Modal
        title={
          editingProduct
            ? `Edit product${selectedCategory ? ` · ${selectedCategory.name}` : ""}`
            : `Add product${selectedCategory ? ` · ${selectedCategory.name}` : ""}`
        }
        open={productModalOpen}
        onClose={() => {
          setProductModalOpen(false)
          setEditingProduct(null)
          setProductForm(emptyProductForm)
        }}
      >
        <form onSubmit={(e) => void handleSaveProduct(e)} className="space-y-4">
          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Product name
            </label>
            <input
              type="text"
              value={productForm.name}
              onChange={(e) => updateProductForm("name", e.target.value)}
              placeholder="e.g. Flat White"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Price
              </label>
              <input
                type="number"
                min={0}
                step="0.01"
                value={productForm.price}
                onChange={(e) => updateProductForm("price", e.target.value)}
                placeholder="0.00"
                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Order in category
              </label>
              <input
                type="number"
                min={1}
                value={productForm.displayOrder}
                onChange={(e) => updateProductForm("displayOrder", e.target.value)}
                placeholder="1"
                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Description
            </label>
            <textarea
              value={productForm.description}
              onChange={(e) => updateProductForm("description", e.target.value)}
              placeholder="Optional description"
              rows={3}
              className="w-full rounded-xl border border-slate-300 bg-white px-3 py-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Image URL
            </label>
            <input
              type="text"
              value={productForm.imageUrl}
              onChange={(e) => updateProductForm("imageUrl", e.target.value)}
              placeholder="Optional image URL"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>

          {editingProduct ? (
            <label className="flex items-center gap-3 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={productForm.isAvailable}
                onChange={(e) => updateProductForm("isAvailable", e.target.checked)}
                className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
              />
              Available
            </label>
          ) : null}

          <div className="flex justify-end gap-3">
            <button
              type="button"
              onClick={() => {
                setProductModalOpen(false)
                setEditingProduct(null)
                setProductForm(emptyProductForm)
              }}
              className="inline-flex h-11 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={productSubmitting || !selectedCategory}
              className="inline-flex h-11 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {productSubmitting
                ? "Saving..."
                : editingProduct
                ? "Save changes"
                : "Create product"}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  )
}