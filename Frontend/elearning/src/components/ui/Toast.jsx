import React from 'react';
import { CheckCircle2, AlertCircle, Info, X } from 'lucide-react';

export const ToastContainer = ({ toasts, removeToast }) => {
  return (
    <div className="fixed top-5 right-5 flex flex-col gap-3 z-[2000] pointer-events-none">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`
            pointer-events-auto w-80 p-4 rounded-xl shadow-2xl 
            bg-white/90 dark:bg-slate-900/90 backdrop-blur-md
            border border-slate-200 dark:border-slate-800
            flex items-start gap-3 
            animate-toast-in hover:scale-[1.02] transition-transform duration-200
          `}
        >
          <div className="flex-shrink-0 mt-0.5">
            {t.type === 'success' && <CheckCircle2 className="w-5 h-5 text-emerald-500" />}
            {t.type === 'error' && <AlertCircle className="w-5 h-5 text-rose-500" />}
            {t.type === 'info' && <Info className="w-5 h-5 text-sky-500" />}
          </div>

          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">
              {t.type.charAt(0).toUpperCase() + t.type.slice(1)}
            </p>
            <p className="text-xs text-slate-600 dark:text-slate-400 mt-1 break-words">
              {t.message}
            </p>
          </div>

          <button 
            onClick={() => removeToast(t.id)}
            className="flex-shrink-0 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      ))}
    </div>
  );
};