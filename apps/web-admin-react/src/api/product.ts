import { tenantHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type ProductItem = {
  publicId: string
  categoryPublicId: string
  categoryName: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number
  isAvailable: boolean
}

export type CreateProductRequest = {
  categoryPublicId: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number | null
}

export type UpdateProductRequest = {
  categoryPublicId: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number | null
  isAvailable: boolean
}

export async function getProductsByCategory(
  categoryPublicId: string
): Promise<ProductItem[]> {
  const response = await tenantHttp.get<ApiResponse<ProductItem[]>>(
    `/api/product-categories/${categoryPublicId}/products`
  )

  return response.data.data ?? []
}

export async function createProduct(
  request: CreateProductRequest
): Promise<ProductItem> {
  const response = await tenantHttp.post<ApiResponse<ProductItem>>(
    "/api/products",
    request
  )

  return response.data.data
}

export async function updateProduct(
  publicId: string,
  request: UpdateProductRequest
): Promise<ProductItem> {
  const response = await tenantHttp.put<ApiResponse<ProductItem>>(
    `/api/products/${publicId}`,
    request
  )

  return response.data.data
}

export async function deleteProduct(publicId: string): Promise<boolean> {
  const response = await tenantHttp.delete<ApiResponse<boolean>>(
    `/api/products/${publicId}`
  )

  return response.data.data
}