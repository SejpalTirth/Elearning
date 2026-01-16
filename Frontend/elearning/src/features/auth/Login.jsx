import { useState } from 'react';
import { useAuth } from '../../context/auth/AuthContext';
import { NavLink } from 'react-router-dom';

const Login = () => {
  const { login } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    login.mutate({ email, password });
  };

  return (
    <div className="min-h-screen flex items-center justify-center
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800 text:white
      transition-colors duration-500 px-4">

      <div className="w-full max-w-md bg-white dark:bg-slate-900
        rounded-2xl shadow-xl p-8
        transform transition-all duration-500
        hover:shadow-2xl hover:-translate-y-1">

        {/* Header */}
        <div className="text-center mb-6">
          <h1 className="text-2xl font-semibold text-slate-800 text:white dark:text-white">
            Welcome Back 👋
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
            Log into your account
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full px-4 py-2 rounded-lg border
              border-slate-300 dark:border-slate-700
              bg-white dark:bg-slate-800 text:white
              text-slate-800 text:white dark:text-white
              focus:ring-2 focus:ring-indigo-500
              transition"
            required
          />

          <input
            type="password"
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full px-4 py-2 rounded-lg border
              border-slate-300 dark:border-slate-700
              bg-white dark:bg-slate-800 text:white
              text-slate-800 text:white dark:text-white
              focus:ring-2 focus:ring-indigo-500
              transition"
            required
          />

          <button
            type="submit"
            disabled={login.isLoading}
            className="w-full bg-indigo-600 hover:bg-indigo-700
              text-white font-medium py-2 rounded-lg
              transition-all duration-300
              hover:scale-[1.02]
              disabled:opacity-50"
          >
            {login.isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        {/* Divider */}
        <div className="flex items-center my-6">
          <div className="flex-grow border-t dark:border-slate-700" />
          <span className="mx-3 text-xs text-slate-400">OR</span>
          <div className="flex-grow border-t dark:border-slate-700" />
        </div>

        {/* Social buttons */}
        <div className="space-y-3">
          <button className="w-full flex items-center justify-center gap-3
            border rounded-lg py-2
            hover:bg-slate-100 dark:hover:bg-slate-800 dark:text-white
            transition">
            <img src="https://www.svgrepo.com/show/475656/google-color.svg" className="w-5 h-5" />
            Continue with Google
          </button>

          <button className="w-full flex items-center justify-center gap-3
            border rounded-lg py-2
            hover:bg-slate-100 dark:hover:bg-slate-800 dark:text-white
            transition">
            <img src="https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg"
                 className="w-5 h-5" />
            Continue with Microsoft
          </button>
        </div>

        {/* Footer */}
        <p className="text-center text-sm text-slate-500 dark:text-slate-400 mt-6">
          Don’t have an account?{' '}
          <NavLink
          to = '/sign-up'
          className="text-indigo-600 hover:underline cursor-pointer">
            Sign up
          </NavLink>
        </p>
      </div>
    </div>
  );
};

export default Login;
