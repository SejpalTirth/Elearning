const Footer = () => {
  return (
    <footer
      className="w-full py-4
        bg-white/80 dark:bg-slate-900/80
        backdrop-blur
        border-t border-slate-200 dark:border-slate-800
        transition-colors"
    >
      <p className="text-center text-sm text-slate-500 dark:text-slate-400">
        © {new Date().getFullYear()} E-Learning. All rights reserved.
      </p>
    </footer>
  );
};

export default Footer;
