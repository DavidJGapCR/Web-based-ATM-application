import type { Transaction, TransactionType } from '../api/client'
import { formatDateTime, formatMoney } from '../format'

interface Props {
  accountName: string
  transactions: Transaction[]
}

const typeLabels: Record<TransactionType, string> = {
  Deposit: 'Deposit',
  Withdrawal: 'Withdrawal',
  TransferIn: 'Transfer in',
  TransferOut: 'Transfer out',
}

const isDebit = (type: TransactionType) => type === 'Withdrawal' || type === 'TransferOut'

export function TransactionHistory({ accountName, transactions }: Props) {
  return (
    <section className="panel history">
      <h2>{accountName} history</h2>
      {transactions.length === 0 ? (
        <p className="empty">No transactions yet.</p>
      ) : (
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Type</th>
                <th className="num">Amount</th>
                <th className="num">Balance</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((t) => (
                <tr key={t.id}>
                  <td>{formatDateTime(t.createdAt)}</td>
                  <td>{typeLabels[t.type]}</td>
                  <td className={`num ${isDebit(t.type) ? 'debit' : 'credit'}`}>
                    {isDebit(t.type) ? '−' : '+'}
                    {formatMoney(t.amount)}
                  </td>
                  <td className="num">{formatMoney(t.balanceAfter)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
