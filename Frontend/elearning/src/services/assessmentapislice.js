import { createApi, fakeBaseQuery } from '@reduxjs/toolkit/query/react';
import { getAssessmentGateway } from '../api/clients/assessment-gateway';

/**
 * @typedef {import('../models').GatewayContractsAssessmentGetQuizForModuleRequest} GetQuizReq
 * @typedef {import('../models').GatewayContractsAssessmentCreateQuiz} CreateQuizReq
 * @typedef {import('../models').GatewayContractsAssessmentAddQuestionRequest} AddQuestionReq
 * @typedef {import('../models').GatewayContractsAssessmentSubmitQuiz} SubmitQuizReq
 * @typedef {import('../models').GatewayContractsAssessmentResultRequest} ResultReq
 * @typedef {import('../models').GatewayContractsAssessmentCourseQuizStatusRequest} StatusReq
 * @typedef {import('../models').GatewayContractsAssessmentGetUnquizzedRequest} UnquizzedReq
 */

const assessmentApi = getAssessmentGateway();

export const assessmentApiSlice = createApi({
  reducerPath: 'assessmentApi',
  baseQuery: fakeBaseQuery(),
  tagTypes: ['Quiz', 'Result', 'Status'],
  endpoints: (builder) => ({
    
    getQuizByModule: builder.query({
      /** @param {GetQuizReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentQuizModule(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: (result, error, arg) => [{ type: 'Quiz', id: arg.moduleId }],
    }),

    getQuizResult: builder.query({
      /** @param {ResultReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentQuizResult(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Result'],
    }),

    getCourseQuizStatus: builder.query({
      /** @param {StatusReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentCourseQuizStatus(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Status'],
    }),

    getUnquizzedModules: builder.query({
      /** @param {UnquizzedReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentCourseUnquizzedModules(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      providesTags: ['Quiz'],
    }),

    // --- MUTATIONS ---

    createQuiz: builder.mutation({
      /** @param {CreateQuizReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentQuiz(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Quiz'],
    }),

    addQuestions: builder.mutation({
      /** @param {AddQuestionReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentQuizQuestions(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: (result, error, arg) => [{ type: 'Quiz', id: arg.quizId }],
    }),

    submitQuiz: builder.mutation({
      /** @param {SubmitQuizReq} arg */
      async queryFn(arg) {
        try {
          const res = await assessmentApi.postApiAssessmentQuizSubmit(arg);
          return { data: res.data };
        } catch (error) {
          return { error };
        }
      },
      invalidatesTags: ['Result', 'Status'],
    }),
  }),
});

export const {
  useGetQuizByModuleQuery,
  useGetQuizResultQuery,
  useGetCourseQuizStatusQuery,
  useGetUnquizzedModulesQuery,
  useCreateQuizMutation,
  useAddQuestionsMutation,
  useSubmitQuizMutation,
} = assessmentApiSlice;