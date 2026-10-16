import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import api from '../api.js';

export default function TaskDetail() {
  const { projectId, taskId } = useParams();
  const [comments, setComments] = useState([]);
  const [text, setText] = useState('');

  const load = async () => {
    const { data } = await api.get(`/tasks/${taskId}/comments`);
    setComments(data);
  };
  useEffect(() => { load(); }, [taskId]);

  const add = async (e) => {
    e.preventDefault();
    if (!text.trim()) return;
    await api.post(`/tasks/${taskId}/comments`, { text });
    setText('');
    load();
  };

  return (
    <div>
      <Link to={`/projects/${projectId}`}>← Board</Link>
      <h2>Task #{taskId} — Comments</h2>
      <form onSubmit={add} className="card">
        <textarea placeholder="Write a comment..." value={text} onChange={(e) => setText(e.target.value)} />
        <button type="submit">Add comment</button>
      </form>
      {comments.map((c) => (
        <div key={c.id} className="card">
          <p>{c.text}</p>
          <small className="muted">user #{c.userId} · {new Date(c.createdAt).toLocaleString()}</small>
        </div>
      ))}
    </div>
  );
}
