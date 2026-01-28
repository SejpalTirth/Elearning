import { useSelector } from 'react-redux';
import { Loader } from './Loader';
import { selectIsLoading } from '../../features/ui/loadingSlice';

export const GlobalLoader = () => {
  const isLoading = useSelector(selectIsLoading);

  if (!isLoading) return null;

  return <Loader />;
};