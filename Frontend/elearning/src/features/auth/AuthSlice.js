import { createSlice } from '@reduxjs/toolkit';
import { authApiSlice } from '../../services/authapislice';

const initialState = {
  user: null,
  isAuthenticated: false,
  authChecked: false,
  allowMe: false,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    resetAuth: () => initialState,
    
    allowMe: (state) => {
      state.allowMe = true;
      state.authChecked = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        authApiSlice.endpoints.getMe.matchFulfilled,
        (state, action) => {
          state.user = action.payload;
          state.isAuthenticated = true;
          state.authChecked = true;
        }
      )
      .addMatcher(
        authApiSlice.endpoints.getMe.matchRejected,
        (state) => {
          state.user = null;
          state.isAuthenticated = false;
          state.authChecked = true;
        }
      );
  },
});

export const { resetAuth } = authSlice.actions;
export default authSlice.reducer;


export const runMeOnce = () => (dispatch, getState) => {
  const { authChecked } = getState().auth;

  if (authChecked) return;

  dispatch(authApiSlice.endpoints.getMe.initiate());
};
