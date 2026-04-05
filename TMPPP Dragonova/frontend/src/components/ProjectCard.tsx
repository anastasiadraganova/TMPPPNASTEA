'use client';

import { ProjectDto } from '@/types';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import Link from 'next/link';

const statusLabels: Record<string, string> = {
  Open: 'Открыт',
  InProgress: 'В работе',
  Completed: 'Завершён',
  Cancelled: 'Отменён',
};

const statusColors: Record<string, string> = {
  Open: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  InProgress: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
  Completed: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  Cancelled: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
};

/**
 * Карточка проекта — использует Abstract Factory (ThemeFactory)
 * для создания тематизированных компонентов Card, Text, Badge.
 */
export function ProjectCard({ project }: { project: ProjectDto }) {
  const factory = useThemeFactory();

  return (
    <Link href={`/projects/${project.id}`}>
      {factory.createCard({
        className: 'hover:shadow-md transition-shadow cursor-pointer',
        children: (
          <>
            <div className="flex justify-between items-start mb-3">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                {project.title}
              </h3>
              <span className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${statusColors[project.status]}`}>
                {statusLabels[project.status]}
              </span>
            </div>
            {factory.createText({
              className: 'mb-4 line-clamp-2',
              children: project.description,
            })}
            <div className="flex flex-wrap gap-2 mb-4">
              {project.requiredSkills.map((skill) =>
                factory.createBadge({ key: skill, children: skill })
              )}
            </div>
            <div className="flex justify-between items-center text-sm">
              {factory.createText({
                children: (
                  <>
                    <span className="font-medium">${project.budget.toLocaleString()}</span>
                    {' · '}
                    {project.proposalCount} откликов
                    {' · '}
                    {project.customerName}
                  </>
                ),
              })}
            </div>
          </>
        ),
      })}
    </Link>
  );
}
