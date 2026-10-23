// Typed client for the ATM API. Components never call fetch directly.

export interface Account {
  id: number
  name: string
  balance: number
}

export type TransactionType = 'Deposit' | 'Withdrawal' | 'TransferIn' | 'TransferOut'

export interface Transaction {
  id: string
  accountId: number
  type: TransactionType
  amount: number
  balanceAfter: number
  transferId: string | null
  createdAt: string
}

export interface AccountOperationResult {
  account: Account
  transaction: Transaction
}

export interface TransferResult {
  transferId: string
  from: Account
  to: Account
}

/** An error response from the API, built from its RFC 7807 ProblemDetails body. */
export class ApiError extends Error {
  readonly status: number
  readonly code?: string

  constructor(status: number, message: string, code?: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(path, {
      ...init,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    })
  } catch {
    throw new ApiError(0, 'Cannot reach the server. Is the API running?')
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new ApiError(
      response.status,
      problem?.title ?? `Request failed (${response.status}).`,
      problem?.code,
    )
  }

  return (await response.json()) as T
}

const post = <T>(path: string, body: unknown) =>
  request<T>(path, { method: 'POST', body: JSON.stringify(body) })

export const getAccounts = () => request<Account[]>('/api/accounts')

export const getTransactions = (accountId: number) =>
  request<Transaction[]>(`/api/accounts/${accountId}/transactions`)

export const deposit = (accountId: number, amount: number) =>
  post<AccountOperationResult>(`/api/accounts/${accountId}/deposits`, { amount })

export const withdraw = (accountId: number, amount: number) =>
  post<AccountOperationResult>(`/api/accounts/${accountId}/withdrawals`, { amount })

export const transfer = (fromAccountId: number, toAccountId: number, amount: number) =>
  post<TransferResult>('/api/transfers', { fromAccountId, toAccountId, amount })
