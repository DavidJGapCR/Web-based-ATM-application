import { useCallback, useEffect, useState } from 'react'
import { ApiError, getAccounts, getTransactions, type Account, type Transaction } from './api/client'
import { AccountCard } from './components/AccountCard'
import { OperationForm } from './components/OperationForm'
import { TransactionHistory } from './components/TransactionHistory'
import './App.css'

export default function App() {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  const refresh = useCallback(async (accountId: number | null) => {
    try {
      const all = await getAccounts()
      setAccounts(all)
      const id = accountId ?? all[0]?.id ?? null
      setSelectedId(id)
      setTransactions(id === null ? [] : await getTransactions(id))
      setLoadError(null)
    } catch (e) {
      setLoadError(e instanceof ApiError ? e.message : 'Failed to load accounts.')
    }
  }, [])

  useEffect(() => {
    // Initial load. refresh() only sets state after its requests resolve, so this is not a synchronous update.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void refresh(null)
  }, [refresh])

  const selected = accounts.find((a) => a.id === selectedId)

  return (
    <main className="app">
      <header>
        <h1>ATM</h1>
      </header>

      {loadError && (
        <p className="message error" role="alert">
          {loadError}
        </p>
      )}

      <section className="accounts">
        {accounts.map((account) => (
          <AccountCard
            key={account.id}
            account={account}
            selected={account.id === selectedId}
            onSelect={(id) => void refresh(id)}
          />
        ))}
      </section>

      {accounts.length > 0 && (
        <div className="workspace">
          <OperationForm accounts={accounts} onCompleted={() => void refresh(selectedId)} />
          {selected && <TransactionHistory accountName={selected.name} transactions={transactions} />}
        </div>
      )}
    </main>
  )
}
