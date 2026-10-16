import { createContext, useContext, useState } from 'react';
import api from './api.js';

const AuthCtx = createContext(null);
export const useAuth = () => useContext(AuthCtx);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('tb_token');
    const username = localStorage.getItem('tb_user');
    const role = localStorage.getItem('tb_role');
    return token ? { token, username, role } : null;
  });

  const save = (data) => {
    localStorage.setItem('tb_token', data.token);
    localStorage.setItem('tb_user', data.username);
    localStorage.setItem('tb_role', data.role);
    setUser({ token: data.token, username: data.username, role: data.role });
  };

  const login = async (username, password) => {
    const { data } = await api.post('/auth/login', { username, password });
    save(data);
  };

  const register = async (username, password, role) => {
    const { data } = await api.post('/auth/register', { username, password, role });
    save(data);
  };

  const logout = () => {
    localStorage.removeItem('tb_token');
    localStorage.removeItem('tb_user');
    localStorage.removeItem('tb_role');
    setUser(null);
  };

  return <AuthCtx.Provider value={{ user, login, register, logout }}>{children}</AuthCtx.Provider>;
}
