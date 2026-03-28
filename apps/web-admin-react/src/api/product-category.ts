import { tenantHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type ProductCategoryItem = {
  publicId: string
  name: string
  displayOrder: number
  isActive: boolean
}

export type CreateProductCategoryRequest = {
  name: string
  displayOrder: number
}

export async function getProductCategories(): Promise<ProductCategoryItem[]> {
  const response = await tenantHttp.get<ApiResponse<ProductCategoryItem[]>>(
    "/api/product-categories"
  )

  return response.data.data ?? []
}

export async function createProductCategory(
  request: CreateProductCategoryRequest
): Promise<ProductCategoryItem> {
  const response = await tenantHttp.post<ApiResponse<ProductCategoryItem>>(
    "/api/product-categories",
    request
  )

  return response.data.data
}

export async function deleteProductCategory(
  publicId: string
): Promise<boolean> {
  const response = await tenantHttp.delete<ApiResponse<boolean>>(
    `/api/product-categories/${publicId}`
  )

  return response.data.data
}