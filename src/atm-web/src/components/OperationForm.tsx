import { useState, type FormEvent } from 'react'
import { ApiError, deposit, transfer, withdraw, type Account } from '../api/client'
import { formatMoney } from '../format'

type Operation = 'deposit' | 'withdraw' | 'transfer'

interface Props {
  accounts: Account[]
  onCompleted: () => void
}

const labels: Record<Operation, string> = {
  deposit: 'Deposit',
  withdraw: 'Withdraw',
  transfer: 'Transfer',
}

export function OperationForm({ accounts, onCompleted }: Props) {
  const [operation, setOperation] = useState<Operation>('deposit')
  const [accountId, setAccountId] = useState(accounts[0]?.id ?? 0)
  const [toAccountId, setToAccountId] = useState(accounts[1]?.id ?? 0)
  const [amount, setAmount] = useState('')
  const [pending, setPending] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const nameOf = (id: number) => accounts.find((a) => a.id === id)?.name ?? `#${id}`

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (pending) return

    const value = Number(amount)
    setError(null)
    setSuccess(null)
    setPending(true)
    try {
      if (operation === 'deposit') {
        await deposit(accountId, value)
        setSuccess(`Deposited ${formatMoney(value)} to ${nameOf(accountId)}.`)
      } else if (operation === 'withdraw') {
        await withdraw(accountId, value)
        setSuccess(`Withdrew ${formatMoney(value)} from ${nameOf(accountId)}.`)
      } else {
        await transfer(accountId, toAccountId, value)
        setSuccess(`Transferred ${formatMoney(value)} from ${nameOf(accountId)} to ${nameOf(toAccountId)}.`)
      }
      setAmount('')
      onCompleted()
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    } finally {
      setPending(false)
    }
  }

  return (
    <form className="panel operation-form" onSubmit={handleSubmit}>
      <div className="tabs" role="tablist">
        {(Object.keys(labels) as Operation[]).map((op) => (
          <button
            key={op}
            type="button"
            role="tab"
            aria-selected={operation === op}
            className={operation === op ? 'active' : ''}
            onClick={() => {
              setOperation(op)
              setError(null)
              setSuccess(null)
            }}
          >
            {labels[op]}
          </button>
        ))}
      </div>

      <label>
        {operation === 'transfer' ? 'From' : 'Account'}
        <select value={accountId} onChange={(e) => setAccountId(Number(e.target.value))}>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.name}
            </option>
          ))}
        </select>
      </label>

      {operation === 'transfer' && (
        <label>
          To
          <select value={toAccountId} onChange={(e) => setToAccountId(Number(e.target.value))}>
            {accounts.map((a) => (
              <option key={a.id} value={a.id}>
                {a.name}
              </option>
            ))}
          </select>
        </label>
      )}

      <label>
        Amount
        <input
          type="number"
          inputMode="decimal"
          min="0.01"
          step="0.01"
          placeholder="0.00"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          required
        />
      </label>

      <button type="submit" className="primary" disabled={pending}>
        {pending ? 'Processing…' : labels[operation]}
      </button>

      {error && (
        <p className="message error" role="alert">
          {error}
        </p>
      )}
      {success && (
        <p className="message success" role="status">
          {success}
        </p>
      )}
    </form>
  )
}
