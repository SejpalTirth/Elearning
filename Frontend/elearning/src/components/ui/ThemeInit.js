import { useEffect } from 'react';
import { useSelector } from 'react-redux';

export const ThemeInit = () => {
  const mode = useSelector((state) => state.theme.mode);

  useEffect(() => {
    const root = document.documentElement;
    if (mode === 'dark') {
      root.classList.add('dark');
    } else {
      root.classList.remove('dark');
    }
  }, [mode]);

  return null;
};