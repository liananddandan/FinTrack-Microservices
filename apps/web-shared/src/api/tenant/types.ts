export type TenantContextDto = {
  tenantPublicId: string
  tenantName: string
  host: string
  isActive: boolean
  logoUrl?: string | null
  themeColor?: string | null
}

export type TenantRegistrationRequest = {
  tenantName: string
  adminName: string
  adminEmail: string
  adminPassword: string
  turnstileToken: string
}

export type TenantRegistrationResult = {
  tenantPublicId: string
  userPublicId: string
  adminEmail: string
}

export type ResolveTenantInvitationResult = {
  invitationPublicId: string
  tenantName: string
  email: string
  role: string
  status: string
  expiredAt: string
}

export type CreateTenantInvitationRequest = {
  email: string
  role: string
}

export type TenantInvitationDto = {
  invitationPublicId: string
  email: string
  role: string
  status: string
  createdAt: string
  acceptedAt?: string | null
  expiredAt: string
  createdByUserEmail: string
}

export type TenantMemberRole = "Admin" | "Member"

export type TenantMemberDto = {
  membershipPublicId: string
  userPublicId: string
  email: string
  userName?: string
  role: TenantMemberRole
  isActive: boolean
  joinedAt: string
}

export type TenantSummaryDto = {
  tenantPublicId: string
  tenantName: string
  isActive: boolean
  createdAt: string
}

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

export type CreateTenantDomainRequest = {
  tenantPublicId: string
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
}

export type UpdateTenantDomainRequest = {
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
}

export type SetTenantDomainActiveRequest = {
  isActive: boolean
}