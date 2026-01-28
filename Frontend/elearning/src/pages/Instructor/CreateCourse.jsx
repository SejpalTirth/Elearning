import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { useCreateCourseMutation, useGetCategoriesQuery } from '../../services/CourseApiSlice';
import { showToast } from '../../features/ui/toastSlice';
import CategorySelect from './components/CategorySelect';
import ModuleEditor from './components/ModuleEditor';
import { ArrowRight } from 'lucide-react';

const CreateCourse = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  
  const { user } = useSelector((state) => state.auth);
  console.log(user);

  const { data: categories = [] } = useGetCategoriesQuery();
  const [createCourse, { isLoading }] = useCreateCourseMutation();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [modules, setModules] = useState([]);
  const [errors, setErrors] = useState({});

  const validate = () => {
    const e = {};
    if (!title.trim()) e.title = 'Title required';
    if (!categoryId) e.categoryId = 'Category required';
    if (modules.length === 0) e.modules = 'Add at least one module';
    if (!user?.userId) e.auth = 'Instructor ID not found. Please re-login.';
    
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleCreate = async () => {
    if (!validate()) {
      if (errors.auth) dispatch(showToast({ message: errors.auth, type: 'error' }));
      return;
    }

    try {
      const cleanedModules = modules.map(({ title, content }) => ({
        title,
        content,
      }));

      const course = await createCourse({
        title,
        description,
        categoryId: Number(categoryId),
        instructorUserId: user.userId,
        modules: cleanedModules,
        isDraft: true,
      }).unwrap();

      dispatch(showToast({ message: 'Course created successfully', type: 'success' }));
      
      navigate(`/assessment/add-quiz/${course.id}`);
    } catch (err) {
      console.error("Course Creation Error:", err);
      dispatch(showToast({ 
        message: err?.data?.message || 'Error saving course. Check console for details.', 
        type: 'error' 
      }));
    }
  };

  return (
    <div className="w-full min-h-screen transition-colors duration-300">
      {/* MINIMAL HEADER */}
      <div className="max-w-6xl mx-auto px-6 pt-10 pb-6 flex justify-between items-center border-b border-slate-200 dark:border-white/5">
        <h1 className="text-xl font-semibold text-slate-900 dark:text-white tracking-tight">Create Course</h1>
        <button
          onClick={handleCreate}
          disabled={isLoading}
          className="flex items-center gap-2 px-5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-medium transition-all active:scale-95 disabled:opacity-50 shadow-sm"
        >
          {isLoading ? 'Saving...' : 'Save & Continue'} <ArrowRight size={16} />
        </button>
      </div>

      <div className="max-w-6xl mx-auto px-6 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-16">
          
          <div className="lg:col-span-7 space-y-12">
            {/* COURSE TITLE */}
            <div className="space-y-2">
              <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">Course Title</label>
              <input
                value={title}
                onChange={(e) => {
                  setTitle(e.target.value);
                  if (errors.title) setErrors({...errors, title: null});
                }}
                placeholder="Enter title..."
                className="w-full text-2xl font-medium bg-transparent border-none outline-none focus:ring-0 p-0 text-slate-900 dark:text-white placeholder:text-slate-300 dark:placeholder:text-slate-800"
              />
              <div className={`h-px w-full transition-colors ${errors.title ? 'bg-red-500' : 'bg-slate-200 dark:bg-white/10'}`} />
              {errors.title && <p className="text-[10px] text-red-500 mt-1">{errors.title}</p>}
            </div>

            {/* DESCRIPTION */}
            <div className="space-y-2">
              <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">Description</label>
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Course summary..."
                className="w-full bg-transparent border-none outline-none focus:ring-0 text-base p-0 resize-none text-slate-600 dark:text-slate-400 placeholder:text-slate-300 dark:placeholder:text-slate-800 leading-relaxed"
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
                  if (errors.categoryId) setErrors({...errors, categoryId: null});
                }}
              />
              {errors.categoryId && <p className="text-[10px] text-red-500">{errors.categoryId}</p>}
            </div>
          </div>

          {/* CURRICULUM */}
          <div className="lg:col-span-5">
            <div className="space-y-6">
              <label className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest">Curriculum Structure</label>
              <ModuleEditor
                modules={modules}
                onChange={(val) => {
                  setModules(val);
                  if (errors.modules) setErrors({...errors, modules: null});
                }}
              />
              {errors.modules && <p className="text-[10px] text-red-500 mt-2">{errors.modules}</p>}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
};

export default CreateCourse;