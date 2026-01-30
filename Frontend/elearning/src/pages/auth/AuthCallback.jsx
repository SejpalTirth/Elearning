import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useSelector } from 'react-redux';

import { Loader } from '../../components/ui/Loader';

const AuthCallback = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const { user, authChecked } = useSelector((state) => state.auth);

  const isNewUser = searchParams.get('isNewUser');
  const userId = searchParams.get('userId');

  /* ---------------------------------------
     HANDLE NEW USER FLOW
  --------------------------------------- */
  useEffect(() => {
    if (!authChecked) return;

    if (isNewUser === 'true' && userId) {
      navigate(`/complete-profile?userId=${userId}`, { replace: true });
    }
  }, [authChecked, isNewUser, userId, navigate]);

  /* ---------------------------------------
     HANDLE EXISTING USER FLOW
  --------------------------------------- */
  useEffect(() => {
    if (!authChecked) return;

    if (user) {
      navigate('/dashboard', { replace: true });
    }
  }, [authChecked, user, navigate]);

  return <Loader />;
};

export default AuthCallback;
