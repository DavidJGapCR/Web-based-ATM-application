const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

export const formatMoney = (value: number) => currency.format(value)

export const formatDateTime = (iso: string) => new Date(iso).toLocaleString()
