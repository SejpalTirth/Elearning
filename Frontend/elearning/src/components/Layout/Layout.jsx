import Header from './Header';
import Footer from './Footer';
import { useIsFetching, useIsMutating } from '@tanstack/react-query';
import { Loader } from '../ui/Loader';

const Layout = ({ children }) => {
  const isFetching = useIsFetching({
    predicate: (query) => !query.meta?.hideLoader
  });

  const isMutating = useIsMutating({
    predicate: (mutation) => !mutation.meta?.hideLoader
  });

  const isLoading = isFetching > 0 || isMutating > 0;

  return (
    <div className="min-h-screen flex flex-col
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800
      transition-colors">

      {isLoading && <Loader />}

      <Header />

      <main className="flex-1 flex items-center justify-center px-4">
        {children}
      </main>

      <Footer />
    </div>
  );
};

export default Layout;