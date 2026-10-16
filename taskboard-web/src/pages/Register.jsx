import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthContext.jsx';

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: '', password: '', role: 'Member' });
  const [err, setErr] = useState('');
  const submit = async (e) => {
    e.preventDefault();
    setErr('');
    try { await register(form.username, form.password, form.role); navigate('/'); }
    catch { setErr('Registration failed. Username may be taken.'); }
  };
  return (
    <form className="auth" onSubmit={submit}>
      <h2>Register</h2>
      {err && <p style={{ color: 'red' }}>{err}</p>}
      <input placeholder="Username" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
      <input type="password" placeholder="Password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
      <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
        <option value="Member">Member</option>
        <option value="Manager">Manager</option>
      </select>
      <button type="submit">Register</button>
    </form>
  );
}
