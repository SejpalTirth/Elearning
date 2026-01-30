import { NavLink, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { useLogoutMutation } from '../../services/authapislice';
import { resetAuth } from '../../features/auth/AuthSlice';
import { toggleTheme } from '../../features/ui/themeSlice'; 
import { showToast } from '../../features/ui/toastSlice';
import { BookOpen } from 'lucide-react';

const Header = () => {
  const theme = useSelector((state) => state.theme.mode);
  const user = useSelector((state) => state.auth.user);
  
  const [logout] = useLogoutMutation();
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const handleLogout = async () => {
  try {
    await logout().unwrap();
    dispatch(
      showToast({
        message: 'Logged out successfully',
        type: 'info',
      })
    );
  } catch {
    // intentionally ignored
  } finally {
    // 🔒 lock auth state
    dispatch(resetAuth());

    // ❌ DO NOT resetApiState
    // dispatch(authApiSlice.util.resetApiState());

    navigate('/login', { replace: true });
  }
};



  return (
    <header
      className="
        sticky top-0 z-50 w-full px-6 py-4
        bg-white/80 dark:bg-slate-900/80
        backdrop-blur-md
        border-b border-slate-200 dark:border-slate-800
        transition-colors
      "
    >
      <div className="max-w-7xl mx-auto flex items-center justify-between">

        {/* BRAND */}
        <NavLink
          to="/dashboard"
          className="text-lg font-semibold text-slate-800 dark:text-white"
        >
          E-Learning
        </NavLink>

        {/* RIGHT SIDE */}
        <div className="flex items-center gap-6 text-sm">

          {/* NAV LINKS (only when logged in) */}
          {user && (
            <>
              <NavLink
                to="/dashboard"
                className="text-slate-600 dark:text-slate-300 hover:text-indigo-600 transition"
              >
                Dashboard
              </NavLink>

              <NavLink
                to="/courses"
                className="text-slate-600 dark:text-slate-300 hover:text-indigo-600 transition"
              >
                Browse Courses
              </NavLink>

              {/* NEW CONTINUE LEARNING BUTTON */}
              <NavLink
                to="/my-learning"
                className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-indigo-50 dark:bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 font-medium hover:bg-indigo-100 dark:hover:bg-indigo-500/20 transition"
              >
                <BookOpen className="w-4 h-4" />
                Continue Learning
              </NavLink>

              {user.role === 'Instructor' && (
                <NavLink
                  to="/instructor/create-course"
                  className="text-slate-600 dark:text-slate-300 hover:text-indigo-600 transition"
                >
                  Create Course
                </NavLink>
              )}

              {user.role === 'Admin' && (
                <NavLink
                  to="/admin"
                  className="text-slate-600 dark:text-slate-300 hover:text-indigo-600 transition"
                >
                  Admin
                </NavLink>
              )}
            </>
          )}

          {/* THEME TOGGLE */}
          <button
            onClick={() => dispatch(toggleTheme())}
            className="text-slate-600 dark:text-slate-300 flex items-center gap-1"
          >
            {theme === 'light' ? '☀️ Light' : '🌙 Dark'}
          </button>

          {/* LOGOUT */}
          {user && (
            <button
              onClick={handleLogout}
              className="text-red-600 hover:text-red-700 font-medium transition"
            >
              Logout
            </button>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;