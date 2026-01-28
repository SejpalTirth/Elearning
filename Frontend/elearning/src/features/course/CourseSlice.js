import { createSlice } from '@reduxjs/toolkit';
import { courseApiSlice } from '../../services/CourseApiSlice';

const initialState = {
  courses: [],          
  selectedCourse: null,
  enrolledCourses: [],
  loading: false,
};

const courseSlice = createSlice({
  name: 'course',
  initialState,
  reducers: {
    resetCourseState: () => initialState,
    
    setSelectedCourse: (state, action) => {
      state.selectedCourse = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        courseApiSlice.endpoints.getAllCourses.matchFulfilled,
        (state, action) => {
          state.courses = action.payload;
        }
      )
      .addMatcher(
        courseApiSlice.endpoints.getCourseById.matchFulfilled,
        (state, action) => {
          state.selectedCourse = action.payload;
        }
      )
      .addMatcher(
        courseApiSlice.endpoints.getEnrolledCourses.matchFulfilled,
        (state, action) => {
          state.enrolledCourses = action.payload;
        }
      )
      .addMatcher(
        courseApiSlice.endpoints.deleteCourse.matchFulfilled,
        (state, action) => {
          const deletedId = action.meta.arg.courseId;
          state.courses = state.courses.filter(c => c.id !== deletedId);
          if (state.selectedCourse?.id === deletedId) {
            state.selectedCourse = null;
          }
        }
      );
  },
});

export const { resetCourseState, setSelectedCourse } = courseSlice.actions;
export default courseSlice.reducer;