import { useNavigate } from 'react-router-dom';
import { Folder, PlayCircle, CheckCircle } from 'lucide-react';
import { useGetUserProgressQuery } from '../../services/progressApiSlice';
import { useGetEnrolledCoursesQuery } from '../../services/CourseApiSlice';

const CourseCard = ({ course, categoryName }) => {
  const navigate = useNavigate();

  const { data: enrolledCourses = [] } = useGetEnrolledCoursesQuery();
  
  const { data: userProgress = [] } = useGetUserProgressQuery();

  const isEnrolled = enrolledCourses.some(e => e.id === course.id);

  const completedModulesCount = userProgress.filter(
    (p) => p.courseId === course.id && p.isCompleted
  ).length;

  const totalModulesCount = course.modules?.length || 0;
  
  let progressPercentage = 0;
  if (totalModulesCount > 0) {
    progressPercentage = Math.min(
      Math.round((completedModulesCount / totalModulesCount) * 100), 
      100
    );
  }

  const isCompleted = isEnrolled && progressPercentage === 100;

  const getStatusIcon = () => {
    if (isCompleted) return <CheckCircle className="w-6 h-6 text-emerald-500" />;
    if (isEnrolled) return <PlayCircle className="w-6 h-6 text-indigo-500" />;
    return <Folder className="w-6 h-6 text-slate-400" />;
  };

  return (
    <div className="group relative rounded-2xl p-6 bg-white border border-slate-200 dark:bg-slate-900 dark:border-slate-800 transition-all hover:shadow-lg">
      
      <div className="absolute top-4 right-4">
        {getStatusIcon()}
      </div>

      {categoryName && (
        <span className="inline-block mb-3 px-2 py-1 rounded text-[10px] font-bold uppercase tracking-wider bg-indigo-100 text-indigo-700 dark:bg-indigo-500/10 dark:text-indigo-400">
          {categoryName}
        </span>
      )}

      <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-1 line-clamp-1">
        {course.title}
      </h3>

      <div className="text-xs text-slate-500 dark:text-slate-400 mb-4">
        by <span className="font-medium text-slate-700 dark:text-slate-300">{course.instructorName || "Tirth"}</span>
      </div>

      <div className="h-16 mb-4">
        {isEnrolled ? (
          <div className="space-y-2">
            <div className="flex justify-between text-xs font-semibold">
              <span className="text-slate-500 dark:text-slate-400">
                {isCompleted ? 'Finished' : 'Progress'}
              </span>
              <span className="text-indigo-600 dark:text-indigo-400">{progressPercentage}%</span>
            </div>
            <div className="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
              <div 
                className={`h-full transition-all duration-700 ${isCompleted ? 'bg-emerald-500' : 'bg-indigo-600'}`}
                style={{ width: `${progressPercentage}%` }}
              />
            </div>
          </div>
        ) : (
          <p className="text-sm text-slate-600 dark:text-slate-400 line-clamp-3">
            {course.description}
          </p>
        )}
      </div>

      <button
        onClick={() => navigate(`/courses/${course.id}`)}
        className={`
          w-full py-2.5 rounded-xl text-sm font-bold transition-all
          ${isEnrolled 
            ? 'bg-indigo-600 text-white hover:bg-indigo-700' 
            : 'bg-white text-slate-900 border border-slate-200 hover:bg-slate-50 dark:bg-white dark:text-slate-900'}
        `}
      >
        {isCompleted ? 'Review Course' : isEnrolled ? 'Continue Learning' : 'Enroll Now'}
      </button>
    </div>
  );
};

export default CourseCard;