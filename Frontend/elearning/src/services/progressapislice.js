import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getProgressGateway } from '../api/clients/progress-gateway';

/**
 * @typedef {import('../models').GatewayContractsProgressModuleCompleteRequest} CompleteModuleReq
 * @typedef {import('../models').ProgressGatewayControllerProgressRecordDto} ProgressRecord
 */

const progressApi = getProgressGateway();

export const progressApiSlice = createApi({
  reducerPath: 'progressApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['Progress'],
  endpoints: (builder) => ({
    
    // FETCH USER PROGRESS
    getUserProgress: builder.query({
      async queryFn() {
        try {
          const res = await progressApi.postApiProgressUser();
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Progress'],
    }),

    // MARK MODULE COMPLETE
    completeModule: builder.mutation({
      /** @param {CompleteModuleReq} arg */
      async queryFn(arg) {
        try {
          const res = await progressApi.postApiProgressCompleteModule(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Progress'],
    }),
  }),
});

export const {
  useGetUserProgressQuery,
  useCompleteModuleMutation,
} = progressApiSlice;