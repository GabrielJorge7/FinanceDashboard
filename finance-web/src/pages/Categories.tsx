import { useEffect, useState } from 'react';
import api from '../lib/api';

interface Category {
  id: number;
  name: string;
  description?: string;
  color: string;
  transactionCount: number;
  totalAmount: number;
}

export default function Categories() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [color, setColor] = useState('#6366f1');
  const [error, setError] = useState('');

  const load = async () => {
    try {
      const { data } = await api.get<Category[]>('/api/Categories');
      setCategories(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar categorias');
    }
  };

  useEffect(() => {
    load();
  }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/api/Categories', { name, description, color });
      setName('');
      setDescription('');
      setColor('#6366f1');
      load();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao criar categoria');
    }
  };

  return (
    <div style={{ padding: 24, fontFamily: 'sans-serif' }}>
      <h2>Categorias</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <form onSubmit={create} style={{ marginBottom: 24, display: 'flex', gap: 8 }}>
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Nome" required />
        <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Descrição" />
        <input type="color" value={color} onChange={(e) => setColor(e.target.value)} />
        <button>Criar</button>
      </form>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ textAlign: 'left', borderBottom: '1px solid #e5e7eb' }}>
            <th>Nome</th>
            <th>Transações</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          {categories.map((c) => (
            <tr key={c.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
              <td>
                <span style={{ display: 'inline-block', width: 12, height: 12, background: c.color, borderRadius: '50%', marginRight: 8 }} />
                {c.name}
              </td>
              <td>{c.transactionCount}</td>
              <td style={{ color: c.totalAmount >= 0 ? '#16a34a' : '#dc2626' }}>R$ {c.totalAmount.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
