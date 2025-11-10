import { useEffect, useState } from 'react';
import api from '../lib/api';

interface CategoryOption {
  id: number;
  name: string;
}

interface Transaction {
  id: number;
  description: string;
  amount: number;
  type: number;
  typeName: string;
  date: string;
  categoryId: number;
  category: { id: number; name: string; color: string };
}

export default function Transactions() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [categories, setCategories] = useState<CategoryOption[]>([]);
  const [description, setDescription] = useState('');
  const [amount, setAmount] = useState('');
  const [type, setType] = useState('Income');
  const [date, setDate] = useState<string>(() => new Date().toISOString().substring(0, 10));
  const [categoryId, setCategoryId] = useState('');
  const [error, setError] = useState('');

  const load = async () => {
    try {
      const [tRes, cRes] = await Promise.all([
        api.get<Transaction[]>('/api/Transactions'),
        api.get('/api/Categories'),
      ]);
      setTransactions(tRes.data);
      setCategories(cRes.data.map((c: any) => ({ id: c.id, name: c.name })));
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar');
    }
  };

  useEffect(() => {
    load();
  }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await api.post('/api/Transactions', {
        description,
        amount: parseFloat(amount),
        type: type === 'Income' ? 1 : 2,
        date,
        categoryId: parseInt(categoryId, 10),
      });
      setDescription('');
      setAmount('');
      setType('Income');
      setDate(new Date().toISOString().substring(0, 10));
      setCategoryId('');
      load();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao criar lançamento');
    }
  };

  return (
    <div style={{ padding: 24, fontFamily: 'sans-serif' }}>
      <h2>Lançamentos</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <form onSubmit={create} style={{ display: 'grid', gridTemplateColumns: 'repeat(6, 1fr)', gap: 8, marginBottom: 24 }}>
        <input style={{ gridColumn: 'span 2' }} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Descrição" required />
        <input value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="Valor" required />
        <select value={type} onChange={(e) => setType(e.target.value)}>
          <option value="Income">Receita</option>
          <option value="Expense">Despesa</option>
        </select>
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
          <option value="">Categoria</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
        <button style={{ gridColumn: 'span 6' }}>Adicionar</button>
      </form>

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>
            <th>Data</th>
            <th>Descrição</th>
            <th>Categoria</th>
            <th>Tipo</th>
            <th>Valor</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((t) => (
            <tr key={t.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
              <td>{new Date(t.date).toLocaleDateString()}</td>
              <td>{t.description}</td>
              <td>{t.category.name}</td>
              <td>{t.typeName === 'Income' ? 'Receita' : 'Despesa'}</td>
              <td style={{ color: t.typeName === 'Income' ? '#16a34a' : '#dc2626' }}>R$ {t.amount.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
