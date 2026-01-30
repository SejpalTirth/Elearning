import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  user: null,
  status: 'idle',
  authChecked: false,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    resetAuth: () => ({
      ...initialState,
      status: 'unauthenticated',
      authChecked: true,
    }),

    startAuthCheck: (state) => {
      if (state.status !== 'idle') return;
      state.status = 'checking';
      state.authChecked = false;
    },

    authSuccess: (state, action) => {
      state.user = action.payload;
      state.status = 'authenticated';
      state.authChecked = true;
    },

    authFailure: (state) => {
      state.user = null;
      state.status = 'unauthenticated';
      state.authChecked = true;
    },
  },
});

export const {
  resetAuth,
  startAuthCheck,
  authSuccess,
  authFailure,
} = authSlice.actions;

export default authSlice.reducer;
