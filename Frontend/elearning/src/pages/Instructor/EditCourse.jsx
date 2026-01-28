import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import {
  useGetCourseByIdQuery,
  useUpdateCourseMutation,
  useGetCategoriesQuery,
} from '../../services/CourseApiSlice';
import { showToast } from '../../features/ui/toastSlice'; // Fixed casing to match your project
import CategorySelect from './components/CategorySelect';
import ModuleEditor from './components/ModuleEditor';
import { ArrowRight } from 'lucide-react';

const EditCourse = () => {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const { data: categories = [] } = useGetCategoriesQuery();
  const { data: course, isLoading: loadingCourse } =
    useGetCourseByIdQuery({ courseId: courseId });

  const [updateCourse, { isLoading }] = useUpdateCourseMutation();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [modules, setModules] = useState([]);
  const [errors, setErrors] = useState({});

  /* ---------- PREFILL ---------- */
  useEffect(() => {
    if (!course) return;

    // eslint-disable-next-line react-hooks/set-state-in-effect
    setTitle(course.title ?? '');
    setDescription(course.description ?? '');
    setCategoryId(String(course.categoryId ?? ''));
    // Keep the IDs from the backend so the update knows which modules are which
    setModules(
      (course.modules ?? []).map((m) => ({
        id: m.id, 
        title: m.title,
        content: m.content,
      }))
    );
  }, [course]);

  /* ---------- VALIDATION ---------- */
  const validate = () => {
    const e = {};
    if (!title.trim()) e.title = 'Title required';
    if (!categoryId) e.categoryId = 'Category required';
    if (modules.length === 0) e.modules = 'Add at least one module';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  /* ---------- UPDATE ---------- */
  const handleUpdate = async () => {
    if (!validate()) return;

    try {
      // MATCHING SWAGGER: { courseId: int, course: { ... } }
      await updateCourse({
        courseId: Number(courseId),
        course: {
          title,
          description,
          categoryId: Number(categoryId),
          modules: modules.map((m) => ({
            id: m.id ?? 0, // Backend expects an ID (0 for new modules)
            title: m.title,
            content: m.content,
          })),
        },
      }).unwrap();

      dispatch(showToast({ message: 'Course updated successfully', type: 'success' }));
      navigate('/instructor/pending'); // Redirecting to pending or dashboard
    } catch (err) {
      console.error("Update Error:", err);
      dispatch(showToast({ message: 'Error updating course', type: 'error' }));
    }
  };

  if (loadingCourse) {
    return (
      <div className="flex justify-center items-center h-64 text-slate-500">
        Loading course…
      </div>
    );
  }

  return (
    <div className="w-full min-h-screen transition-colors duration-300">
      {/* HEADER */}
      <div className="max-w-6xl mx-auto px-6 pt-10 pb-6 flex justify-between items-center border-b border-slate-200 dark:border-white/5">
        <h1 className="text-xl font-semibold text-slate-900 dark:text-white tracking-tight">
          Edit Course
        </h1>
        <button
          onClick={handleUpdate}
          disabled={isLoading}
          className="flex items-center gap-2 px-5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium transition-all active:scale-95 disabled:opacity-50 shadow-sm"
        >
          {isLoading ? 'Saving...' : 'Save Changes'} <ArrowRight size={16} />
        </button>
      </div>

      {/* BODY */}
      <div className="max-w-6xl mx-auto px-6 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-16">

          <div className="lg:col-span-7 space-y-12">
            {/* TITLE */}
            <div className="space-y-2">
              <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">
                Course Title
              </label>
              <input
                value={title}
                onChange={(e) => {
                  setTitle(e.target.value);
                  if (errors.title) setErrors({ ...errors, title: null });
                }}
                className="w-full text-2xl font-medium bg-transparent border-none outline-none p-0 text-slate-900 dark:text-white"
              />
              <div className={`h-px w-full ${errors.title ? 'bg-red-500' : 'bg-slate-200 dark:bg-white/10'}`} />
              {errors.title && <p className="text-[10px] text-red-500">{errors.title}</p>}
            </div>

            {/* DESCRIPTION */}
            <div className="space-y-2">
              <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">
                Description
              </label>
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full bg-transparent border-none outline-none text-base p-0 resize-none text-slate-600 dark:text-slate-400"
              />
              <div className="h-px w-full bg-slate-200 dark:bg-white/10" />
            </div>

            {/* CATEGORY */}
            <div className="space-y-4">
               <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">Category</label>
               <CategorySelect
                categories={categories}
                value={categoryId}
                onChange={(val) => {
                  setCategoryId(val);
                  if (errors.categoryId) setErrors({ ...errors, categoryId: null });
                }}
              />
              {errors.categoryId && <p className="text-[10px] text-red-500">{errors.categoryId}</p>}
            </div>
          </div>

          {/* MODULES */}
          <div className="lg:col-span-5">
             <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest mb-6 block">Curriculum</label>
             <ModuleEditor
              modules={modules}
              onChange={(val) => {
                setModules(val);
                if (errors.modules) setErrors({ ...errors, modules: null });
              }}
            />
            {errors.modules && <p className="text-[10px] text-red-500 mt-2">{errors.modules}</p>}
          </div>

        </div>
      </div>
    </div>
  );
};

export default EditCourse;