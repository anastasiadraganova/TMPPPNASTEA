import { create } from 'zustand';
import { UserDto } from '@/types';
import { apiClient } from '@/lib/api';

interface AuthState {
  user: UserDto | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, name: string, role: 'Customer' | 'Freelancer') => Promise<void>;
  logout: () => void;
  loadUser: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: typeof window !== 'undefined' ? localStorage.getItem('token') : null,
  isLoading: false,

  login: async (email, password) => {
    set({ isLoading: true });
    try {
      const res = await apiClient.login({ email, password });
      localStorage.setItem('token', res.token);
      set({ user: res.user, token: res.token });
    } finally {
      set({ isLoading: false });
    }
  },

  register: async (email, password, name, role) => {
    set({ isLoading: true });
    try {
      const res = await apiClient.register({ email, password, name, role });
      localStorage.setItem('token', res.token);
      set({ user: res.user, token: res.token });
    } finally {
      set({ isLoading: false });
    }
  },

  logout: () => {
    localStorage.removeItem('token');
    set({ user: null, token: null });
  },

  loadUser: async () => {
    const token = localStorage.getItem('token');
    if (!token) return;
    set({ isLoading: true });
    try {
      const user = await apiClient.getMe();
      set({ user, token });
    } catch {
      localStorage.removeItem('token');
      set({ user: null, token: null });
    } finally {
      set({ isLoading: false });
    }
  },
}));
