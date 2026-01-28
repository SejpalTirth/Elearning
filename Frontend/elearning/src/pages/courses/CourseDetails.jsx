import { useParams, useNavigate } from 'react-router-dom';
import {
  useGetCourseByIdQuery,
  useGetCourseModulesQuery,
  useGetCategoriesQuery,
  useGetEnrolledCoursesQuery,
  useEnrollInCourseMutation  
} from '../../services/CourseApiSlice';
import { useGetUserProgressQuery } from '../../services/progressApiSlice';
import { showLoader, hideLoader } from '../../features/ui/loadingSlice';
import { useDispatch } from 'react-redux';
import { showToast } from '../../features/ui/toastSlice';
import { CheckCircle2, Lock, PlayCircle, BarChart3, ChevronRight } from 'lucide-react';

const CourseDetails = () => {
  const { id } = useParams();
  const courseId = Number(id);
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const { data: course, isLoading: courseLoading } = useGetCourseByIdQuery({ courseId });
  const { data: modules = [] } = useGetCourseModulesQuery({ courseId });
  const { data: categories = [] } = useGetCategoriesQuery();
  const { data: enrolledCourses = [] } = useGetEnrolledCoursesQuery();
  
  // Fetch the progress array you just shared
  const { data: progressData = [], isLoading: progressLoading } = useGetUserProgressQuery(undefined, {
    refetchOnMountOrArgChange: true,
  });

  const [enrollInCourse, { isLoading: enrolling }] = useEnrollInCourseMutation();

  const isEnrolled = enrolledCourses.some((c) => Number(c.id) === courseId);
  
  const completedModulesForThisCourse = progressData.filter(
    (p) => Number(p.courseId) === courseId && p.isCompleted === true
  );

  const completedModuleIds = completedModulesForThisCourse.map(p => p.moduleId);

  const progressPercentage = modules.length > 0 
    ? Math.round((completedModuleIds.length / modules.length) * 100) 
    : 0;

  const categoryMap = Object.fromEntries(categories.map((c) => [c.id, c.name]));

  const handleEnrollOrContinue = async () => {
    if (isEnrolled) {
      navigate(`/courses/${courseId}/modules`);
      return;
    }
    try {
      dispatch(showLoader());
      await enrollInCourse({ courseId }).unwrap();
      dispatch(hideLoader());
      dispatch(showToast({ message: "Enrolled successfully!", type: "success" }));
    } catch {
      dispatch(hideLoader());
      dispatch(showToast({ message: "Enrolment failed", type: 'error' }));
    }
  };

  if (courseLoading || progressLoading) {
    return (
      <div className="flex flex-col justify-center items-center h-96">
        <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mb-4"></div>
        <p className="text-slate-500 font-medium">Loading Course...</p>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-6 lg:px-10 py-12">
      <div className="grid grid-cols-1 lg:grid-cols-5 gap-12">

        {/* LEFT: INFO & PROGRESS */}
        <div className="lg:col-span-2 space-y-8">
          <div>
            <h1 className="text-4xl font-black text-slate-900 dark:text-white mb-4 leading-tight tracking-tight">
              {course?.title}
            </h1>
            <p className="text-slate-600 dark:text-slate-400 leading-relaxed text-lg italic">
              {course?.description}
            </p>
          </div>

          <div className="p-6 bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-5">
            <div className="flex items-center gap-3 text-slate-700 dark:text-slate-300 font-semibold text-sm">
              <span className="p-2 bg-indigo-50 dark:bg-indigo-900/30 rounded-lg">👨‍🏫</span>
              {course?.instructorName || "Instructor"}
            </div>
            <div className="flex items-center gap-3 text-slate-700 dark:text-slate-300 font-semibold text-sm">
              <span className="p-2 bg-indigo-50 dark:bg-indigo-900/30 rounded-lg">📂</span>
              {categoryMap[course?.categoryId] || "Uncategorized"}
            </div>

            {/* REAL-TIME PROGRESS BAR */}
            {isEnrolled && (
              <div className="pt-5 border-t border-slate-100 dark:border-slate-800">
                <div className="flex justify-between items-center mb-3">
                  <span className="text-[10px] font-black uppercase tracking-widest text-indigo-600 flex items-center gap-2">
                    <BarChart3 className="w-3 h-3" /> Learning Progress
                  </span>
                  <span className="text-2xl font-black text-slate-900 dark:text-white">{progressPercentage}%</span>
                </div>
                <div className="w-full bg-slate-100 dark:bg-slate-800 h-2.5 rounded-full overflow-hidden">
                  <div 
                    className="bg-indigo-600 h-full transition-all duration-1000 ease-in-out shadow-[0_0_12px_rgba(79,70,229,0.4)]"
                    style={{ width: `${progressPercentage}%` }}
                  />
                </div>
                <p className="text-[10px] text-slate-400 mt-2 font-medium">
                  {completedModuleIds.length} of {modules.length} modules completed
                </p>
              </div>
            )}
          </div>

          <button
            onClick={handleEnrollOrContinue}
            disabled={enrolling}
            className={`w-full py-4 rounded-2xl text-base font-bold transition-all transform active:scale-[0.97] shadow-lg ${
              enrolling ? 'bg-slate-300 cursor-not-allowed' : 'bg-indigo-600 text-white hover:bg-indigo-700'
            }`}
          >
            {enrolling ? 'Enrolling...' : isEnrolled ? 'Continue Learning' : 'Enroll in Course'}
          </button>
        </div>

        {/* RIGHT: CURRICULUM */}
        <div className="lg:col-span-3">
          <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-8 border-b pb-4 border-slate-100 dark:border-slate-800">
            Course Content
          </h2>

          <div className="space-y-4">
            {modules.map((module, index) => {
              // Check if this module ID is in our list of completed IDs
              const isCompleted = completedModuleIds.some(mId => Number(mId) === Number(module.id));

              return (
                <div
                  key={module.id}
                  onClick={() => isEnrolled && navigate(`/courses/${courseId}/modules/${module.id}`)}
                  className={`
                    group p-5 rounded-2xl border transition-all duration-300
                    ${!isEnrolled 
                      ? 'bg-slate-50 border-slate-100 opacity-60 cursor-not-allowed' 
                      : isCompleted 
                        ? 'bg-emerald-50/40 border-emerald-200 dark:bg-emerald-500/5 dark:border-emerald-500/10 cursor-pointer hover:border-emerald-400' 
                        : 'bg-white border-slate-200 dark:bg-slate-900 dark:border-slate-800 cursor-pointer hover:border-indigo-500 hover:shadow-md'
                    }
                  `}
                >
                  <div className="flex justify-between items-center">
                    <div className="flex items-center gap-5">
                      <div className={`w-11 h-11 rounded-xl flex items-center justify-center transition-all ${
                        isCompleted ? 'bg-emerald-500 text-white' : 
                        !isEnrolled ? 'bg-slate-200 text-slate-500' : 
                        'bg-indigo-50 text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white'
                      }`}>
                        {isCompleted ? <CheckCircle2 className="w-5 h-5" /> : 
                         !isEnrolled ? <Lock className="w-4 h-4" /> : 
                         <PlayCircle className="w-5 h-5" />}
                      </div>

                      <div>
                        <h3 className={`font-bold text-base ${isCompleted ? 'text-emerald-900 dark:text-emerald-400' : 'text-slate-900 dark:text-white'}`}>
                          {index + 1}. {module.title}
                        </h3>
                        <p className="text-xs text-slate-400 mt-0.5 font-medium tracking-wide uppercase">
                          {isCompleted ? 'Completed' : 'Lesson'}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      {isCompleted ? (
                        <span className="text-[10px] font-black uppercase tracking-widest text-emerald-600 bg-emerald-100 px-3 py-1 rounded-full">Finished</span>
                      ) : isEnrolled ? (
                        <ChevronRight className="w-5 h-5 text-indigo-600 opacity-0 group-hover:opacity-100 transition-all -translate-x-2 group-hover:translate-x-0" />
                      ) : (
                         <Lock className="w-4 h-4 text-slate-300" />
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};

export default CourseDetails;