import { useMemo, useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useCreateQuizMutation,
  useAddQuestionsMutation,
  useGetUnquizzedModulesQuery,
} from '../../services/assessmentApiSlice';
import { useGetCourseModulesQuery } from '../../services/CourseApiSlice';
import { useDispatch } from 'react-redux';
import { showToast } from '../../features/ui/toastSlice';
import { PlusCircle, CheckCircle, Info, ArrowLeft } from 'lucide-react';

const AddQuiz = () => {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const { data: modules = [] } = useGetCourseModulesQuery({ courseId });
  const { data: unquizzedModules = [], isLoading: loadingUnquizzed, isSuccess: fetchedUnquizzed } = useGetUnquizzedModulesQuery({ courseId });

  const [selectedModuleId, setSelectedModuleId] = useState('');
  const [quizTitle, setQuizTitle] = useState('');
  const [timeLimitMinutes, setTimeLimitMinutes] = useState(10);

  const [quizCreated, setQuizCreated] = useState(false);
  const [createdQuizId, setCreatedQuizId] = useState(null);
  const [questionCount, setQuestionCount] = useState(0);

  const [questionForm, setQuestionForm] = useState({
    text: '',
    marks: 1,
    options: ['', '', '', ''],
    correctAnswerIndex: 0,
  });

  useEffect(() => {
    if (fetchedUnquizzed && unquizzedModules.length === 0 && !quizCreated) {
      navigate('/instructor/pending');
    }
  }, [unquizzedModules, fetchedUnquizzed, navigate, quizCreated]);

  const availableModules = useMemo(
    () => modules.filter((m) => unquizzedModules.includes(m.id)),
    [modules, unquizzedModules]
  );

  const activeModuleName = useMemo(
    () => modules.find(m => m.id === Number(selectedModuleId))?.title || '',
    [modules, selectedModuleId]
  );

  const [createQuiz] = useCreateQuizMutation();
  const [addQuestion] = useAddQuestionsMutation();

  const handleCreateQuiz = async () => {
    if (!selectedModuleId || !quizTitle) {
      dispatch(showToast({ message: "Module and Title are required", type: 'error' }));
      return;
    }
    try {
      const res = await createQuiz({
        moduleId: Number(selectedModuleId),
        title: quizTitle,
        timeLimitMinutes,
      }).unwrap();
      setQuizCreated(true);
      setCreatedQuizId(res?.quizId ?? res?.id);
      dispatch(showToast({ message: "Quiz initialized", type: 'info' }));
    } catch {
      dispatch(showToast({ message: "Failed to create quiz", type: 'error' }));
    }
  };

  const handleAddQuestion = async () => {
    const { text, options, marks, correctAnswerIndex } = questionForm;
    if (!text || options.some(o => !o.trim())) {
      dispatch(showToast({ message: "Complete all fields", type: 'error' }));
      return;
    }
    try {
      await addQuestion({
        quizId: createdQuizId,
        question: { question: text, marks: Number(marks), options, correctAnswerIndex },
      }).unwrap();
      setQuestionCount(c => c + 1);
      setQuestionForm({ text: '', marks: 1, options: ['', '', '', ''], correctAnswerIndex: 0 });
      dispatch(showToast({ message: "Question saved", type: 'success' }));
    } catch {
      dispatch(showToast({ message: "Error saving question", type: 'error' }));
    }
  };

  const completeModule = () => {
    if (questionCount < 1) {
      dispatch(showToast({ message: "Add a question first", type: 'error' }));
      return;
    }
    setQuizCreated(false);
    setSelectedModuleId('');
    setQuizTitle('');
    setQuestionCount(0);
    dispatch(showToast({ message: "Module finalized!", type: 'success' }));
  };

  if (loadingUnquizzed) return null;

  return (
    <div className="max-w-4xl mx-auto py-10 px-6">
      <div className="text-center mb-8">
        <h2 className="text-3xl font-bold text-white tracking-tight">Add Quiz</h2>
        <p className="text-slate-500 text-sm mt-1">Course Assessment Builder</p>
      </div>

      {/* STEP 1: CONFIGURATION */}
      <div className={`bg-slate-900 border border-slate-800 rounded-2xl p-8 mb-6 transition-all duration-500 ${quizCreated ? 'opacity-40 grayscale pointer-events-none' : 'shadow-xl'}`}>
        <div className="flex items-center gap-2 mb-6 text-indigo-400">
          <Info size={16} />
          <h3 className="text-[10px] font-bold uppercase tracking-[0.2em]">Step 1: Quiz Details</h3>
        </div>

        <div className="max-w-md mx-auto space-y-4">
          <select
            value={selectedModuleId}
            onChange={(e) => {
              setSelectedModuleId(e.target.value);
              const mod = modules.find(m => m.id === Number(e.target.value));
              setQuizTitle(mod ? `${mod.title} Quiz` : '');
            }}
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white text-sm focus:ring-1 focus:ring-indigo-500 outline-none"
          >
            <option value="" disabled>Select Module</option>
            {availableModules.map((m) => <option key={m.id} value={m.id}>{m.title}</option>)}
          </select>

          <input
            value={quizTitle}
            onChange={(e) => setQuizTitle(e.target.value)}
            placeholder="Quiz Title"
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white text-sm focus:ring-1 focus:ring-indigo-500 outline-none"
          />

          <input
            type="number"
            value={timeLimitMinutes}
            onChange={(e) => setTimeLimitMinutes(+e.target.value)}
            placeholder="Time Limit (mins)"
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white text-sm focus:ring-1 focus:ring-indigo-500 outline-none"
          />

          <button
            onClick={handleCreateQuiz}
            className="w-full bg-indigo-600 hover:bg-indigo-500 text-white font-bold py-3.5 rounded-xl transition-all shadow-lg shadow-indigo-500/20 text-xs uppercase tracking-widest mt-2"
          >
            Initialize Quiz
          </button>
        </div>
      </div>

      {/* STEP 2: QUESTIONS */}
      {quizCreated && (
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-8 shadow-2xl animate-in fade-in slide-in-from-bottom-4 duration-500">
          <div className="flex justify-between items-center mb-8 pb-4 border-b border-slate-800/50">
            <div className="flex items-center gap-3">
              <PlusCircle className="text-emerald-500" size={20} />
              <div>
                <h3 className="text-[10px] font-bold uppercase tracking-widest text-white">Step 2: Add Questions</h3>
                <p className="text-[9px] text-emerald-500 font-bold uppercase mt-0.5">{activeModuleName}</p>
              </div>
            </div>
            <div className="bg-emerald-500/10 text-emerald-500 px-3 py-1 rounded-full text-[10px] font-bold">
              {questionCount} Added
            </div>
          </div>

          <div className="space-y-6">
            <div className="flex gap-4">
              <textarea
                rows={2}
                value={questionForm.text}
                onChange={(e) => setQuestionForm({ ...questionForm, text: e.target.value })}
                placeholder="Type your question here..."
                className="flex-[4] bg-slate-950 border border-slate-800 rounded-xl px-5 py-4 text-white text-base focus:ring-1 focus:ring-emerald-500 outline-none transition resize-none"
              />
              <div className="flex-1">
                <label className="text-[9px] font-bold text-slate-500 uppercase block mb-1">Marks</label>
                <input
                  type="number"
                  value={questionForm.marks}
                  onChange={(e) => setQuestionForm({ ...questionForm, marks: e.target.value })}
                  className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-4 text-white text-center focus:ring-1 focus:ring-emerald-500 outline-none"
                />
              </div>
            </div>

            {/* GROWING OPTION BOXES */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {questionForm.options.map((opt, i) => (
                <div key={i} className="relative group">
                  <textarea
                    rows={1}
                    value={opt}
                    onInput={(e) => {
                      e.target.style.height = 'auto';
                      e.target.style.height = e.target.scrollHeight + 'px';
                    }}
                    onChange={(e) => {
                      const options = [...questionForm.options];
                      options[i] = e.target.value;
                      setQuestionForm({ ...questionForm, options });
                    }}
                    placeholder={`Option ${i + 1}`}
                    className={`w-full bg-slate-950 border border-slate-800 rounded-xl pl-5 pr-12 py-3.5 text-sm text-white outline-none transition overflow-hidden resize-none ${
                      questionForm.correctAnswerIndex === i ? 'ring-1 ring-emerald-500 border-emerald-500 bg-emerald-500/5' : 'focus:border-slate-600'
                    }`}
                  />
                  {questionForm.correctAnswerIndex === i && (
                    <CheckCircle className="absolute right-4 top-4 text-emerald-500" size={18} />
                  )}
                </div>
              ))}
            </div>

            <div className="flex flex-col md:flex-row gap-4 pt-6 border-t border-slate-800/50">
              <div className="flex-[2]">
                <label className="text-[9px] font-bold text-slate-500 uppercase block mb-2">Select Correct Answer</label>
                <select
                  value={questionForm.correctAnswerIndex}
                  onChange={(e) => setQuestionForm({ ...questionForm, correctAnswerIndex: Number(e.target.value) })}
                  className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white text-sm focus:ring-1 focus:ring-emerald-500 outline-none"
                >
                  {questionForm.options.map((_, i) => (
                    <option key={i} value={i}>Option {i + 1} is Correct</option>
                  ))}
                </select>
              </div>
              <button
                onClick={handleAddQuestion}
                className="flex-1 bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-3 rounded-xl transition-all shadow-lg shadow-emerald-500/20 text-xs uppercase tracking-widest self-end h-[46px]"
              >
                Save Question
              </button>
            </div>

            <button
              onClick={completeModule}
              className="w-full bg-white text-slate-900 font-bold py-3.5 rounded-xl hover:bg-slate-200 transition-all text-xs uppercase tracking-widest mt-2"
            >
              Finish Module
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default AddQuiz;