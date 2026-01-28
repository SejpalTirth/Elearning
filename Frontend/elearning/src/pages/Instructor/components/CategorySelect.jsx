import React from 'react';

const CategorySelect = ({ categories, value, onChange }) => {
  return (
    <div className="flex flex-wrap gap-2">
      {categories?.map((c) => {
        const isSelected = String(value) === String(c.id);
        return (
          <button
            key={c.id}
            type="button"
            onClick={() => onChange(c.id)}
            className={`px-4 py-1.5 rounded-md text-[11px] font-medium border transition-all ${
              isSelected
                ? 'bg-indigo-600/10 border-indigo-500 text-indigo-600 dark:text-indigo-400 shadow-sm'
                : 'bg-transparent border-slate-200 dark:border-white/10 text-slate-500 hover:border-slate-400 dark:hover:border-white/30 hover:text-slate-700 dark:hover:text-slate-300'
            }`}
          >
            {c.name}
          </button>
        );
      })}
    </div>
  );
};

export default CategorySelect;