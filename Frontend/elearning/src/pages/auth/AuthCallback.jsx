import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';

import { runMeOnce } from '../../features/auth/AuthSlice';
import { Loader } from '../../components/ui/Loader';

const AuthCallback = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const user = useSelector((state) => state.auth.user);

  const isNewUser = searchParams.get('isNewUser');
  const userId = searchParams.get('userId');

  useEffect(() => {
    dispatch({ type: 'auth/allowMe' });
    dispatch(runMeOnce());
  }, [dispatch]);

  useEffect(() => {
    if (isNewUser === 'true' && userId) {
      navigate(`/complete-profile?userId=${userId}`, { replace: true });
    }
  }, [isNewUser, userId, navigate]);

  useEffect(() => {
    if (user) {
      navigate('/dashboard', { replace: true });
    }
  }, [user, navigate]);

  return <Loader />;
};

export default AuthCallback;
