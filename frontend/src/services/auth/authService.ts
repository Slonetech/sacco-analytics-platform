import type { User } from '../api/authApi';

export const authService = {
  setTokens: (accessToken: string, refreshToken: string) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  },

  getAccessToken: (): string | null => {
    return localStorage.getItem('accessToken');
  },

  getRefreshToken: (): string | null => {
    return localStorage.getItem('refreshToken');
  },

  setUser: (user: User) => {
    localStorage.setItem('user', JSON.stringify(user));
  },

  getUser: (): User | null => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('accessToken');
  },
};
