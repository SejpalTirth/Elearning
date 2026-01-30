import { authApiSlice } from '../../services/authapislice';
import { authSuccess, authFailure } from './AuthSlice';

export const loginAndBootstrapAuth = (credentials) => async (dispatch) => {
  try {
    await dispatch(
      authApiSlice.endpoints.login.initiate(credentials)
    ).unwrap();

    const user = await dispatch(
      authApiSlice.endpoints.getMe.initiate(undefined, {
        subscribe: false,
        forceRefetch: false,
      })
    ).unwrap();

    dispatch(authSuccess(user));
    return true;
  } catch (err) {
    dispatch(authFailure());
    throw err;
  }
};