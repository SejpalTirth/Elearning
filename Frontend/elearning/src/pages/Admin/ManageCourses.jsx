import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  useGetAllCoursesQuery, 
  useDeleteCourseMutation,
  useRestoreCourseMutation
} from '../../services/CourseApiSlice';
import { showToast } from '../../features/ui/toastSlice';
import { useDispatch } from 'react-redux';
import { 
  BookOpen, 
  Trash2, 
  Eye, 
  RefreshCcw, 
  Search, 
  Loader2, 
  AlertCircle,
  Archive
} from 'lucide-react';

const ManageCourses = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [searchTerm, setSearchTerm] = useState('');

  const { data: courses = [], isLoading, isFetching } = useGetAllCoursesQuery();
  const [deleteCourse, { isLoading: isDeleting }] = useDeleteCourseMutation();
  const [restoreCourse, { isLoading: isRestoring }] = useRestoreCourseMutation();

  const filteredCourses = courses.filter(c => 
    c.title.toLowerCase().includes(searchTerm.toLowerCase()) || 
    c.instructorName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAction = async (actionFn, courseId, successMsg) => {
    try {
      await actionFn({ courseId }).unwrap();
      dispatch(showToast({ message: successMsg, type: 'success' }));
    } catch (err) {
      const errorMsg = err?.data?.message || "Operation failed";
      dispatch(showToast({ message: errorMsg, type: 'error' }));
    }
  };

  const confirmDelete = (id) => {
    if (window.confirm('Are you sure you want to archive this course? It will be hidden from students.')) {
      handleAction(deleteCourse, id, 'Course archived successfully');
    }
  };

  const confirmRestore = (id) => {
    handleAction(restoreCourse, id, 'Course restored to active status');
  };

  return (
    <div className="w-full max-w-7xl mx-auto py-10 px-6 min-h-screen transition-colors duration-300">
      {/* HEADER SECTION */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-10">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">Manage Courses</h1>
          <p className="text-slate-500 dark:text-slate-400 mt-1 font-medium">Archive, restore, or review platform curriculum.</p>
        </div>

        <div className="relative group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 group-focus-within:text-emerald-500 transition-colors" size={20} />
          <input 
            type="text" 
            placeholder="Search catalog..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full md:w-96 pl-12 pr-6 py-3 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-white focus:ring-4 focus:ring-emerald-500/10 outline-none transition-all shadow-sm"
          />
        </div>
      </div>

      {/* DATA STATE */}
      {(isLoading || isFetching) && !courses.length ? (
        <div className="flex flex-col items-center justify-center py-32 text-slate-400">
          <Loader2 className="animate-spin mb-4 text-emerald-500" size={48} />
          <p className="font-bold uppercase tracking-[0.2em] text-[10px]">Syncing Course Registry...</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-5">
          {filteredCourses.map((course) => (
            <div 
              key={course.id} 
              className={`group relative overflow-hidden bg-white dark:bg-slate-900 border rounded-[2rem] p-6 transition-all duration-300 ${
                course.isDeleted 
                  ? 'border-slate-100 dark:border-slate-800/50 opacity-70 grayscale-[0.5]' 
                  : 'border-slate-200 dark:border-slate-800 hover:border-emerald-500/50 dark:hover:border-emerald-500/30 hover:shadow-2xl hover:shadow-emerald-500/5'
              }`}
            >
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 relative z-10">
                <div className="flex items-start gap-6">
                  {/* ICON BLOCK */}
                  <div className={`p-5 rounded-[1.5rem] border transition-all duration-500 shadow-sm ${
                    course.isDeleted 
                      ? 'bg-slate-50 dark:bg-slate-800/50 text-slate-400 border-slate-100 dark:border-slate-700' 
                      : 'bg-emerald-50 dark:bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-100 dark:border-emerald-500/20 group-hover:scale-110'
                  }`}>
                    {course.isDeleted ? <Archive size={28} /> : <BookOpen size={28} />}
                  </div>
                  
                  <div className="space-y-1">
                    <div className="flex flex-wrap items-center gap-3">
                      <h3 className={`text-xl font-bold tracking-tight transition-all ${
                        course.isDeleted ? 'text-slate-400 line-through' : 'text-slate-900 dark:text-white'
                      }`}>
                        {course.title}
                      </h3>
                      {course.isDeleted && (
                        <span className="bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 text-[10px] font-black uppercase px-2.5 py-1 rounded-lg tracking-widest border border-slate-200 dark:border-slate-700">
                          Archived
                        </span>
                      )}
                    </div>
                    
                    <p className="text-sm font-medium text-slate-500 dark:text-slate-400 flex items-center gap-2">
                      Instructor: <span className="text-slate-700 dark:text-slate-300">{course.instructorName || 'Unknown'}</span>
                    </p>
                    
                    <div className="flex items-center gap-4 pt-3">
                      <div className="flex items-center gap-1.5 px-3 py-1 bg-slate-50 dark:bg-slate-800/50 rounded-full border border-slate-100 dark:border-slate-700">
                         <span className="text-[10px] font-black text-slate-400 uppercase tracking-tighter">ID: #{course.id}</span>
                      </div>
                      <div className="flex items-center gap-1.5 px-3 py-1 bg-slate-50 dark:bg-slate-800/50 rounded-full border border-slate-100 dark:border-slate-700">
                         <span className="text-[10px] font-black text-slate-400 uppercase tracking-tighter">{course.modulesCount || 0} Modules</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* ACTIONS */}
                <div className="flex items-center gap-4 self-end md:self-center">
                  <button 
                    onClick={() => navigate(`/courses/${course.id}`)}
                    disabled={course.isDeleted}
                    title="View Public Page"
                    className="p-3.5 bg-white dark:bg-slate-800 text-slate-400 dark:text-slate-500 hover:text-indigo-600 dark:hover:text-white border border-slate-200 dark:border-slate-700 rounded-2xl transition-all hover:shadow-lg disabled:opacity-20"
                  >
                    <Eye size={22} />
                  </button>

                  <div className="w-px h-10 bg-slate-200 dark:bg-slate-800 hidden md:block mx-1"></div>

                  {course.isDeleted ? (
                    <button 
                      onClick={() => confirmRestore(course.id)}
                      disabled={isRestoring}
                      className="flex items-center gap-2.5 px-6 py-3.5 bg-emerald-500 hover:bg-emerald-600 text-white rounded-2xl shadow-lg shadow-emerald-500/20 transition-all text-xs font-black uppercase tracking-widest disabled:opacity-50"
                    >
                      <RefreshCcw size={18} className={isRestoring ? 'animate-spin' : ''} />
                      Restore Course
                    </button>
                  ) : (
                    <button 
                      onClick={() => confirmDelete(course.id)}
                      disabled={isDeleting}
                      title="Archive Course"
                      className="p-3.5 bg-white dark:bg-slate-800 text-slate-400 hover:text-red-500 dark:hover:text-red-400 border border-slate-200 dark:border-slate-700 rounded-2xl transition-all hover:shadow-lg hover:border-red-200 dark:hover:border-red-500/30"
                    >
                      <Trash2 size={22} />
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* EMPTY STATE */}
      {!isLoading && filteredCourses.length === 0 && (
        <div className="text-center py-32 bg-slate-50 dark:bg-slate-900/50 border border-dashed border-slate-200 dark:border-slate-800 rounded-[3rem]">
          <div className="inline-flex p-6 rounded-full bg-slate-100 dark:bg-slate-800 mb-6">
            <AlertCircle className="text-slate-400" size={48} />
          </div>
          <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-1">No matches found</h3>
          <p className="text-slate-500 dark:text-slate-400 text-sm italic font-medium">Try adjusting your search terms or filters.</p>
        </div>
      )}
    </div>
  );
};

export default ManageCourses;