import { useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useLocation } from 'react-router-dom';
import { bootstrapAuth } from './authBootstrap';

const AuthBootstrapper = ({ children }) => {
  const dispatch = useDispatch();
  const location = useLocation();
  const initialized = useRef(false);

  const authStatus = useSelector((state) => state.auth.status);

  useEffect(() => {
    if (initialized.current) return;
    initialized.current = true;

    if (location.pathname === '/login') return;
    if (location.pathname === '/') return;

    if (authStatus === 'unauthenticated') return;

    dispatch(bootstrapAuth());
  }, [dispatch, location.pathname, authStatus]);

  return children;
};

export default AuthBootstrapper;
