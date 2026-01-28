import { useState, useMemo } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useRegisterMutation } from '../../services/authapislice';

const passwordRules = {
  length: pwd => pwd.length >= 8,
  uppercase: pwd => /[A-Z]/.test(pwd),
  lowercase: pwd => /[a-z]/.test(pwd),
  number: pwd => /[0-9]/.test(pwd),
  special: pwd => /[^A-Za-z0-9]/.test(pwd),
};

const Rule = ({ ok, label }) => (
  <li className={`text-sm flex items-center gap-2 ${ok ? 'text-green-600 dark:text-green-400' : 'text-slate-500 dark:text-slate-400'}`}>
    <span>{ok ? '✔' : '✖'}</span> {label}
  </li>
);

const Signup = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [submitted, setSubmitted] = useState(false);
  
  const navigate = useNavigate();
  const [register, { isLoading, error }] = useRegisterMutation();

  const passwordValidation = useMemo(() => ({
    length: passwordRules.length(password),
    uppercase: passwordRules.uppercase(password),
    lowercase: passwordRules.lowercase(password),
    number: passwordRules.number(password),
    special: passwordRules.special(password),
  }), [password]);

  const isPasswordValid = Object.values(passwordValidation).every(Boolean);
  const passwordsMatch = password === confirmPassword && password.length > 0;
  const isFormValid = email && isPasswordValid && passwordsMatch;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitted(true);
    if (!isFormValid) return;

    try {
      await register({ email, password }).unwrap();
      navigate('/complete-profile');
    } catch {
        //We show error message on page from return template
    }
  };

  // Shared class for inputs to keep it clean
  const inputClasses = `
    w-full px-4 py-2 rounded-lg border outline-none transition-all
    bg-white text-slate-900 border-slate-300
    dark:bg-slate-800 dark:text-white dark:border-slate-700 
    focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400
  `;

  return (
    <div className="w-full max-w-md bg-white dark:bg-slate-900 rounded-2xl shadow-xl p-8 border border-transparent dark:border-slate-800">
      <h1 className="text-2xl font-semibold text-center text-slate-800 dark:text-white">
        Create an account 
      </h1>

      <form onSubmit={handleSubmit} className="space-y-4 mt-6">
        <div>
          <input
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={e => setEmail(e.target.value)}
            className={inputClasses}
            required
          />
        </div>

        <div>
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            className={inputClasses}
            required
          />
        </div>

        <ul className="grid grid-cols-1 gap-1 mt-2 p-3 rounded-lg bg-slate-50 dark:bg-slate-800/50">
          <Rule ok={passwordValidation.length} label="Minimum 8 characters" />
          <Rule ok={passwordValidation.uppercase} label="One uppercase letter" />
          <Rule ok={passwordValidation.lowercase} label="One lowercase letter" />
          <Rule ok={passwordValidation.number} label="One number" />
          <Rule ok={passwordValidation.special} label="One special character" />
        </ul>

        <div>
          <input
            type="password"
            placeholder="Confirm password"
            value={confirmPassword}
            onChange={e => setConfirmPassword(e.target.value)}
            className={inputClasses}
            required
          />
        </div>

        {submitted && !passwordsMatch && (
          <p className="text-sm text-red-500 dark:text-red-400">Passwords do not match</p>
        )}

        {error && (
          <p className="text-sm text-red-500 dark:text-red-400">
            {error?.data?.message || 'Registration failed'}
          </p>
        )}

        <button
          type="submit"
          disabled={!isFormValid || isLoading}
          className="w-full bg-indigo-600 text-white py-2 rounded-lg font-medium
            hover:bg-indigo-700 transition-colors
            disabled:opacity-50 disabled:cursor-not-allowed
            dark:bg-indigo-500 dark:hover:bg-indigo-600"
        >
          {isLoading ? 'Creating account...' : 'Sign up'}
        </button>

        <div className="mt-6 text-center">
          <NavLink
            to="/login"
            className="text-sm font-medium text-indigo-600 dark:text-indigo-400 hover:underline transition-colors"
          >
            Back to Login
          </NavLink>
        </div>
      </form>
    </div>
  );
};

export default Signup;