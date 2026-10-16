import { Link, Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import Login from './pages/Login.jsx';
import Register from './pages/Register.jsx';
import Projects from './pages/Projects.jsx';
import Board from './pages/Board.jsx';
import TaskDetail from './pages/TaskDetail.jsx';
import { useAuth } from './AuthContext.jsx';

function Nav() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <nav className="nav">
      <Link to="/">TaskBoard</Link>
      <div>
        {user ? (
          <>
            <span className="muted">{user.username} ({user.role})</span>{' '}
            <button onClick={() => { logout(); navigate('/login'); }}>Logout</button>
          </>
        ) : (
          <>
            <Link to="/login">Login</Link> | <Link to="/register">Register</Link>
          </>
        )}
      </div>
    </nav>
  );
}

function Guard({ children }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  return (
    <>
      <Nav />
      <main className="container">
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<Guard><Projects /></Guard>} />
          <Route path="/projects/:id" element={<Guard><Board /></Guard>} />
          <Route path="/projects/:projectId/tasks/:taskId" element={<Guard><TaskDetail /></Guard>} />
        </Routes>
      </main>
    </>
  );
}
