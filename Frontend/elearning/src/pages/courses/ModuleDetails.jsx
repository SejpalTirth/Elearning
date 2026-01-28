import { useParams, useNavigate } from 'react-router-dom';
import {
  useGetModuleByIdQuery,
  useGetEnrolledCoursesQuery,
} from '../../services/CourseApiSlice';
import { useGetUserProgressQuery } from '../../services/progressApiSlice';
import { CheckCircle, ArrowLeft, RotateCcw } from 'lucide-react';

const ModuleDetails = () => {
  const { courseId, moduleId } = useParams();
  const navigate = useNavigate();

  const numericCourseId = Number(courseId);
  const numericModuleId = Number(moduleId);

  const {
    data: module,
    isLoading: moduleLoading,
  } = useGetModuleByIdQuery({ moduleId: numericModuleId });

  const {
    data: enrolledCourses = [],
    isLoading: enrollmentLoading,
  } = useGetEnrolledCoursesQuery(undefined, {
    refetchOnMountOrArgChange: true,
  });

  const { 
    data: progressData = [], 
    isLoading: progressLoading 
  } = useGetUserProgressQuery(undefined, {
    refetchOnMountOrArgChange: true,
  });

  const isEnrolled =
    !enrollmentLoading &&
    enrolledCourses.some((c) => Number(c.id) === numericCourseId);

  const isPassed = progressData.some(
    (p) => Number(p.moduleId) === numericModuleId && p.isCompleted === true
  );

  if (moduleLoading || progressLoading || !module) {
    return (
      <div className="flex justify-center items-center h-64 text-gray-600 dark:text-gray-300">
        <div className="animate-pulse">Loading module...</div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-6 py-16">
      
      {/* NAVIGATION BAR */}
      <div className="max-w-3xl mx-auto mb-8">
        <button 
          onClick={() => navigate(`/courses/${numericCourseId}`)}
          className="flex items-center gap-2 text-slate-500 hover:text-indigo-600 font-medium transition-colors"
        >
          <ArrowLeft className="w-4 h-4" /> Back to Curriculum
        </button>
      </div>

      {/* TITLE */}
      <div className="text-center mb-12">
        <h1 className="text-4xl font-extrabold text-slate-900 dark:text-gray-100 mb-3">
          {module.title}
        </h1>
        <div className="flex justify-center items-center gap-2">
           <span className="h-px w-8 bg-slate-300"></span>
           <p className="text-xs font-bold uppercase tracking-widest text-indigo-500">
             Lesson Content
           </p>
           <span className="h-px w-8 bg-slate-300"></span>
        </div>
      </div>

      {/* CONTENT CARD */}
      <div
        className="
          mx-auto max-w-3xl
          p-10 rounded-3xl
          bg-white border border-gray-200
          dark:bg-slate-900 dark:border-slate-800
          text-gray-800 dark:text-gray-200
          text-lg leading-relaxed whitespace-pre-line
          shadow-xl shadow-slate-200/50 dark:shadow-none
          opacity-0 translate-y-6
          animate-[fadeUp_0.6s_ease-out_forwards]
        "
      >
        {module.content}
      </div>

      {/* DIVIDER */}
      <div className="max-w-3xl mx-auto my-12 border-t border-gray-100 dark:border-white/5" />

      {/* QUIZ ACTION SECTION */}
      <div
        className="
          text-center
          opacity-0 translate-y-6
          animate-[fadeUp_0.6s_ease-out_forwards]
          [animation-delay:150ms]
        "
      >
        {!isEnrolled ? (
          <p className="text-sm text-gray-500 dark:text-gray-400 italic bg-gray-50 dark:bg-white/5 inline-block px-6 py-2 rounded-full">
            🔒 Enroll in the course to attempt the quiz.
          </p>
        ) : isPassed ? (
          /* SHOW THIS IF THE MODULE IS ALREADY PASSED */
          <div className="flex flex-col items-center gap-4">
            <div className="flex items-center gap-2 px-6 py-3 bg-emerald-100 dark:bg-emerald-500/10 text-emerald-700 dark:text-emerald-400 rounded-2xl border border-emerald-200 dark:border-emerald-500/20 shadow-sm">
              <CheckCircle className="w-5 h-5" />
              <span className="font-bold">You've passed this module!</span>
            </div>
            
            <div className="flex gap-4 mt-2">
              <button
                onClick={() => navigate(`/courses/${numericCourseId}`)}
                className="text-sm font-bold text-slate-500 hover:text-slate-700 underline transition"
              >
                Go to next lesson
              </button>
              
              {/* Optional: Allow them to retake if they want to improve/review */}
              <button
                onClick={() => navigate(`/courses/${numericCourseId}/modules/${numericModuleId}/quiz`)}
                className="flex items-center gap-1 text-xs text-indigo-500 hover:text-indigo-600 font-semibold transition"
              >
                <RotateCcw className="w-3 h-3" /> Retake Quiz
              </button>
            </div>
          </div>
        ) : (
          /* SHOW THIS IF ENROLLED BUT NOT PASSED */
          <button
            onClick={() =>
              navigate(`/courses/${numericCourseId}/modules/${numericModuleId}/quiz`)
            }
            className="
              inline-flex items-center gap-3
              px-10 py-4 rounded-2xl
              text-base font-black
              bg-indigo-600 text-white
              hover:bg-indigo-700 hover:scale-105
              shadow-lg shadow-indigo-500/25
              transition-all duration-300
            "
          >
            📝 Start Module Quiz
          </button>
        )}
      </div>
    </div>
  );
};

export default ModuleDetails;