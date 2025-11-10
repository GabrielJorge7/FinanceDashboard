import { BrowserRouter, Routes, Route, Navigate, Link } from 'react-router-dom';
import React from 'react';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Categories from './pages/Categories';
import Transactions from './pages/Transactions';
import Register from './pages/Register';

function PrivateRoute({ children }: { children: React.ReactElement }) {
  const token = localStorage.getItem('token');
  if (!token) return <Navigate to="/login" replace />;
  return children;
}

function Layout({ children }: { children: React.ReactNode }) {
  const logout = () => {
    localStorage.removeItem('token');
    window.location.href = '/login';
  };
  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <aside style={{ width: 220, background: '#111827', color: '#fff', padding: 16 }}>
        <h2 style={{ fontSize: 18, marginBottom: 24 }}>Finance</h2>
        <nav style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          <Link style={linkStyle} to="/">Dashboard</Link>
          <Link style={linkStyle} to="/categories">Categorias</Link>
          <Link style={linkStyle} to="/transactions">Lançamentos</Link>
          <Link style={linkStyle} to="/register">Cadastrar</Link>
          <button onClick={logout} style={{ marginTop: 16, background: '#dc2626', color: '#fff', padding: '8px 12px', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Sair</button>
        </nav>
      </aside>
      <main style={{ flex: 1, background: '#f3f4f6' }}>{children}</main>
    </div>
  );
}

const linkStyle: React.CSSProperties = {
  color: '#e5e7eb', textDecoration: 'none', padding: '8px 12px', borderRadius: 4, background: '#1f2937'
};

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/" element={<PrivateRoute><Layout><Dashboard /></Layout></PrivateRoute>} />
        <Route path="/categories" element={<PrivateRoute><Layout><Categories /></Layout></PrivateRoute>} />
        <Route path="/transactions" element={<PrivateRoute><Layout><Transactions /></Layout></PrivateRoute>} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
