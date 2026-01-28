import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useLoginMutation } from '../../services/authapislice';
import { useDispatch } from 'react-redux';
import { showToast } from '../../features/ui/toastSlice';

const Login = () => {
  const navigate = useNavigate();

  const [
    login,
    { isLoading, isError, error, reset }
  ] = useLoginMutation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const dispatch = useDispatch();

  const handleSubmit = async (e) => {
  e.preventDefault();
  try {
    await login({ email, password }).unwrap();
    dispatch({ type: 'auth/allowMe' });
    dispatch(showToast({message : "Logged in successfully", type : 'success'}));
    navigate('/dashboard', { replace: true });
  } catch {
    // error handled by RTK Query
  }
};

  const handleSSOLogin = (provider) => {
    const baseURL = 'https://localhost:7249';
    const endpoint =
      provider === 'google'
        ? '/api/GatewayAuth/google-login'
        : '/api/GatewayAuth/microsoft-login';

    window.location.href = `${baseURL}${endpoint}`;
  };

  return (
    <div
      className="min-h-screen flex items-center justify-center
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800
      transition-colors duration-500 px-4"
    >
      <div
        className="w-full max-w-md bg-white dark:bg-slate-900
        rounded-2xl shadow-xl p-8"
      >
        <div className="text-center mb-6">
          <h1 className="text-2xl font-semibold text-slate-800 dark:text-white">
            Welcome Back
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
            Log into your account
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              reset();
            }}
            className="w-full px-4 py-2 rounded-lg border
              border-slate-300 dark:border-slate-700
              bg-white dark:bg-slate-800
              text-slate-800 dark:text-white
              focus:ring-2 focus:ring-indigo-500
              outline-none"
            required
          />

          <input
            type="password"
            placeholder="••••••••"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value);
              reset();
            }}
            className="w-full px-4 py-2 rounded-lg border
              border-slate-300 dark:border-slate-700
              bg-white dark:bg-slate-800
              text-slate-800 dark:text-white
              focus:ring-2 focus:ring-indigo-500
              outline-none"
            required
          />

          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-indigo-600 hover:bg-indigo-700
              text-white font-medium py-2 rounded-lg
              transition
              disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? 'Logging in...' : 'Login'}
          </button>

          {isError && (
            <p className="text-sm text-red-500 text-center mt-2">
              {error?.data?.message || 'Invalid email or password'}
            </p>
          )}
        </form>

        <div className="flex items-center my-6">
          <div className="flex-grow border-t dark:border-slate-700" />
          <span className="mx-3 text-xs text-slate-400">OR</span>
          <div className="flex-grow border-t dark:border-slate-700" />
        </div>

        <div className="space-y-3">
          <button
            type="button"
            onClick={() => handleSSOLogin('google')}
            className="w-full flex items-center justify-center gap-3
              border border-slate-200 dark:border-slate-700 rounded-lg py-2
              hover:bg-slate-50 dark:hover:bg-slate-800 dark:text-white
              transition"
          >
            <img
              src="https://www.svgrepo.com/show/475656/google-color.svg"
              className="w-5 h-5"
              alt="Google"
            />
            Continue with Google
          </button>

          <button
            type="button"
            onClick={() => handleSSOLogin('microsoft')}
            className="w-full flex items-center justify-center gap-3
              border border-slate-200 dark:border-slate-700 rounded-lg py-2
              hover:bg-slate-50 dark:hover:bg-slate-800 dark:text-white
              transition"
          >
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg"
              className="w-5 h-5"
              alt="Microsoft"
            />
            Continue with Microsoft
          </button>
        </div>

        <p className="text-center text-sm text-slate-500 dark:text-slate-400 mt-6">
          Don’t have an account?{' '}
          <NavLink
            to="/sign-up"
            className="text-indigo-600 dark:text-indigo-400 hover:underline font-medium"
          >
            Sign up
          </NavLink>
        </p>
      </div>
    </div>
  );
};

export default Login;
