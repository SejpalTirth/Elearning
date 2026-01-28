import { createSlice } from '@reduxjs/toolkit';
import { usersApiSlice } from '../../services/usersapislice';

const initialState = {
  allUsers: [],
  activeUser: null,
  userRoles: [],
  availableRoles: [],
};

const userSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    clearActiveUser: (state) => {
      state.activeUser = null;
      state.userRoles = [];
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(
        usersApiSlice.endpoints.getAllUsers.matchFulfilled,
        (state, action) => {
          state.allUsers = action.payload;
        }
      )
      .addMatcher(
        usersApiSlice.endpoints.getUserById.matchFulfilled,
        (state, action) => {
          state.activeUser = action.payload;
        }
      )
      .addMatcher(
        usersApiSlice.endpoints.getAllRoles.matchFulfilled,
        (state, action) => {
          state.availableRoles = action.payload;
        }
      );
  },
});

export const { clearActiveUser } = userSlice.actions;
export default userSlice.reducer;