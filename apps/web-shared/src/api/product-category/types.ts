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

export type UpdateProductCategoryRequest = {
  name: string
  displayOrder: number
  isActive: boolean
}