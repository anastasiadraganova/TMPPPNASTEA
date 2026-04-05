'use client';

import { ComponentType } from 'react';
import { ProjectDto } from '@/types';

type ProjectCardProps = { project: ProjectDto };

function isUrgent(project: ProjectDto): boolean {
  if (!project.deadline || project.status !== 'Open') return false;
  const deadline = new Date(project.deadline).getTime();
  const now = Date.now();
  const daysLeft = (deadline - now) / (1000 * 60 * 60 * 24);
  return daysLeft >= 0 && daysLeft <= 3;
}

function isPopular(project: ProjectDto): boolean {
  return project.proposalCount >= 3;
}

// Decorator: добавляет визуальный акцент для срочных проектов.
export function withUrgentHighlight(Wrapped: ComponentType<ProjectCardProps>) {
  return function UrgentHighlightedCard({ project }: ProjectCardProps) {
    if (!isUrgent(project)) return <Wrapped project={project} />;

    return (
      <div className="rounded-xl ring-2 ring-orange-400 ring-offset-2 ring-offset-transparent">
        <div className="mb-2 inline-flex px-2 py-1 text-xs font-semibold rounded-md bg-orange-100 text-orange-800 dark:bg-orange-900/40 dark:text-orange-300">
          Срочно
        </div>
        <Wrapped project={project} />
      </div>
    );
  };
}

// Decorator: добавляет бейдж популярности в карточку.
export function withPopularBadge(Wrapped: ComponentType<ProjectCardProps>) {
  return function PopularBadgedCard({ project }: ProjectCardProps) {
    if (!isPopular(project)) return <Wrapped project={project} />;

    return (
      <div className="relative">
        <span className="absolute -top-2 -right-2 z-10 inline-flex items-center px-2 py-1 rounded-full text-xs font-semibold bg-emerald-600 text-white shadow">
          Hot
        </span>
        <Wrapped project={project} />
      </div>
    );
  };
}
