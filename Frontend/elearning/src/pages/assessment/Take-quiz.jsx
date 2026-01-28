import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { 
  useGetQuizByModuleQuery, 
  useSubmitQuizMutation 
} from '../../services/assessmentApiSlice';
import { useCompleteModuleMutation } from '../../services/progressApiSlice';
import { showToast } from '../../features/ui/toastSlice';
import { 
  Loader2, 
  CheckCircle2, GraduationCap 
} from 'lucide-react';
import { hideLoader, showLoader } from '../../features/ui/loadingSlice';

const QuizPage = () => {
  const { CourseId, ModuleId } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  
  const userId = useSelector((state) => state.auth.user?.userId);

  // Queries & Mutations
  const { data: quiz, isLoading } = useGetQuizByModuleQuery({ moduleId: ModuleId });
  const [submitQuiz, { isLoading: isSubmitting }] = useSubmitQuizMutation();
  const [completeModule] = useCompleteModuleMutation();

  const [selectedAnswers, setSelectedAnswers] = useState({});
  const [currentStep, setCurrentStep] = useState(0);
  const [isTimeUp, setIsTimeUp] = useState(false);
  const [timeLeft, setTimeLeft] = useState(null);

  useEffect(() => {
    if (quiz?.timeLimitMinutes && timeLeft === null) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setTimeLeft(quiz.timeLimitMinutes * 60);
    }
  }, [quiz, timeLeft]);

  useEffect(() => {
    if (timeLeft === null || timeLeft <= 0 || isTimeUp) return;
    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          setTimeout(() => setIsTimeUp(true), 0);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [timeLeft === null, isTimeUp]);

  const formatTime = (seconds) => {
    if (seconds === null) return "00:00";
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs < 10 ? '0' : ''}${secs}`;
  };

  // ---------------- SUBMIT LOGIC ----------------
  const handleSubmit = async () => {
    if (!quiz?.quizId || !userId) {
      dispatch(showToast({ message: "Missing quiz or user information", type: 'error' }));
      return;
    }

    const payload = {
      quizId: quiz.quizId,
      userId: userId,
      answers: Object.entries(selectedAnswers).map(([qId, aId]) => ({
        questionId: Number(qId),
        selectedAnswerId: aId
      }))
    };

    try {
      dispatch(showLoader());
      const result = await submitQuiz(payload).unwrap();

      // If user passed, mark the module as complete
      if (result.passed) {
        await completeModule({ moduleId: Number(ModuleId) }).unwrap();
        dispatch(hideLoader());
      }

      dispatch(hideLoader());

      dispatch(showToast({ message: "Assessment submitted!", type: 'success' }));
      
      navigate(`/assessment/${CourseId}/${ModuleId}/result/${result.submissionId}`, { replace: true });

    } catch (err) {
      dispatch(showToast({ 
        message: err?.data?.message || "Failed to submit assessment", 
        type: 'error' 
      }));
    }
  };

  if (isLoading) return (
    <div className="fixed inset-0 bg-[#0f172a] flex items-center justify-center">
      <Loader2 className="w-10 h-10 text-indigo-500 animate-spin" />
    </div>
  );

  const questions = quiz?.questions || [];
  const currentQuestion = questions[currentStep];
  const totalQuestions = questions.length;
  const allAnswered = questions.every(q => selectedAnswers[q.id] !== undefined);

  return (
    <div className="fixed inset-0 z-[100] bg-[#0f172a] text-slate-200 overflow-y-auto font-sans">
      
      {/* 1. Header */}
      <div className="max-w-6xl mx-auto px-8 pt-16 pb-12 flex flex-col md:flex-row md:items-end justify-between border-b border-slate-800/50">
        <div className="space-y-4">
          <div className="flex items-center gap-3">
            <GraduationCap className="w-5 h-5 text-indigo-500" />
            <span className="text-xs font-black tracking-[0.2em] text-indigo-400 uppercase">Assessment Mode</span>
          </div>
          <h1 className="text-4xl font-bold text-white tracking-tight">{quiz?.title}</h1>
        </div>

        <div className="flex items-center gap-12 mt-8 md:mt-0">
          <div className="text-right">
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest mb-1">Remaining</p>
            <p className={`text-3xl font-mono font-medium ${timeLeft < 60 ? 'text-red-500 animate-pulse' : 'text-white'}`}>
              {formatTime(timeLeft)}
            </p>
          </div>
          <div className="text-right">
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest mb-1">Progress</p>
            <p className="text-3xl font-medium text-white">
              {currentStep + 1}<span className="text-slate-700"> / {totalQuestions}</span>
            </p>
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-8 grid grid-cols-1 lg:grid-cols-12 gap-20 mt-12 pb-24">
        
        {/* 2. Sidebar */}
        <div className="lg:col-span-4 space-y-12">
          <div className="space-y-6">
            <h3 className="text-xs font-bold text-slate-400 uppercase tracking-widest">Guidelines</h3>
            <div className="space-y-4 text-sm text-slate-500 italic">
              <p className="flex items-center gap-3">
                <span className="w-1 h-1 rounded-full bg-indigo-500" />
                Passing Score: {quiz?.totalMarks} Points
              </p>
              <p className="flex items-center gap-3">
                <span className="w-1 h-1 rounded-full bg-indigo-500" />
                Submit to save progress
              </p>
            </div>
          </div>
        </div>

        {/* 3. Main Quiz Body */}
        <div className="lg:col-span-8 space-y-12">
          <div>
            <span className="text-xs font-bold text-indigo-400 uppercase mb-4 block tracking-tighter">Question {currentStep + 1}</span>
            <h2 className="text-3xl font-semibold text-white leading-tight">
              {currentQuestion?.text}
            </h2>
          </div>

          <div className="space-y-4">
            {currentQuestion?.answers.map((answer, idx) => {
              const isSelected = selectedAnswers[currentQuestion.id] === answer.id;
              return (
                <button
                  key={answer.id}
                  onClick={() => setSelectedAnswers(prev => ({ ...prev, [currentQuestion.id]: answer.id }))}
                  className={`
                    w-full group text-left p-6 rounded-2xl border-2 transition-all duration-300 flex items-center gap-6
                    ${isSelected 
                      ? 'border-indigo-600 bg-indigo-600/10' 
                      : 'border-slate-800 bg-transparent hover:border-slate-700 hover:bg-slate-800/5'}
                  `}
                >
                  <div className={`
                    w-10 h-10 rounded-xl border-2 flex items-center justify-center font-bold text-sm transition-all
                    ${isSelected ? 'bg-indigo-600 border-indigo-500 text-white' : 'border-slate-800 text-slate-500 group-hover:text-slate-300'}
                  `}>
                    {String.fromCharCode(65 + idx)}
                  </div>
                  <span className={`text-lg transition-colors flex-grow ${isSelected ? 'text-white' : 'text-slate-400 group-hover:text-slate-200'}`}>
                    {answer.text}
                  </span>
                  {isSelected && <CheckCircle2 className="w-5 h-5 text-indigo-500" />}
                </button>
              );
            })}
          </div>

          {/* 4. Controls */}
          <div className="pt-10 flex items-center justify-between border-t border-slate-800/60">
            <button
              disabled={currentStep === 0 || isSubmitting}
              onClick={() => setCurrentStep(prev => prev - 1)}
              className="px-6 py-2 font-bold text-slate-500 hover:text-white disabled:opacity-0 transition-all"
            >
              Back
            </button>

            <button
              onClick={() => currentStep === totalQuestions - 1 ? handleSubmit() : setCurrentStep(prev => prev + 1)}
              disabled={(currentStep === totalQuestions - 1 && !allAnswered) || isSubmitting}
              className={`px-12 py-4 rounded-xl font-bold transition-all transform active:scale-95 flex items-center gap-2 ${
                currentStep === totalQuestions - 1 
                ? 'bg-indigo-600 text-white shadow-xl shadow-indigo-500/20' 
                : 'bg-white text-[#0f172a] hover:bg-slate-200'
              } ${isSubmitting ? 'opacity-70' : ''}`}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Submitting...</span>
                </>
              ) : (
                currentStep === totalQuestions - 1 ? "Submit Assessment" : "Next Question"
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default QuizPage;