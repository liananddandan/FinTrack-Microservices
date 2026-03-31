import type { ProductCategoryItem } from "../../api/product-category"
import { HiOutlineQueueList } from "react-icons/hi2"

type Props = {
  categories: ProductCategoryItem[]
  selectedCategoryId: string
  loading: boolean
  onSelect: (categoryId: string) => void
}

export default function CategorySidebar({
  categories,
  selectedCategoryId,
  loading,
  onSelect,
}: Props) {
  return (
    <div className="rounded-3xl border border-slate-200 bg-white p-5">
      <div className="mb-4 flex items-center gap-2 text-slate-800">
        <HiOutlineQueueList className="h-5 w-5 text-slate-500" />
        <h2 className="text-base font-semibold">Categories</h2>
      </div>

      {loading ? (
        <div className="text-sm text-slate-500">Loading categories...</div>
      ) : categories.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
          No categories available yet.
        </div>
      ) : (
        <div className="space-y-2">
          {categories.map((category) => {
            const isActive = selectedCategoryId === category.publicId

            return (
              <button
                key={category.publicId}
                type="button"
                onClick={() => onSelect(category.publicId)}
                className={[
                  "w-full rounded-2xl border px-4 py-3 text-left transition",
                  isActive
                    ? "border-indigo-200 bg-indigo-50 text-indigo-700"
                    : "border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50",
                ].join(" ")}
              >
                <div className="flex items-center justify-between gap-3">
                  <span className="truncate text-sm font-medium">
                    {category.name}
                  </span>
                  <span className="text-xs text-slate-500">
                    #{category.displayOrder}
                  </span>
                </div>
              </button>
            )
          })}
        </div>
      )}
    </div>
  )
}