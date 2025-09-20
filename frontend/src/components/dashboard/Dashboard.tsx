import { useAuth } from '../../hooks/auth/useAuth';

const Dashboard = () => {
  const { user, logout } = useAuth();

  return (
    <div className="container" style={{ padding: '2rem' }}>
      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
          <h1>SACCO Analytics Dashboard</h1>
          <button onClick={logout} className="btn-secondary">
            Logout
          </button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
          <div style={{ padding: '1.5rem', backgroundColor: '#f8fafc', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
            <h3 style={{ margin: '0 0 0.5rem 0', color: '#1e293b' }}>Welcome Back!</h3>
            <p style={{ margin: '0', color: '#64748b' }}>
              {user?.firstName} {user?.lastName}
            </p>
            <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.875rem', color: '#64748b' }}>
              {user?.email}
            </p>
          </div>

          <div style={{ padding: '1.5rem', backgroundColor: '#f0f9ff', borderRadius: '8px', border: '1px solid #bae6fd' }}>
            <h3 style={{ margin: '0 0 0.5rem 0', color: '#0c4a6e' }}>User Role</h3>
            <p style={{ margin: '0', color: '#0369a1' }}>
              {user?.roles?.join(', ') || 'No roles assigned'}
            </p>
          </div>

          <div style={{ padding: '1.5rem', backgroundColor: '#f0fdf4', borderRadius: '8px', border: '1px solid #bbf7d0' }}>
            <h3 style={{ margin: '0 0 0.5rem 0', color: '#14532d' }}>Tenant Status</h3>
            <p style={{ margin: '0', color: '#166534' }}>
              {user?.tenantId ? `Tenant: ${user.tenantId}` : 'No tenant assigned'}
            </p>
          </div>
        </div>

        <div style={{ padding: '1.5rem', backgroundColor: '#fefce8', borderRadius: '8px', border: '1px solid #fef08a' }}>
          <h3 style={{ marginTop: '0', color: '#713f12' }}>🚀 Development Status</h3>
          <ul style={{ margin: '0', paddingLeft: '1.5rem', color: '#a16207' }}>
            <li>✅ Backend .NET 9 API with JWT Authentication</li>
            <li>✅ Frontend React + TypeScript</li>
            <li>✅ User Registration & Login</li>
            <li>✅ Token Management & Refresh</li>
            <li>⏳ Multi-tenant Database (Next)</li>
            <li>⏳ Reporting Dashboard</li>
            <li>⏳ Financial Analytics</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
