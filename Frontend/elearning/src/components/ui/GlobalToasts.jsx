import { useSelector, useDispatch } from 'react-redux';
import { ToastContainer } from './Toast';
import { removeToast } from '../../features/ui/toastSlice';

export const GlobalToasts = () => {
  const toasts = useSelector((state) => state.toast.items);
  const dispatch = useDispatch();

  const handleRemove = (id) => {
    dispatch(removeToast(id));
  };

  return <ToastContainer toasts={toasts} removeToast={handleRemove} />;
};