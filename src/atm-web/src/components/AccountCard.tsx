import type { Account } from '../api/client'
import { formatMoney } from '../format'

interface Props {
  account: Account
  selected: boolean
  onSelect: (accountId: number) => void
}

export function AccountCard({ account, selected, onSelect }: Props) {
  return (
    <button
      type="button"
      className={`account-card${selected ? ' selected' : ''}`}
      aria-pressed={selected}
      onClick={() => onSelect(account.id)}
    >
      <span className="account-name">{account.name}</span>
      <span className="account-balance">{formatMoney(account.balance)}</span>
      <span className="account-hint">{selected ? 'Showing history' : 'View history'}</span>
    </button>
  )
}
