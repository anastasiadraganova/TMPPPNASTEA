'use client';

import { useEffect, useState } from 'react';
import { ProjectCategoryNode, ProjectDto } from '@/types';
import { apiFacade } from '@/lib/apiFacade';
import { ProjectCard } from '@/components/ProjectCard';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import { Button } from '@/patterns/ButtonFactory';
import { CategoryTree } from '@/components/CategoryTree';
import { withPopularBadge, withUrgentHighlight } from '@/components/decorators/ProjectCardDecorators';

const EnhancedProjectCard = withPopularBadge(withUrgentHighlight(ProjectCard));

/**
 * Главная страница — каталог открытых проектов.
 * Использует Abstract Factory (ThemeFactory) для создания тематизированных компонентов.
 * Использует Factory Method (ButtonFactory) для кнопок фильтрации.
 */
export default function HomePage() {
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [categories, setCategories] = useState<ProjectCategoryNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<string>('');
  const factory = useThemeFactory();

  useEffect(() => {
    loadProjects();
  }, [statusFilter]);

  async function loadProjects() {
    setLoading(true);
    try {
      const [projectsData, categoriesData] = await Promise.all([
        apiFacade.getProjects(statusFilter || undefined),
        apiFacade.getProjectCategoriesTree(),
      ]);

      setProjects(projectsData);
      setCategories(categoriesData);
    } catch (err) {
      console.error('Ошибка загрузки проектов:', err);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <CategoryTree nodes={categories} />

      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Открытые проекты
        </h1>
        <div className="flex gap-2">
          {/* Factory Method: кнопки фильтрации создаются через фабрику */}
          <Button
            variant={statusFilter === '' ? 'primary' : 'ghost'}
            onClick={() => setStatusFilter('')}
          >
            Все
          </Button>
          <Button
            variant={statusFilter === 'Open' ? 'primary' : 'ghost'}
            onClick={() => setStatusFilter('Open')}
          >
            Открытые
          </Button>
          <Button
            variant={statusFilter === 'InProgress' ? 'primary' : 'ghost'}
            onClick={() => setStatusFilter('InProgress')}
          >
            В работе
          </Button>
          <Button
            variant={statusFilter === 'Completed' ? 'primary' : 'ghost'}
            onClick={() => setStatusFilter('Completed')}
          >
            Завершённые
          </Button>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12">
          {factory.createText({ children: 'Загрузка проектов...' })}
        </div>
      ) : projects.length === 0 ? (
        <div className="text-center py-12">
          {factory.createCard({
            children: (
              <div className="text-center">
                <h2 className="text-xl font-semibold mb-2 text-gray-900 dark:text-white">
                  Проекты не найдены
                </h2>
                {factory.createText({ children: 'Пока нет открытых проектов. Создайте первый!' })}
              </div>
            ),
          })}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <EnhancedProjectCard key={project.id} project={project} />
          ))}
        </div>
      )}
    </div>
  );
}
