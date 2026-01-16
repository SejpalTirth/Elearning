import { useTheme } from '../../context/theme/ThemeContext';

const Header = () => {
  const { theme, toggleTheme } = useTheme();

  return (
    <header className="w-full px-6 py-4
      bg-white dark:bg-slate-900
      border-b border-slate-200 dark:border-slate-800
      transition-colors">

      <div className="max-w-7xl mx-auto flex items-center justify-between">
        {/* Logo / Brand */}
        <h1 className="text-lg font-semibold text-slate-800 dark:text-white">
          E-Learning
        </h1>

        {/* Theme Toggle */}
        <button
          onClick={toggleTheme}
          className="flex items-center gap-2 text-sm
            text-slate-600 dark:text-slate-300
            hover:text-indigo-600 dark:hover:text-indigo-400
            transition"
        >
          {theme === 'light' ? '🌙 Dark' : '☀️ Light'}
        </button>
      </div>
    </header>
  );
};

export default Header;
