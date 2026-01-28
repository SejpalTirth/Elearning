import React, { useState } from 'react';
import { Trash2, Plus, AlertCircle, ChevronUp, ChevronDown } from 'lucide-react';

const ModuleEditor = ({ modules = [], onChange }) => {
  const [newModule, setNewModule] = useState({ title: '', content: '' });
  const [localErrors, setLocalErrors] = useState({ title: false, content: false });

  const addModule = () => {
    const hasTitle = newModule.title.trim().length > 0;
    const hasContent = newModule.content.trim().length > 0;

    if (!hasTitle || !hasContent) {
      setLocalErrors({ title: !hasTitle, content: !hasContent });
      return;
    }

    onChange([...modules, { ...newModule }]);
    setNewModule({ title: '', content: '' });
    setLocalErrors({ title: false, content: false });
  };

  // REORDERING LOGIC
  const moveModule = (index, direction) => {
    const newModules = [...modules];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    // Boundary check
    if (targetIndex < 0 || targetIndex >= modules.length) return;

    // Swap elements
    [newModules[index], newModules[targetIndex]] = [newModules[targetIndex], newModules[index]];
    onChange(newModules);
  };

  return (
    <div className="space-y-6">
      {/* INPUT BOX AREA */}
      <div className="space-y-4 bg-slate-50 dark:bg-white/[0.03] p-5 rounded-2xl border border-slate-200 dark:border-white/10 transition-all shadow-sm">
        <div className="space-y-1">
          <input
            value={newModule.title}
            onChange={(e) => {
              setNewModule({ ...newModule, title: e.target.value });
              if (localErrors.title) setLocalErrors({ ...localErrors, title: false });
            }}
            placeholder="Module title (e.g. Introduction to React)"
            className="w-full bg-transparent border-none outline-none focus:ring-0 text-sm font-semibold text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-700"
          />
          <div className={`h-px w-full transition-colors ${localErrors.title ? 'bg-red-500' : 'bg-slate-200 dark:bg-white/10'}`} />
          {localErrors.title && (
            <p className="text-[10px] text-red-500 flex items-center gap-1 mt-1">
              <AlertCircle size={10} /> Title is required
            </p>
          )}
        </div>

        <div className="space-y-1">
          <textarea
            rows={2}
            value={newModule.content}
            onChange={(e) => {
              setNewModule({ ...newModule, content: e.target.value });
              if (localErrors.content) setLocalErrors({ ...localErrors, content: false });
            }}
            placeholder="Briefly describe what this module covers..."
            className="w-full bg-transparent border-none outline-none focus:ring-0 text-xs text-slate-600 dark:text-slate-400 placeholder:text-slate-400 dark:placeholder:text-slate-700 resize-none leading-relaxed"
          />
          <div className={`h-px w-full transition-colors ${localErrors.content ? 'bg-red-500' : 'bg-slate-200 dark:bg-white/10'}`} />
          {localErrors.content && (
            <p className="text-[10px] text-red-500 flex items-center gap-1 mt-1">
              <AlertCircle size={10} /> Content is required
            </p>
          )}
        </div>

        <div className="flex justify-end pt-1">
          <button 
            onClick={addModule} 
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-xl text-[11px] font-bold hover:bg-indigo-700 transition-all active:scale-95"
          >
            <Plus size={14} strokeWidth={3} /> Add Module
          </button>
        </div>
      </div>

      {/* SAVED MODULES LIST WITH REORDERING */}
      <div className="space-y-3">
        {modules.map((m, i) => (
          <div key={i} className="group flex flex-col p-4 rounded-xl border border-slate-100 dark:border-white/[0.03] bg-white dark:bg-transparent hover:bg-slate-50 dark:hover:bg-white/[0.01] transition-all">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span className="text-[10px] font-black text-slate-300 dark:text-slate-700 tracking-tighter">
                  {(i + 1).toString().padStart(2, '0')}
                </span>
                <span className="text-sm font-semibold text-slate-800 dark:text-slate-200 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                  {m.title}
                </span>
              </div>
              
              <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                {/* REORDER BUTTONS */}
                <button 
                  onClick={() => moveModule(i, 'up')}
                  disabled={i === 0}
                  className={`p-1 rounded hover:bg-slate-200 dark:hover:bg-white/10 transition-colors ${i === 0 ? 'text-slate-200 dark:text-slate-800' : 'text-slate-500 dark:text-slate-400'}`}
                >
                  <ChevronUp size={16} />
                </button>
                <button 
                  onClick={() => moveModule(i, 'down')}
                  disabled={i === modules.length - 1}
                  className={`p-1 rounded hover:bg-slate-200 dark:hover:bg-white/10 transition-colors ${i === modules.length - 1 ? 'text-slate-200 dark:text-slate-800' : 'text-slate-500 dark:text-slate-400'}`}
                >
                  <ChevronDown size={16} />
                </button>
                <div className="w-px h-4 bg-slate-200 dark:bg-white/10 mx-1" />
                <button 
                  onClick={() => onChange(modules.filter((_, x) => x !== i))}
                  className="p-1 text-slate-400 hover:text-red-500 transition-all"
                >
                  <Trash2 size={14} />
                </button>
              </div>
            </div>
            <p className="pl-7 mt-1 text-[11px] text-slate-500 dark:text-slate-500 line-clamp-1 italic">
              {m.content}
            </p>
          </div>
        ))}

        {modules.length === 0 && (
          <div className="py-8 text-center text-slate-400 dark:text-slate-800 italic text-[11px] uppercase tracking-widest font-medium">
            Draft your curriculum above
          </div>
        )}
      </div>
    </div>
  );
};

export default ModuleEditor;