'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import { apiClient } from '@/lib/api';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import { Button } from '@/patterns/ButtonFactory';
import { ProjectType } from '@/types';

/**
 * Страница создания проекта.
 * Использует Factory Method для кнопок и Abstract Factory для UI-элементов.
 */
export default function CreateProjectPage() {
  const { user } = useAuthStore();
  const router = useRouter();
  const factory = useThemeFactory();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [budget, setBudget] = useState('');
  const [type, setType] = useState<ProjectType>('FixedPrice');
  const [deadline, setDeadline] = useState('');
  const [skills, setSkills] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [useTemplate, setUseTemplate] = useState(false);
  const [templateId, setTemplateId] = useState('landing');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    try {
      if (useTemplate) {
        await apiClient.createProjectFromTemplate(templateId);
      } else {
        await apiClient.createProject({
          title,
          description,
          budget: Number(budget),
          type,
          deadline: deadline || undefined,
          requiredSkills: skills.split(',').map((s) => s.trim()).filter(Boolean),
        });
      }
      router.push('/dashboard');
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  }

  if (!user || user.role !== 'Customer') {
    return (
      <div className="text-center py-12">
        {factory.createText({ children: 'Только заказчики могут создавать проекты' })}
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Создать проект</h1>

      {/* Переключатель: свой проект или из шаблона (Prototype) */}
      <div className="flex gap-4 mb-6">
        <Button
          variant={!useTemplate ? 'primary' : 'ghost'}
          onClick={() => setUseTemplate(false)}
        >
          Свой проект
        </Button>
        <Button
          variant={useTemplate ? 'primary' : 'ghost'}
          onClick={() => setUseTemplate(true)}
        >
          Из шаблона (Prototype)
        </Button>
      </div>

      {factory.createCard({
        children: (
          <form onSubmit={handleSubmit} className="space-y-4">
            {useTemplate ? (
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                  Шаблон проекта
                </label>
                <select
                  value={templateId}
                  onChange={(e) => setTemplateId(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg dark:bg-gray-800 dark:border-gray-600 dark:text-white"
                >
                  <option value="landing">Разработка лендинга ($500, 14 дней)</option>
                  <option value="mobile-app">Мобильное приложение ($5 000, 90 дней)</option>
                  <option value="tech-support">Техподдержка ($1 500/мес, 6 мес)</option>
                </select>
                {factory.createText({
                  className: 'mt-2 text-sm',
                  children: 'Проект будет создан из готового шаблона с предзаполненными данными (паттерн Prototype).',
                })}
              </div>
            ) : (
              <>
                <div>
                  <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                    Название
                  </label>
                  {factory.createInput({
                    type: 'text',
                    value: title,
                    onChange: (e) => setTitle(e.target.value),
                    required: true,
                    placeholder: 'Название проекта',
                  })}
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                    Описание
                  </label>
                  <textarea
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                    required
                    rows={4}
                    className="w-full px-3 py-2 border rounded-lg dark:bg-gray-800 dark:border-gray-600 dark:text-white"
                    placeholder="Опишите задачу подробно"
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                      Бюджет ($)
                    </label>
                    {factory.createInput({
                      type: 'number',
                      value: budget,
                      onChange: (e) => setBudget(e.target.value),
                      required: true,
                      min: 1,
                      placeholder: 'Сумма',
                    })}
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                      Тип проекта
                    </label>
                    <select
                      value={type}
                      onChange={(e) => setType(e.target.value as ProjectType)}
                      className="w-full px-3 py-2 border rounded-lg dark:bg-gray-800 dark:border-gray-600 dark:text-white"
                    >
                      <option value="FixedPrice">Фиксированная цена</option>
                      <option value="Hourly">Почасовой</option>
                      <option value="LongTerm">Долгосрочный</option>
                    </select>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                    Дедлайн
                  </label>
                  {factory.createInput({
                    type: 'date',
                    value: deadline,
                    onChange: (e) => setDeadline(e.target.value),
                  })}
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                    Навыки (через запятую)
                  </label>
                  {factory.createInput({
                    type: 'text',
                    value: skills,
                    onChange: (e) => setSkills(e.target.value),
                    placeholder: 'React, TypeScript, Node.js',
                  })}
                </div>
              </>
            )}
            <div className="flex gap-4">
              <Button variant="primary" type="submit" disabled={submitting}>
                {submitting ? 'Создание...' : 'Создать проект'}
              </Button>
              <Button variant="ghost" type="button" onClick={() => router.back()}>
                Отмена
              </Button>
            </div>
          </form>
        ),
      })}
    </div>
  );
}
