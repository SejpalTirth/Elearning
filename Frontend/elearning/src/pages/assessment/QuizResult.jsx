import { useParams, useNavigate } from 'react-router-dom';
import { useGetQuizResultQuery } from '../../services/assessmentApiSlice';
import { 
  Loader2, Trophy, Target, CheckCircle2, 
  RotateCcw, ArrowLeft, XCircle, Percent 
} from 'lucide-react';

const QuizResult = () => {
  const { CourseId, ModuleId, submissionId } = useParams();
  const navigate = useNavigate();

  const { data: result, isLoading } = useGetQuizResultQuery({ submissionId });

  if (isLoading) return (
    <div className="fixed inset-0 bg-[#0f172a] flex items-center justify-center">
      <Loader2 className="w-10 h-10 text-indigo-500 animate-spin" />
    </div>
  );

  const displayMarks = result?.obtainedMarks ?? 0;
  const displayTotal = result?.totalMarks ?? 20;
  const displayPercent = result?.percentage ?? 0;
  const isPassed = result?.passed ?? false;

  return (
    <div className="fixed inset-0 z-[100] bg-[#0f172a] text-slate-200 overflow-y-auto">
      <div className={`absolute top-0 left-1/2 -translate-x-1/2 w-full h-[500px] opacity-20 blur-[120px] pointer-events-none -z-10 ${isPassed ? 'bg-emerald-500' : 'bg-rose-500'}`} />

      <div className="max-w-4xl mx-auto px-6 py-20 flex flex-col items-center">
        
        <div className={`w-20 h-20 rounded-2xl flex items-center justify-center mb-6 shadow-2xl ${isPassed ? 'bg-emerald-500 text-white' : 'bg-rose-500 text-white'}`}>
          {isPassed ? <Trophy className="w-10 h-10" /> : <XCircle className="w-10 h-10" />}
        </div>

        <h1 className="text-4xl font-black text-white mb-2 tracking-tight">
          {isPassed ? 'Module Completed!' : 'Assessment Result'}
        </h1>
        
        {/* Using the statusMessage from your JSON response */}
        <p className="text-slate-400 text-lg mb-12 text-center max-w-md">
          {result?.statusMessage || "Review your performance below."}
        </p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 w-full mb-12">
          {/* Score Card */}
          <div className="bg-slate-900/50 border border-slate-800 p-8 rounded-3xl text-center">
            <Target className="w-6 h-6 text-indigo-400 mx-auto mb-3" />
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest mb-1">Obtained Marks</p>
            <p className="text-3xl font-bold text-white">{displayMarks}<span className="text-slate-600 text-xl">/{displayTotal}</span></p>
          </div>

          {/* Percentage Card */}
          <div className="bg-slate-900/50 border border-slate-800 p-8 rounded-3xl text-center">
            <Percent className="w-6 h-6 text-sky-400 mx-auto mb-3" />
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest mb-1">Percentage</p>
            <p className="text-3xl font-bold text-white">{displayPercent}%</p>
          </div>

          {/* Status Card */}
          <div className="bg-slate-900/50 border border-slate-800 p-8 rounded-3xl text-center">
            <CheckCircle2 className={`w-6 h-6 mx-auto mb-3 ${isPassed ? 'text-emerald-400' : 'text-rose-400'}`} />
            <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest mb-1">Result</p>
            <p className={`text-xl font-bold uppercase ${isPassed ? 'text-emerald-500' : 'text-rose-500'}`}>
              {isPassed ? 'Passed' : 'Failed'}
            </p>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row gap-4">
          <button
            onClick={() => navigate(ModuleId ? `/courses/${CourseId}/modules/${ModuleId}` : '/dashboard', {replace : true})}
            className="flex items-center justify-center gap-2 px-10 py-4 bg-white text-[#0f172a] rounded-xl font-bold hover:bg-slate-200 transition-all active:scale-95"
          >
            <ArrowLeft className="w-5 h-5" />
            Back to Module
          </button>

          {!isPassed && (
            <button
              onClick={() => navigate(-1 , {replace : true})} 
              className="flex items-center justify-center gap-2 px-10 py-4 bg-indigo-600 text-white rounded-xl font-bold hover:bg-indigo-500 transition-all active:scale-95 shadow-xl shadow-indigo-500/20"
            >
              <RotateCcw className="w-5 h-5" />
              Try Again
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default QuizResult;