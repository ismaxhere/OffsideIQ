import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  token: localStorage.getItem('offsideiq_token'),
  user: JSON.parse(localStorage.getItem('offsideiq_user') || 'null'),

  setAuth: (token, user) => {
    localStorage.setItem('offsideiq_token', token);
    localStorage.setItem('offsideiq_user', JSON.stringify(user));
    set({ token, user });
  },

  logout: () => {
    localStorage.removeItem('offsideiq_token');
    localStorage.removeItem('offsideiq_user');
    set({ token: null, user: null });
  },
}));
