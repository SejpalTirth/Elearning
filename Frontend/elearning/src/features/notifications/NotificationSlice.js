import { createSlice } from '@reduxjs/toolkit';
import { notificationApiSlice } from '../../services/notificationapislice';

const initialState = {
  userNotifications: [],
  lastTriggerStatus: null,
};

const notificationSlice = createSlice({
  name: 'notifications',
  initialState,
  reducers: {
    clearNotificationState: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        notificationApiSlice.endpoints.getUserNotifications.matchFulfilled,
        (state, action) => {
          state.userNotifications = action.payload;
        }
      )
      .addMatcher(
        notificationApiSlice.endpoints.triggerNotification.matchFulfilled,
        (state) => {
          state.lastTriggerStatus = 'success';
        }
      )
      .addMatcher(
        notificationApiSlice.endpoints.triggerNotification.matchRejected,
        (state) => {
          state.lastTriggerStatus = 'failed';
        }
      );
  },
});

export const { clearNotificationState } = notificationSlice.actions;
export default notificationSlice.reducer;