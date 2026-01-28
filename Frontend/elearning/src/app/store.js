import { configureStore } from '@reduxjs/toolkit';

import loadingReducer from '../features/ui/loadingSlice';
import toastReducer from '../features/ui/toastSlice';
import themeReducer from '../features/ui/themeSlice';
import authReducer from '../features/auth/AuthSlice';
import courseReducer from '../features/course/CourseSlice';
import userReducer from '../features/users/UserSlice';
import assessmentReducer from '../features/assessment/AssessmentSlice';
import notificationReducer from '../features/notifications/NotificationSlice';
import progressReducer from '../features/progress/ProgressSlice';

import { authApiSlice } from '../services/authapislice';
import { courseApiSlice } from '../services/CourseApiSlice';
import { usersApiSlice } from '../services/usersapislice';
import { assessmentApiSlice } from '../services/assessmentapislice';
import { notificationApiSlice } from '../services/notificationapislice';
import { progressApiSlice } from '../services/progressapislice';

export const store = configureStore({
  reducer: {
    loading: loadingReducer,
    toast : toastReducer,
    theme : themeReducer,
    auth: authReducer,
    course: courseReducer,
    user : userReducer,
    assessment : assessmentReducer,
    notification : notificationReducer,
    progress : progressReducer,

    [authApiSlice.reducerPath]: authApiSlice.reducer,
    [courseApiSlice.reducerPath]: courseApiSlice.reducer,
    [usersApiSlice.reducerPath]: usersApiSlice.reducer,
    [assessmentApiSlice.reducerPath]: assessmentApiSlice.reducer,
    [notificationApiSlice.reducerPath]: notificationApiSlice.reducer,
    [progressApiSlice.reducerPath]: progressApiSlice.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      authApiSlice.middleware,
      courseApiSlice.middleware,
      usersApiSlice.middleware,
      assessmentApiSlice.middleware,
      notificationApiSlice.middleware,
      progressApiSlice.middleware,
    ),
});
