import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../../context/auth/AuthContext';
import { useQueryClient } from '@tanstack/react-query';
import { Loader } from '../../components/ui/Loader';

const AuthCallback = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { user } = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    const isNewUser = searchParams.get('isNewUser');
    const userId = searchParams.get('userId');

    if (isNewUser === 'true' && userId) {
      navigate(`/complete-profile?userId=${userId}`, { replace: true });
      return;
    }

    queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });

  }, [searchParams, navigate, queryClient]);

  useEffect(() => {
    if (user) {

      navigate('/dashboard', { replace: true });
      
    }
  }, [user, navigate]);

  return <Loader />;
};

export default AuthCallback;