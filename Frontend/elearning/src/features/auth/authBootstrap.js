import { authApiSlice } from '../../services/authapislice';
import { startAuthCheck, authSuccess, authFailure } from './AuthSlice';

export const bootstrapAuth = () => async (dispatch, getState) => {
  const { status, authChecked } = getState().auth;

  if (authChecked) return;
  if (status === 'authenticated') return;

  dispatch(startAuthCheck());

  try {
    const result = await dispatch(
      authApiSlice.endpoints.getMe.initiate(undefined, {
        subscribe: false,
        forceRefetch: false,
      })
    ).unwrap();

    dispatch(authSuccess(result));
  } catch {
    dispatch(authFailure());
  }
};
