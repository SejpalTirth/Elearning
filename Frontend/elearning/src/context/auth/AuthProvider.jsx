import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useLocation, useNavigate } from 'react-router-dom';
import { AuthContext } from './AuthContext';
import { fetchMe } from '../../queries/auth/auth.queries';
import { localLoginMutation, logoutMutation } from '../../queries/auth/auth.mutations';

export const AuthProvider = ({ children }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  const isAuthPage = ['/login', '/sign-up', '/', '/complete-profile'].includes(location.pathname);

  const { data: user, isLoading: isCheckingAuth } = useQuery({
    queryKey: ['auth', 'me'],
    queryFn: fetchMe,
    enabled: !isAuthPage,
    retry: false,
    refetchOnWindowFocus: false,
    meta: { hideLoader: true }
  });

  const login = useMutation({
    mutationFn: localLoginMutation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
      navigate('/dashboard');
    }
  });

  const logout = useMutation({
    mutationFn: logoutMutation,
    onSuccess: () => {
      queryClient.clear();
      navigate('/login', { replace: true });
    }
  });


  const value = {
    user,
    isAuthenticated: !!user,
    isCheckingAuth,
    login,
    logout
  };


  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};