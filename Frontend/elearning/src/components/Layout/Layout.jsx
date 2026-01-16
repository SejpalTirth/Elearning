import Header from './Header';
import Footer from './Footer';

const Layout = ({ children }) => {
  return (
    <div className="min-h-screen flex flex-col
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800
      transition-colors">

      <Header />

      <main className="flex-1 flex items-center justify-center px-4">
        {children}
      </main>

      <Footer />
    </div>
  );
};

export default Layout;
