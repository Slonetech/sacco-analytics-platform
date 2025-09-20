import apiClient from './apiClient.js';

export const authApi = {
  login: async (data) => {
    const response = await apiClient.post('/auth/login', data);
    return response.data;
  },

  register: async (data) => {
    const response = await apiClient.post('/auth/register', data);
    return response.data;
  },
};
