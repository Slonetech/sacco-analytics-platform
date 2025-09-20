export const authService = {
  setTokens: (accessToken, refreshToken) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  },

  getAccessToken: () => {
    return localStorage.getItem('accessToken');
  },

  setUser: (user) => {
    localStorage.setItem('user', JSON.stringify(user));
  },

  getUser: () => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('accessToken');
  },
};
