import { NavLink, useNavigate } from 'react-router-dom';
import { useTheme } from '../../context/theme/ThemeContext';
import { useAuth } from '../../context/auth/AuthContext';

const Header = () => {
  const { theme, toggleTheme } = useTheme();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    navigate('/login', { replace: true });
    await logout.mutateAsync();
  };

  return (
    <header
      className="sticky top-0 z-50
        w-full px-6 py-4
        bg-white/80 dark:bg-slate-900/80
        backdrop-blur-md
        border-b border-slate-200 dark:border-slate-800
        transition-colors"
    >
      <div className="max-w-7xl mx-auto flex items-center justify-between">

        {/* BRAND */}
        <NavLink
          to="/dashboard"
          className="text-lg font-semibold text-slate-800 dark:text-white"
        >
          E-Learning
        </NavLink>

        {/* NAVIGATION */}
        {user && (
          <nav className="flex items-center gap-6 text-sm">

            <NavLink
              to="/dashboard"
              className="text-slate-600 dark:text-slate-300 hover:text-indigo-600"
            >
              Dashboard
            </NavLink>

            <NavLink
              to="/courses"
              className="text-slate-600 dark:text-slate-300 hover:text-indigo-600"
            >
              Browse Courses
            </NavLink>

            {user.role === 'Instructor' && (
              <>
                <NavLink
                  to="/instructor"
                  className="text-slate-600 dark:text-slate-300 hover:text-indigo-600"
                >
                  Instructor
                </NavLink>

                <NavLink
                  to="/instructor/create-course"
                  className="text-slate-600 dark:text-slate-300 hover:text-indigo-600"
                >
                  Create Course
                </NavLink>
              </>
            )}

            {user.role === 'Admin' && (
              <NavLink
                to="/admin"
                className="text-slate-600 dark:text-slate-300 hover:text-indigo-600"
              >
                Admin
              </NavLink>
            )}

            {/* THEME TOGGLE */}
            <button
              onClick={toggleTheme}
              className="text-slate-600 dark:text-slate-300 hover:text-indigo-600 transition"
            >
              {theme === 'light' ? 'Dark' : 'Light'}
            </button>

            {/* LOGOUT */}
            <button
              onClick={handleLogout}
              className="text-red-600 hover:text-red-700 font-medium transition"
            >
              Logout
            </button>
          </nav>
        )}
      </div>
    </header>
  );
};

export default Header;
