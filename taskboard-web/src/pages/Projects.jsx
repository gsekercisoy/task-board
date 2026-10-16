import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api.js';

export default function Projects() {
  const [projects, setProjects] = useState([]);
  const [form, setForm] = useState({ name: '', description: '' });

  const load = async () => {
    const { data } = await api.get('/projects');
    setProjects(data);
  };
  useEffect(() => { load(); }, []);

  const create = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) return;
    await api.post('/projects', form);
    setForm({ name: '', description: '' });
    load();
  };

  return (
    <div>
      <h2>Projects</h2>
      <form onSubmit={create} className="card">
        <input placeholder="Project name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        <input placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        <button type="submit">Create project</button>
      </form>
      {projects.map((p) => (
        <div key={p.id} className="card">
          <Link to={`/projects/${p.id}`}><strong>{p.name}</strong></Link>
          <p className="muted">{p.description}</p>
        </div>
      ))}
    </div>
  );
}
