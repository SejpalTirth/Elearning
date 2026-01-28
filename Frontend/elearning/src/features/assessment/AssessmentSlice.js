import { createSlice } from '@reduxjs/toolkit';
import { assessmentApiSlice } from '../../services/assessmentapislice';

const initialState = {
  activeQuiz: null,
  lastResult: null,
  quizStatus: null,
  isSubmitting: false,
};

const assessmentSlice = createSlice({
  name: 'assessment',
  initialState,
  reducers: {
    clearAssessmentSession: (state) => {
      state.activeQuiz = null;
      state.lastResult = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        assessmentApiSlice.endpoints.getQuizByModule.matchFulfilled,
        (state, action) => {
          state.activeQuiz = action.payload;
        }
      )
      .addMatcher(
        assessmentApiSlice.endpoints.submitQuiz.matchFulfilled,
        (state, action) => {
          state.lastResult = action.payload;
        }
      )
      .addMatcher(
        assessmentApiSlice.endpoints.getCourseQuizStatus.matchFulfilled,
        (state, action) => {
          state.quizStatus = action.payload;
        }
      );
  },
});

export const { clearAssessmentSession } = assessmentSlice.actions;
export default assessmentSlice.reducer;