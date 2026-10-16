import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import api from '../api.js';

const COLUMNS = ['Todo', 'InProgress', 'Done'];

export function isOverdue(task) {
  if (!task.dueDate || task.status === 'Done') return false;
  return new Date(task.dueDate) < new Date();
}

export default function Board() {
  const { id } = useParams();
  const [project, setProject] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [form, setForm] = useState({ title: '', description: '', priority: 'Medium', dueDate: '' });
  const [err, setErr] = useState('');

  const load = async () => {
    const [p, t] = await Promise.all([api.get(`/projects/${id}`), api.get(`/projects/${id}/tasks`)]);
    setProject(p.data);
    setTasks(t.data);
  };
  useEffect(() => { load(); }, [id]);

  const create = async (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    await api.post(`/projects/${id}/tasks`, {
      title: form.title,
      description: form.description || null,
      priority: form.priority,
      dueDate: form.dueDate || null
    });
    setForm({ title: '', description: '', priority: 'Medium', dueDate: '' });
    load();
  };

  const move = async (task, status) => {
    setErr('');
    try {
      await api.post(`/tasks/${task.id}/move`, { status });
      load();
    } catch (e) {
      setErr(e.response?.data || 'Move failed. Done tasks cannot move back.');
    }
  };

  const remove = async (task) => {
    if (!confirm('Delete task?')) return;
    try { await api.delete(`/tasks/${task.id}`); load(); }
    catch { setErr('Delete not allowed. Manager or creator only.'); }
  };

  return (
    <div>
      <Link to="/">← Projects</Link>
      <h2>{project?.name}</h2>
      <p className="muted">{project?.description}</p>
      {err && <p style={{ color: 'red' }}>{err}</p>}
      <form onSubmit={create} className="card">
        <input placeholder="Task title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        <input placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        <select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
        <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        <button type="submit">Add task</button>
      </form>
      <div className="board">
        {COLUMNS.map((col) => (
          <div key={col} className="col">
            <h3>{col}</h3>
            {tasks.filter((t) => String(t.status) === col || (typeof t.status === 'number' && [['Todo','InProgress','Done'][t.status]] == col)).map((t) => (
              <div key={t.id} className="card">
                <Link to={`/projects/${id}/tasks/${t.id}`}><strong>{t.title}</strong></Link>
                <div><span className={`badge ${t.priority}`}>{typeof t.priority === 'number' ? ['Low','Medium','High'][t.priority] : t.priority}</span></div>
                {isOverdue(t) && <div className="overdue">Overdue!</div>}
                <div>
                  {col !== 'Todo' && <button className="ghost" onClick={() => move(t, col === 'Done' ? 'InProgress' : 'Todo')}>◀</button>}
                  {col !== 'Done' && <button className="ghost" onClick={() => move(t, col === 'Todo' ? 'InProgress' : 'Done')}>▶</button>}
                  <button className="ghost" onClick={() => remove(t)}>Delete</button>
                </div>
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
