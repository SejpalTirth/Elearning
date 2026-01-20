import React from 'react';

export const Loader = () => {
  return (
    <div className="fixed inset-0 z-[9999] flex flex-col items-center justify-center 
                    bg-white/60 dark:bg-slate-900/60 backdrop-blur-[2px] 
                    transition-all duration-300">
      
      {/* The Spinner */}
      <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent 
                      rounded-full animate-spin shadow-sm"></div>
      
      <p className="mt-4 text-sm font-medium text-slate-700 dark:text-slate-200 animate-pulse">
        Please wait a moment
      </p>
    </div>
  );
};