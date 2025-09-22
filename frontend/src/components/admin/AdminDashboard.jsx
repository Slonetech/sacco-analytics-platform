import { useState, useEffect } from 'react';
import { useAuth } from '../../hooks/auth/useAuth.tsx';
import { tenantApi, userApi } from '../../services/api/tenantApi.js';

const AdminDashboard = () => {
  const { user, logout } = useAuth();
  const [tenants, setTenants] = useState([]);
  const [users, setUsers] = useState([]);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (user?.roles?.includes('SystemAdmin')) {
      // Debug: Check if token exists
      const token = localStorage.getItem('accessToken');
      console.log('Token exists:', !!token);
      console.log('User roles:', user.roles);
      
      loadData();
    }
  }, [user]);

  const loadData = async () => {
    setLoading(true);
    try {
      // Load tenants and users in parallel
      const [tenantsData, usersData] = await Promise.all([
        tenantApi.getAllTenants(),
        userApi.getAllUsers()
      ]);
      
      setTenants(tenantsData);
      setUsers(usersData);
      setError('');
    } catch (err) {
      console.error('Error loading data:', err);
      if (err.response?.status === 401) {
        setError('Authentication failed. Please login again.');
        setTimeout(() => logout(), 2000);
      } else {
        setError(`Failed to load data: ${err.response?.data?.message || err.message}`);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTenant = async (formData) => {
    setLoading(true);
    try {
      await tenantApi.createTenant(formData);
      setShowCreateForm(false);
      await loadData(); // Reload all data
      setError('');
    } catch (err) {
      console.error('Error creating tenant:', err);
      setError(`Failed to create tenant: ${err.response?.data?.message || err.message}`);
    } finally {
      setLoading(false);
    }
  };

  if (!user?.roles?.includes('SystemAdmin')) {
    return (
      <div className="container" style={{ padding: '2rem' }}>
        <div className="card">
          <h2>Access Denied</h2>
          <p>You need SystemAdmin privileges to access this page.</p>
          <p>Your current roles: {user?.roles?.join(', ') || 'None'}</p>
          <button onClick={logout} className="btn-secondary" style={{ marginTop: '1rem' }}>
            Logout
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container" style={{ padding: '2rem' }}>
      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
          <div>
            <h1>System Administration</h1>
            <p style={{ color: '#64748b', margin: '0.5rem 0 0 0' }}>
              Welcome, {user.firstName} {user.lastName}
            </p>
          </div>
          <button onClick={logout} className="btn-secondary">
            Logout
          </button>
        </div>

        {error && (
          <div style={{
            padding: '1rem',
            backgroundColor: '#fee2e2',
            color: '#dc2626',
            borderRadius: '4px',
            marginBottom: '1rem',
            border: '1px solid #fecaca'
          }}>
            {error}
          </div>
        )}

        {loading && (
          <div style={{
            padding: '1rem',
            backgroundColor: '#f0f9ff',
            color: '#0369a1',
            borderRadius: '4px',
            marginBottom: '1rem',
            textAlign: 'center'
          }}>
            Loading...
          </div>
        )}

        {/* Tenant Management Section */}
        <div style={{ marginBottom: '3rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h2>SACCO Tenants ({tenants.length})</h2>
            <button 
              onClick={() => setShowCreateForm(true)} 
              className="btn-primary"
              disabled={loading}
            >
              Create New SACCO
            </button>
          </div>

          {showCreateForm && (
            <CreateTenantForm 
              onSubmit={handleCreateTenant}
              onCancel={() => setShowCreateForm(false)}
              loading={loading}
            />
          )}

          {tenants.length === 0 ? (
            <div style={{ 
              padding: '2rem', 
              textAlign: 'center', 
              backgroundColor: '#f8fafc',
              borderRadius: '8px',
              color: '#64748b'
            }}>
              No tenants created yet. Create your first SACCO to get started.
            </div>
          ) : (
            <div style={{ display: 'grid', gap: '1rem' }}>
              {tenants.map(tenant => (
                <TenantCard key={tenant.id} tenant={tenant} />
              ))}
            </div>
          )}
        </div>

        {/* Users Section */}
        <div>
          <h2>Users ({users.length})</h2>
          {users.length === 0 ? (
            <div style={{ 
              padding: '2rem', 
              textAlign: 'center', 
              backgroundColor: '#f8fafc',
              borderRadius: '8px',
              color: '#64748b'
            }}>
              No users found.
            </div>
          ) : (
            <div style={{ display: 'grid', gap: '1rem' }}>
              {users.map(user => (
                <UserCard key={user.id} user={user} tenants={tenants} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const CreateTenantForm = ({ onSubmit, onCancel, loading }) => {
  const [formData, setFormData] = useState({
    name: '',
    code: '',
    description: '',
    contactEmail: '',
    contactPhone: ''
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div style={{ 
      backgroundColor: '#f8fafc', 
      padding: '1.5rem', 
      borderRadius: '8px', 
      marginBottom: '1rem',
      border: '1px solid #e2e8f0'
    }}>
      <h3 style={{ marginTop: '0' }}>Create New SACCO</h3>
      <form onSubmit={handleSubmit}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              SACCO Name
            </label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({...formData, name: e.target.value})}
              required
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
              placeholder="e.g., Nakuru Teachers SACCO"
            />
          </div>
          
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              SACCO Code
            </label>
            <input
              type="text"
              value={formData.code}
              onChange={(e) => setFormData({...formData, code: e.target.value.toUpperCase()})}
              required
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
              placeholder="e.g., NKRTS"
            />
          </div>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({...formData, description: e.target.value})}
            style={{
              width: '100%',
              padding: '0.5rem',
              border: '1px solid #ddd',
              borderRadius: '4px',
              minHeight: '80px'
            }}
            placeholder="Brief description of the SACCO"
          />
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Contact Email
            </label>
            <input
              type="email"
              value={formData.contactEmail}
              onChange={(e) => setFormData({...formData, contactEmail: e.target.value})}
              required
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
              placeholder="admin@sacco.com"
            />
          </div>
          
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Contact Phone
            </label>
            <input
              type="tel"
              value={formData.contactPhone}
              onChange={(e) => setFormData({...formData, contactPhone: e.target.value})}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
              placeholder="+254712345678"
            />
          </div>
        </div>

        <div style={{ display: 'flex', gap: '1rem' }}>
          <button 
            type="submit" 
            disabled={loading}
            className="btn-primary"
            style={{ opacity: loading ? 0.7 : 1 }}
          >
            {loading ? 'Creating...' : 'Create SACCO'}
          </button>
          <button 
            type="button" 
            onClick={onCancel}
            className="btn-secondary"
            disabled={loading}
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

const TenantCard = ({ tenant }) => (
  <div style={{ 
    padding: '1rem', 
    border: '1px solid #e2e8f0', 
    borderRadius: '8px',
    backgroundColor: 'white'
  }}>
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
      <div>
        <h3 style={{ margin: '0 0 0.5rem 0' }}>{tenant.name}</h3>
        <p style={{ margin: '0', color: '#64748b', fontSize: '0.875rem' }}>
          Code: <strong>{tenant.code}</strong> | Email: {tenant.contactEmail}
        </p>
        {tenant.description && (
          <p style={{ margin: '0.5rem 0 0 0', color: '#64748b' }}>
            {tenant.description}
          </p>
        )}
        <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.75rem', color: '#64748b' }}>
          Created: {new Date(tenant.createdAt).toLocaleDateString()}
        </p>
      </div>
      <span style={{ 
        padding: '0.25rem 0.75rem', 
        backgroundColor: '#dcfce7', 
        color: '#166534', 
        borderRadius: '12px',
        fontSize: '0.75rem'
      }}>
        Active
      </span>
    </div>
  </div>
);

const UserCard = ({ user, tenants }) => {
  const tenantName = tenants.find(t => t.id === user.tenantId)?.name || 'No tenant assigned';
  
  return (
    <div style={{ 
      padding: '1rem', 
      border: '1px solid #e2e8f0', 
      borderRadius: '8px',
      backgroundColor: 'white'
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h4 style={{ margin: '0 0 0.5rem 0' }}>{user.firstName} {user.lastName}</h4>
          <p style={{ margin: '0', color: '#64748b', fontSize: '0.875rem' }}>
            {user.email}
          </p>
          <p style={{ margin: '0.25rem 0 0 0', color: '#64748b', fontSize: '0.875rem' }}>
            Tenant: <strong>{tenantName}</strong>
          </p>
        </div>
        <div style={{ textAlign: 'right' }}>
          <p style={{ margin: '0', fontSize: '0.75rem', color: '#64748b' }}>
            Joined: {new Date(user.createdAt).toLocaleDateString()}
          </p>
          {user.lastLoginAt && (
            <p style={{ margin: '0', fontSize: '0.75rem', color: '#64748b' }}>
              Last login: {new Date(user.lastLoginAt).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
