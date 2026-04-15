'use client';

import { useEffect, useState } from 'react';
import { ProjectSearchFilters, ProjectStatus, ProjectType } from '@/types';

type ProjectFiltersProps = {
  filters: ProjectSearchFilters;
  onChange: (next: ProjectSearchFilters) => void;
};

function toNumberOrUndefined(value: string): number | undefined {
  if (!value.trim()) return undefined;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : undefined;
}

function normalizeSkills(value: string): string[] {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

export function ProjectFilters({ filters, onChange }: ProjectFiltersProps) {
  const [keywordInput, setKeywordInput] = useState(filters.keyword ?? '');
  const [skillsInput, setSkillsInput] = useState((filters.skills ?? []).join(', '));

  useEffect(() => {
    setKeywordInput(filters.keyword ?? '');
  }, [filters.keyword]);

  useEffect(() => {
    setSkillsInput((filters.skills ?? []).join(', '));
  }, [filters.skills]);

  useEffect(() => {
    const normalizedKeyword = keywordInput.trim();
    const currentKeyword = filters.keyword ?? '';
    if (normalizedKeyword === currentKeyword) {
      return;
    }

    const timer = window.setTimeout(() => {
      onChange({
        ...filters,
        keyword: normalizedKeyword || undefined,
      });
    }, 300);

    return () => window.clearTimeout(timer);
  }, [filters, keywordInput, onChange]);

  const applySkills = () => {
    const nextSkills = normalizeSkills(skillsInput);
    const currentSkills = (filters.skills ?? []).join('|');
    const nextValue = nextSkills.join('|');

    if (nextValue === currentSkills) {
      return;
    }

    onChange({
      ...filters,
      skills: nextSkills.length > 0 ? nextSkills : undefined,
    });
  };

  const updateBudget = (field: 'minBudget' | 'maxBudget', value: string) => {
    const parsed = toNumberOrUndefined(value);
    onChange({
      ...filters,
      [field]: parsed,
    });
  };

  return (
    <section className="mb-8 rounded-2xl border border-gray-200 bg-white/80 p-4 shadow-sm backdrop-blur-sm dark:border-gray-700 dark:bg-gray-900/70">
      <div className="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-3">
        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Поиск</span>
          <input
            value={keywordInput}
            onChange={(e) => setKeywordInput(e.target.value)}
            placeholder="Название или описание"
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          />
        </label>

        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Статус</span>
          <select
            value={filters.status ?? ''}
            onChange={(e) => onChange({ ...filters, status: (e.target.value || undefined) as ProjectStatus | undefined })}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          >
            <option value="">Все</option>
            <option value="Open">Открыт</option>
            <option value="InProgress">В работе</option>
            <option value="Completed">Завершен</option>
            <option value="Cancelled">Отменен</option>
          </select>
        </label>

        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Тип проекта</span>
          <select
            value={filters.type ?? ''}
            onChange={(e) => onChange({ ...filters, type: (e.target.value || undefined) as ProjectType | undefined })}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          >
            <option value="">Любой</option>
            <option value="FixedPrice">Фиксированный</option>
            <option value="Hourly">Почасовой</option>
            <option value="LongTerm">Долгосрочный</option>
          </select>
        </label>

        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Бюджет от</span>
          <input
            type="number"
            min={0}
            value={filters.minBudget ?? ''}
            onChange={(e) => updateBudget('minBudget', e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          />
        </label>

        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Бюджет до</span>
          <input
            type="number"
            min={0}
            value={filters.maxBudget ?? ''}
            onChange={(e) => updateBudget('maxBudget', e.target.value)}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          />
        </label>

        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Сортировка</span>
          <select
            value={filters.sortBy ?? 'newest'}
            onChange={(e) =>
              onChange({
                ...filters,
                sortBy: e.target.value as 'newest' | 'budget_asc' | 'budget_desc' | 'deadline',
              })
            }
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          >
            <option value="newest">Сначала новые</option>
            <option value="budget_asc">Бюджет по возрастанию</option>
            <option value="budget_desc">Бюджет по убыванию</option>
            <option value="deadline">По дедлайну</option>
          </select>
        </label>
      </div>

      <div className="mt-3">
        <label className="space-y-1">
          <span className="text-sm font-medium text-gray-700 dark:text-gray-200">Навыки (через запятую)</span>
          <input
            value={skillsInput}
            onChange={(e) => setSkillsInput(e.target.value)}
            onBlur={applySkills}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault();
                applySkills();
              }
            }}
            placeholder="React, TypeScript, ASP.NET"
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none transition focus:border-blue-500 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100"
          />
        </label>
      </div>
    </section>
  );
}
