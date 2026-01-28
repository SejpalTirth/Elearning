import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGetCourseQuizStatusQuery } from '../../../services/assessmentApiSlice';
import { usePublishCourseMutation } from '../../../services/CourseApiSlice';
import { showToast } from '../../../features/ui/toastSlice';
import { useDispatch } from 'react-redux';
import { ChevronDown, ChevronUp, AlertCircle, CheckCircle2 } from 'lucide-react';

const PendingCourseCard = ({ course }) => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [showModules, setShowModules] = useState(false);

  const {
    data: status,
    isLoading,
  } = useGetCourseQuizStatusQuery({
    courseId: course.id,
  });

  const [publishCourse, { isLoading: isPublishing }] = usePublishCourseMutation();

  if (isLoading) return <div className="p-6 border border-slate-200 dark:border-slate-800 rounded-2xl animate-pulse bg-slate-50 dark:bg-slate-900/50" />;

  const pendingModules = status?.modules?.filter((m) => !m.quizExists) ?? [];
  const allQuizzesDone = pendingModules.length === 0;

  const handlePublish = async () => {
    try {
      await publishCourse({ courseId: course.id }).unwrap();
      dispatch(showToast({ message: 'Course published successfully!', type: 'success' }));
    } catch {
      dispatch(showToast({ message: 'Failed to publish course', type: 'error' }));
    }
  };

  return (
    <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl overflow-hidden shadow-sm transition-all">
      <div className="p-6">
        <div className="flex justify-between items-start">
          <div>
            <span className="text-[10px] font-bold text-indigo-500 uppercase tracking-widest">
              {course.category?.name || 'Uncategorized'}
            </span>
            <h3 className="text-xl font-semibold text-slate-900 dark:text-white mt-1">
              {course.title}
            </h3>
          </div>
          
          <div className={`px-3 py-1 rounded-full text-[11px] font-bold uppercase tracking-tight flex items-center gap-1.5 ${
            allQuizzesDone ? 'bg-emerald-500/10 text-emerald-500' : 'bg-amber-500/10 text-amber-500'
          }`}>
            {allQuizzesDone ? <CheckCircle2 size={14} /> : <AlertCircle size={14} />}
            {allQuizzesDone ? 'Ready' : 'Incomplete'}
          </div>
        </div>

        <div className="mt-4 flex flex-wrap items-center gap-4">
          <button
            onClick={() => navigate(`/instructor/courses/${course.id}/edit`)}
            className="text-xs font-semibold text-slate-500 hover:text-indigo-600 transition-colors"
          >
            Edit Course Content
          </button>
          
          {!allQuizzesDone && (
            <button
              onClick={() => setShowModules(!showModules)}
              className="text-xs font-semibold text-indigo-600 hover:underline flex items-center gap-1"
            >
              {showModules ? 'Hide details' : `Show ${pendingModules.length} pending modules`}
              {showModules ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
            </button>
          )}
        </div>

        <div className="mt-6">
          {allQuizzesDone ? (
            <button
              disabled={isPublishing}
              onClick={handlePublish}
              className="w-full sm:w-auto bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-sm font-medium px-8 py-2.5 rounded-xl transition shadow-lg"
            >
              {isPublishing ? 'Publishing...' : 'Publish Course Now'}
            </button>
          ) : (
            <button
              onClick={() => navigate(`/assessment/add-quiz/${course.id}`)}
              className="w-full sm:w-auto bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-8 py-2.5 rounded-xl transition shadow-lg"
            >
              Add Remaining Quizzes
            </button>
          )}
        </div>
      </div>

      {/* PENDING MODULES LIST */}
      {showModules && !allQuizzesDone && (
        <div className="bg-slate-50 dark:bg-slate-950/50 border-t border-slate-200 dark:border-slate-800 p-6">
          <h4 className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest mb-4">
            Modules Missing Quizzes
          </h4>
          <ul className="space-y-3">
            {pendingModules.map((mod, idx) => (
              <li key={mod.moduleId || idx} className="flex items-start gap-3 text-sm text-slate-600 dark:text-slate-400 bg-white dark:bg-slate-900 p-3 rounded-lg border border-slate-100 dark:border-slate-800">
                <div className="h-5 w-5 rounded-full bg-amber-500/10 text-amber-500 flex items-center justify-center text-[10px] font-bold shrink-0">
                  {idx + 1}
                </div>
                {/* FIXED: Using mod.title based on your second API response */}
                <span>{mod.title}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default PendingCourseCard;