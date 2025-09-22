import apiClient from './apiClient.js';

export const tenantApi = {
  getAllTenants: async () => {
    const response = await apiClient.get('/tenants');
    return response.data;
  },

  createTenant: async (tenantData) => {
    const response = await apiClient.post('/tenants', tenantData);
    return response.data;
  },

  updateTenant: async (id, tenantData) => {
    const response = await apiClient.put(`/tenants/${id}`, tenantData);
    return response.data;
  },

  deleteTenant: async (id) => {
    const response = await apiClient.delete(`/tenants/${id}`);
    return response.data;
  },
};

export const userApi = {
  getAllUsers: async (tenantId = null) => {
    const params = tenantId ? { tenantId } : {};
    const response = await apiClient.get('/users', { params });
    return response.data;
  },

  assignUserToTenant: async (userId, tenantId) => {
    const response = await apiClient.post('/users/assign-to-tenant', {
      userId,
      tenantId
    });
    return response.data;
  },

  assignRole: async (userId, roleName) => {
    const response = await apiClient.post(`/users/${userId}/roles`, {
      roleName
    });
    return response.data;
  },
};
