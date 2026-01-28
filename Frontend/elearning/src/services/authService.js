import { authApiEndpoints } from './authapislice';
import { resetAuth } from '../features/auth/AuthSlice';

let isRefreshing = false;
let refreshPromise = null;

const getStore = async () => {
  const module = await import('../app/store');
  return module.store;
};

export const refreshSession = async () => {
  if (isRefreshing) return refreshPromise;

  isRefreshing = true;

  refreshPromise = (async () => {
    const store = await getStore();

    try {
      await store
        .dispatch(
          authApiEndpoints.endpoints.refresh.initiate()
        )
        .unwrap();

      return true;
    } catch {
      try {
        await store.dispatch(
          authApiEndpoints.endpoints.logout.initiate()
        );
      } catch {
        // ignore logout errors
      }

      store.dispatch(resetAuth());

      window.location.replace('/login');

      return false;
    } finally {
      isRefreshing = false;
      refreshPromise = null;
    }
  })();

  return refreshPromise;
};
