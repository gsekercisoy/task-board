import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthContext.jsx';

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: '', password: '' });
  const [err, setErr] = useState('');
  const submit = async (e) => {
    e.preventDefault();
    setErr('');
    try { await login(form.username, form.password); navigate('/'); }
    catch { setErr('Login failed. Check credentials.'); }
  };
  return (
    <form className="auth" onSubmit={submit}>
      <h2>Login</h2>
      {err && <p style={{ color: 'red' }}>{err}</p>}
      <input placeholder="Username" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
      <input type="password" placeholder="Password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
      <button type="submit">Login</button>
      <p><Link to="/register">No account? Register</Link></p>
    </form>
  );
}
