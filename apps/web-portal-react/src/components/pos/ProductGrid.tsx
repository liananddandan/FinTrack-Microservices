import type { ProductItem } from "../../api/product"

type Props = {
  categoryName: string
  loading: boolean
  products: ProductItem[]
  onAdd: (product: ProductItem) => void
}

export default function ProductGrid({
  categoryName,
  loading,
  products,
  onAdd,
}: Props) {
  return (
    <div className="rounded-3xl border border-slate-200 bg-white p-5">
      <div className="mb-4 flex items-center justify-between gap-4">
        <div>
          <h2 className="text-base font-semibold text-slate-800">
            {categoryName || "Menu"}
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            Select products and add them to the current order.
          </p>
        </div>
      </div>

      {!categoryName ? (
        <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
          Choose a category to view products.
        </div>
      ) : loading ? (
        <div className="text-sm text-slate-500">Loading products...</div>
      ) : products.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
          No products available in this category yet.
        </div>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
          {products.map((product) => (
            <div
              key={product.publicId}
              className="rounded-xl border border-slate-200 bg-white p-3"
            >
              <div className="truncate text-sm font-medium text-slate-800">
                {product.name}
              </div>
              <div className="mt-1 text-sm text-slate-500">
                ${product.price.toFixed(2)}
              </div>
              <button
                type="button"
                onClick={() => onAdd(product)}
                className="mt-3 w-full rounded-lg bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
              >
                Add
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}