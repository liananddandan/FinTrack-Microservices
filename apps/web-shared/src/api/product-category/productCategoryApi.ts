import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  ProductCategoryItem,
  CreateProductCategoryRequest,
  UpdateProductCategoryRequest,
} from "./types"

export function createProductCategoryApi(tenantHttp: AxiosInstance) {
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
    async getProductCategories(): Promise<ProductCategoryItem[]> {
      const response = await tenantHttp.get<ApiResponse<ProductCategoryItem[]>>(
        "/api/product-categories"
      )

      return response.data.data ?? []
    },

    async createProductCategory(
      request: CreateProductCategoryRequest
    ): Promise<ProductCategoryItem> {
      const response = await tenantHttp.post<ApiResponse<ProductCategoryItem>>(
        "/api/product-categories",
        request
      )

      return unwrapApiResponse(response, "Failed to create product category")
    },

    async updateProductCategory(
      publicId: string,
      request: UpdateProductCategoryRequest
    ): Promise<ProductCategoryItem> {
      const response = await tenantHttp.put<ApiResponse<ProductCategoryItem>>(
        `/api/product-categories/${publicId}`,
        request
      )

      return unwrapApiResponse(response, "Failed to update product category")
    },

    async deleteProductCategory(publicId: string): Promise<boolean> {
      const response = await tenantHttp.delete<ApiResponse<boolean>>(
        `/api/product-categories/${publicId}`
      )

      return unwrapApiResponse(response, "Failed to delete product category")
    },
  }
}