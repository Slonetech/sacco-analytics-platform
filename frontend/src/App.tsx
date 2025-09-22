import { useAuth, AuthProvider } from './hooks/auth/useAuth.tsx';
import LoginForm from './components/auth/LoginForm.tsx';
import Dashboard from './components/dashboard/Dashboard.tsx';
import AdminDashboard from './components/admin/AdminDashboard.jsx';
import './App.css';

function AppContent() {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <div>Loading...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  // Show admin dashboard for system admins
  if (user?.roles?.includes('SystemAdmin')) {
    return <AdminDashboard />;
  }

  // Show regular dashboard for other users
  return <Dashboard />;
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
