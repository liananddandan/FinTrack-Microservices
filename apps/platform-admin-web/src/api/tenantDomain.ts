import { platformHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type TenantDomainMappingDto = {
  domainPublicId: string
  tenantPublicId: string
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string | null
}

export type CreateTenantDomainMappingRequest = {
  tenantPublicId: string
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
}

export type UpdateTenantDomainMappingRequest = {
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
}

export async function getTenantDomains(
  tenantPublicId: string
): Promise<TenantDomainMappingDto[]> {
  const response = await platformHttp.get<ApiResponse<TenantDomainMappingDto[]>>(
    `/api/platform/tenant-domains/by-tenant/${tenantPublicId}`
  )

  return response.data.data ?? []
}

export async function createTenantDomain(
  request: CreateTenantDomainMappingRequest
): Promise<TenantDomainMappingDto> {
  const response = await platformHttp.post<ApiResponse<TenantDomainMappingDto>>(
    "/api/platform/tenant-domains",
    request
  )

  return response.data.data
}

export async function updateTenantDomain(
  domainPublicId: string,
  request: UpdateTenantDomainMappingRequest
): Promise<TenantDomainMappingDto> {
  const response = await platformHttp.put<ApiResponse<TenantDomainMappingDto>>(
    `/api/platform/tenant-domains/${domainPublicId}`,
    request
  )

  return response.data.data
}

export async function setTenantDomainActive(
  domainPublicId: string,
  isActive: boolean
): Promise<TenantDomainMappingDto> {
  const response = await platformHttp.patch<ApiResponse<TenantDomainMappingDto>>(
    `/api/platform/tenant-domains/${domainPublicId}/active`,
    { isActive }
  )

  return response.data.data
}

export async function deleteTenantDomain(
  domainPublicId: string
): Promise<boolean> {
  const response = await platformHttp.delete<ApiResponse<boolean>>(
    `/api/platform/tenant-domains/${domainPublicId}`
  )

  return response.data.data
}