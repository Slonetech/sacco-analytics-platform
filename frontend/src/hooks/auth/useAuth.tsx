import { useState, useEffect, createContext, useContext } from 'react';
import { authApi } from '../../services/api/authApi.js';
import { authService } from '../../services/auth/authService.js';

const AuthContext = createContext(undefined);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const isAuthenticated = !!user;

  useEffect(() => {
    const savedUser = authService.getUser();
    if (savedUser && authService.isAuthenticated()) {
      setUser(savedUser);
    }
    setIsLoading(false);
  }, []);

  const login = async (credentials) => {
    try {
      setIsLoading(true);
      const response = await authApi.login(credentials);
      
      authService.setTokens(response.accessToken, response.refreshToken);
      authService.setUser(response.user);
      setUser(response.user);
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData) => {
    try {
      setIsLoading(true);
      await authApi.register(userData);
      await login({ email: userData.email, password: userData.password });
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const value = {
    user,
    isLoading,
    isAuthenticated,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
