import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getGatewayUsers } from '../api/clients/gateway-users';

/**
 * @typedef {import('../models').GatewayContractsUsersUserIdRequest} UserIdReq
 * @typedef {import('../models').GatewayContractsUsersCompleteProfileRequest} CompleteProfileReq
 * @typedef {import('../models').GatewayContractsUsersUpdateUserRoleRequest} UpdateRoleReq
 */

const usersApi = getGatewayUsers();

export const usersApiSlice = createApi({
  reducerPath: 'usersApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['User', 'Role'],
  endpoints: (builder) => ({
    
    // --- QUERIES ---
    getAllUsers: builder.query({
      async queryFn() {
        try {
          const res = await usersApi.postApiUsersAll();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['User'],
    }),

    getUserById: builder.query({
      /** @param {UserIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await usersApi.postApiUsersById(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'User', id: arg.userId }],
    }),

    getAllRoles: builder.query({
      async queryFn() {
        try {
          const res = await usersApi.postApiUsersRolesAll();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Role'],
    }),

    getUserRoles: builder.query({
      /** @param {UserIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await usersApi.postApiUsersRolesUser(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Role', id: arg.userId }],
    }),

    // --- MUTATIONS ---
    completeProfile: builder.mutation({
      /** @param {CompleteProfileReq} arg */
      async queryFn(arg) {
        try {
          const res = await usersApi.postApiUsersCompleteProfile(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: (result, error, arg) => ['User', { type: 'User', id: arg.userId }],
    }),

    updateUserRole: builder.mutation({
      async queryFn(arg) {
        try {
          const res = await usersApi.postApiUsersRolesUpdate(arg);
          return { data: res.data };
        } catch (error) {
          return { 
            error: {
              status: error.response?.status || 500,
              data: error.response?.data || { message: error.message }
            } 
          };
        }
      },
      invalidatesTags: (result, error, arg) => [{ type: 'Role', id: arg.userId }, 'User'],
    }),

    deleteUser: builder.mutation({
      /** @param {UserIdReq} arg */
      async queryFn(arg) {
        try {
          const res = await usersApi.postApiUsersDelete(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['User'],
    }),
  }),
});

export const {
  useGetAllUsersQuery,
  useGetUserByIdQuery,
  useGetAllRolesQuery,
  useGetUserRolesQuery,
  useCompleteProfileMutation,
  useUpdateUserRoleMutation,
  useDeleteUserMutation,
} = usersApiSlice;