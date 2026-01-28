import { createSlice } from '@reduxjs/toolkit';
import { progressApiSlice } from '../../services/progressapislice';

const initialState = {
  records: [],
  overallCompletion: 0,
};

const progressSlice = createSlice({
  name: 'progress',
  initialState,
  reducers: {
    resetProgress: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        progressApiSlice.endpoints.getUserProgress.matchFulfilled,
        (state, action) => {
          state.records = action.payload;
          
          const total = action.payload.length;
          const completed = action.payload.filter(r => r.isCompleted).length;
          state.overallCompletion = total > 0 ? (completed / total) * 100 : 0;
        }
      );
  },
});

export const { resetProgress } = progressSlice.actions;
export default progressSlice.reducer;