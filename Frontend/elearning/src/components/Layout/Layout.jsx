import { useSelector } from 'react-redux';
import Header from './Header';
import Footer from './Footer';
import { Loader } from '../ui/Loader';

const Layout = ({ children }) => {
  const isLoading = useSelector((state) => {
    const queries = state.authApi?.queries ?? {};
    const mutations = state.authApi?.mutations ?? {};

    const isAnyQueryLoading = Object.values(queries).some(
      (q) => q?.status === 'pending'
    );

    const isAnyMutationLoading = Object.values(mutations).some(
      (m) => m?.status === 'pending'
    );

    return isAnyQueryLoading || isAnyMutationLoading;
  });

  return (
    <div
      className="min-h-screen flex flex-col
      bg-gradient-to-br from-indigo-50 to-slate-100
      dark:from-slate-900 dark:to-slate-800
      transition-colors"
    >
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
