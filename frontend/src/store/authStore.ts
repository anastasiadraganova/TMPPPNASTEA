import { create } from 'zustand';
import { UserDto } from '@/types';
import { apiFacade } from '@/lib/apiFacade';

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
  token: typeof window !== 'undefined' ? apiFacade.getStoredToken() : null,
  isLoading: false,

  login: async (email, password) => {
    set({ isLoading: true });
    try {
      const user = await apiFacade.login({ email, password });
      set({ user, token: apiFacade.getStoredToken() });
    } finally {
      set({ isLoading: false });
    }
  },

  register: async (email, password, name, role) => {
    set({ isLoading: true });
    try {
      const user = await apiFacade.register({ email, password, name, role });
      set({ user, token: apiFacade.getStoredToken() });
    } finally {
      set({ isLoading: false });
    }
  },

  logout: () => {
    apiFacade.logout();
    set({ user: null, token: null });
  },

  loadUser: async () => {
    const token = apiFacade.getStoredToken();
    if (!token) return;
    set({ isLoading: true });
    try {
      const user = await apiFacade.loadCurrentUser();
      if (!user) {
        set({ user: null, token: null });
        return;
      }
      set({ user, token });
    } catch {
      apiFacade.logout();
      set({ user: null, token: null });
    } finally {
      set({ isLoading: false });
    }
  },
}));
