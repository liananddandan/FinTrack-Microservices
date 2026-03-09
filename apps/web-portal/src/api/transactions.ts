import { tenantHttp } from "./http";
import type { ApiResponse } from "./types";

export interface CreateDonationRequest {
  title: string;
  description?: string;
  amount: number;
  currency: string;
}

export interface CreateDonationResult {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  paymentReference?: string;
  failureReason?: string;
}

export interface TransactionListItem {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  title: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  riskStatus: string;
  createdAtUtc: string;
}

export interface TransactionDetail {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  title: string;
  description?: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  riskStatus: string;
  createdByUserPublicId: string;
  createdAtUtc: string;
  approvedByUserPublicId?: string;
  approvedAtUtc?: string;
  paidByUserPublicId?: string;
  paidAtUtc?: string;
  paymentReference?: string;
  failureReason?: string;
  refundedByUserPublicId?: string;
  refundedAtUtc?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export async function createDonation(
  payload: CreateDonationRequest
): Promise<CreateDonationResult> {
  const response = await tenantHttp.post<ApiResponse<CreateDonationResult>>(
    "/api/transactions/donations",
    payload
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Donation failed");
  }

  return result.data;
}

export async function getMyTransactions(
  pageNumber = 1,
  pageSize = 10
): Promise<PagedResult<TransactionListItem>> {
  const response = await tenantHttp.get<ApiResponse<PagedResult<TransactionListItem>>>(
    "/api/transactions/my",
    {
      params: {
        pageNumber,
        pageSize,
      },
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch transactions");
  }

  return result.data;
}

export async function getTransactionDetail(
  transactionPublicId: string
): Promise<TransactionDetail> {
  const response = await tenantHttp.get<ApiResponse<TransactionDetail>>(
    `/api/transactions/${transactionPublicId}`
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch transaction detail");
  }

  return result.data;
}