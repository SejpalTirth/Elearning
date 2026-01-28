import { createSlice } from '@reduxjs/toolkit';

const toastSlice = createSlice({
  name: 'toast',
  initialState: {
    items: [],
  },
  reducers: {
    addToast: (state, action) => {
      state.items.push(action.payload);
    },
    removeToast: (state, action) => {
      state.items = state.items.filter((t) => t.id !== action.payload);
    },
  },
});

export const { addToast, removeToast } = toastSlice.actions;

export const showToast = ({ message, type = 'info' }) => (dispatch) => {
  const id = Math.random().toString(36).substring(2, 9);
  
  dispatch(addToast({ id, message, type }));

  setTimeout(() => {
    dispatch(removeToast(id));
  }, 4000);
};

export default toastSlice.reducer;