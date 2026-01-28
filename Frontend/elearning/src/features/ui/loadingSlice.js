import { createSlice } from '@reduxjs/toolkit';

const loadingSlice = createSlice({
  name: 'loading',
  initialState: {
    activeRequests: 0,
  },
  reducers: {
    showLoader: (state) => {
      state.activeRequests += 1;
    },
    hideLoader: (state) => {
      state.activeRequests = Math.max(0, state.activeRequests - 1);
    },
  },
});

export const { showLoader, hideLoader } = loadingSlice.actions;
export default loadingSlice.reducer;

// Selector to use in your components
export const selectIsLoading = (state) => state.loading.activeRequests > 0;