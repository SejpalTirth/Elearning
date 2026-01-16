import { useState, useMemo } from 'react';
import { useMutation } from '@tanstack/react-query';
import { localRegistrationMutation } from '../../queries/auth/auth.mutations';
import { NavLink } from 'react-router-dom';

/* -----------------------------
   Password rules helpers
----------------------------- */
const passwordRules = {
  length: pwd => pwd.length >= 8,
  uppercase: pwd => /[A-Z]/.test(pwd),
  lowercase: pwd => /[a-z]/.test(pwd),
  number: pwd => /[0-9]/.test(pwd),
  special: pwd => /[^A-Za-z0-9]/.test(pwd),
};

/* -----------------------------
   Rule component (MOVED OUTSIDE)
----------------------------- */
const Rule = ({ ok, label }) => (
  <li className={`text-sm ${ok ? 'text-green-600' : 'text-slate-500'}`}>
    {ok ? '✔' : '✖'} {label}
  </li>
);

const Signup = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const mutation = useMutation({
    mutationFn: localRegistrationMutation,
  });

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

  const handleSubmit = e => {
    e.preventDefault();
    setSubmitted(true);
    if (!isFormValid) return;
    mutation.mutate({ email, password });
  };

  return (
    <div className="w-full max-w-md bg-white dark:bg-slate-900 rounded-2xl shadow-xl p-8">
      <h1 className="text-2xl font-semibold text-center text-slate-800 dark:text-white">
        Create an account 🚀
      </h1>

      <form onSubmit={handleSubmit} className="space-y-4 mt-6">
        <input
          type="email"
          placeholder="you@example.com"
          value={email}
          onChange={e => setEmail(e.target.value)}
          className="w-full px-4 py-2 rounded-lg border"
          required
        />

        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          className="w-full px-4 py-2 rounded-lg border"
          required
        />

        {/* Password rules */}
        <ul className="space-y-1 mt-2">
          <Rule ok={passwordValidation.length} label="Minimum 8 characters" />
          <Rule ok={passwordValidation.uppercase} label="One uppercase letter" />
          <Rule ok={passwordValidation.lowercase} label="One lowercase letter" />
          <Rule ok={passwordValidation.number} label="One number" />
          <Rule ok={passwordValidation.special} label="One special character" />
        </ul>

        <input
          type="password"
          placeholder="Confirm password"
          value={confirmPassword}
          onChange={e => setConfirmPassword(e.target.value)}
          className="w-full px-4 py-2 rounded-lg border"
          required
        />

        {submitted && !passwordsMatch && (
          <p className="text-sm text-red-500">Passwords do not match</p>
        )}

        <button
          type="submit"
          disabled={!isFormValid || mutation.isLoading}
          className="w-full bg-indigo-600 text-white py-2 rounded-lg disabled:opacity-50"
        >
          {mutation.isLoading ? 'Creating account...' : 'Sign up'}
        </button>
        <div className="mt-6 text-center">
        <NavLink
            to="/login"
            className="inline-flex items-center gap-1
            text-sm font-medium text-indigo-600
            hover:text-indigo-700 hover:underline
            transition-colors"
        >
            Back to Login
        </NavLink>
</div>
      </form>
    </div>
  );
};

export default Signup;
