import { useEffect, useState } from 'react';
import api from '../lib/api';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, Bar, BarChart } from 'recharts';

interface Summary {
  totalIncome: number;
  totalExpense: number;
  balance: number;
  transactionCount: number;
}

interface MonthlyFlow {
  month: string;
  year: number;
  income: number;
  expense: number;
  balance: number;
}

export default function Dashboard() {
  const [summary, setSummary] = useState<Summary | null>(null);
  const [monthly, setMonthly] = useState<MonthlyFlow[]>([]);
  const [error, setError] = useState('');

  const load = async () => {
    try {
      const [s, m] = await Promise.all([
        api.get<Summary>('/api/Transactions/summary'),
        api.get<MonthlyFlow[]>('/api/Transactions/monthly-flow')
      ]);
      setSummary(s.data);
      setMonthly(m.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar dados');
    }
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <div style={{ padding: 24, fontFamily: 'sans-serif' }}>
      <h2>Dashboard</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      {summary && (
        <div style={{ display: 'flex', gap: 16, marginBottom: 24 }}>
          <Card title="Receitas">R$ {summary.totalIncome.toFixed(2)}</Card>
          <Card title="Despesas">R$ {summary.totalExpense.toFixed(2)}</Card>
          <Card title="Saldo">R$ {summary.balance.toFixed(2)}</Card>
          <Card title="Lançamentos">{summary.transactionCount}</Card>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
        <div>
          <h3>Fluxo Mensal</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={monthly}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="income" fill="#22c55e" name="Receitas" />
              <Bar dataKey="expense" fill="#ef4444" name="Despesas" />
            </BarChart>
          </ResponsiveContainer>
        </div>
        <div>
          <h3>Saldo Acumulado</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={accumulate(monthly)}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line dataKey="balance" stroke="#6366f1" name="Saldo" />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}

function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div style={{ border: '1px solid #e5e7eb', padding: 16, borderRadius: 8, background: '#fff' }}>
      <div style={{ color: '#6b7280', fontSize: 12 }}>{title}</div>
      <div style={{ fontWeight: 600, fontSize: 20 }}>{children}</div>
    </div>
  );
}

function accumulate(months: MonthlyFlow[]) {
  let running = 0;
  return months.map((m) => ({
    ...m,
    balance: (running += m.income - m.expense),
  }));
}
