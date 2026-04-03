import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  ProductItem,
  CreateProductRequest,
  UpdateProductRequest,
} from "./types"

export function createProductApi(tenantHttp: AxiosInstance) {
  function unwrapApiResponse<T>(
    response: { data: ApiResponse<T> },
    defaultMessage: string
  ): T {
    const result = response.data

    if (result.data == null) {
      throw new Error(result.message || defaultMessage)
    }

    return result.data
  }

  return {
    async getProductsByCategory(
      categoryPublicId: string
    ): Promise<ProductItem[]> {
      const response = await tenantHttp.get<ApiResponse<ProductItem[]>>(
        `/api/product-categories/${categoryPublicId}/products`
      )

      return response.data.data ?? []
    },

    async createProduct(request: CreateProductRequest): Promise<ProductItem> {
      const response = await tenantHttp.post<ApiResponse<ProductItem>>(
        "/api/products",
        request
      )

      return unwrapApiResponse(response, "Failed to create product")
    },

    async updateProduct(
      publicId: string,
      request: UpdateProductRequest
    ): Promise<ProductItem> {
      const response = await tenantHttp.put<ApiResponse<ProductItem>>(
        `/api/products/${publicId}`,
        request
      )

      return unwrapApiResponse(response, "Failed to update product")
    },

    async deleteProduct(publicId: string): Promise<boolean> {
      const response = await tenantHttp.delete<ApiResponse<boolean>>(
        `/api/products/${publicId}`
      )

      return unwrapApiResponse(response, "Failed to delete product")
    },
  }
}