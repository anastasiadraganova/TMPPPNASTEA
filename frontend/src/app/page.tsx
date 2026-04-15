'use client';

import { Suspense, useEffect, useMemo, useState } from 'react';
import { ReadonlyURLSearchParams, usePathname, useRouter, useSearchParams } from 'next/navigation';
import { ProjectCategoryNode, ProjectDto, ProjectSearchFilters, ProjectStatus, ProjectType } from '@/types';
import { apiFacade } from '@/lib/apiFacade';
import { ProjectCard } from '@/components/ProjectCard';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import { CategoryTree } from '@/components/CategoryTree';
import { withPopularBadge, withUrgentHighlight } from '@/components/decorators/ProjectCardDecorators';
import { ProjectFilters } from '@/components/ProjectFilters';

const EnhancedProjectCard = withPopularBadge(withUrgentHighlight(ProjectCard));

const statusValues = new Set<ProjectStatus>(['Open', 'InProgress', 'Completed', 'Cancelled']);
const typeValues = new Set<ProjectType>(['FixedPrice', 'Hourly', 'LongTerm']);
type SortByValue = NonNullable<ProjectSearchFilters['sortBy']>;
const sortValues = new Set<SortByValue>(['newest', 'budget_asc', 'budget_desc', 'deadline']);

function parseNumber(value: string | null): number | undefined {
  if (!value) return undefined;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : undefined;
}

function parseFilters(params: ReadonlyURLSearchParams): ProjectSearchFilters {
  const statusParam = params.get('status');
  const typeParam = params.get('type');
  const sortByParam = params.get('sortBy');
  const keywordParam = params.get('keyword')?.trim();
  const skillsParam = params.get('skills');

  const status = statusParam && statusValues.has(statusParam as ProjectStatus)
    ? (statusParam as ProjectStatus)
    : undefined;

  const type = typeParam && typeValues.has(typeParam as ProjectType)
    ? (typeParam as ProjectType)
    : undefined;

  const sortBy = sortByParam && sortValues.has(sortByParam as SortByValue)
    ? (sortByParam as SortByValue)
    : 'newest';

  const skills = skillsParam
    ? skillsParam
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0)
    : undefined;

  return {
    status,
    maxBudget: parseNumber(params.get('maxBudget')),
    minBudget: parseNumber(params.get('minBudget')),
    keyword: keywordParam || undefined,
    skills,
    type,
    sortBy,
  };
}

function toQueryString(filters: ProjectSearchFilters): string {
  const params = new URLSearchParams();
  if (filters.status) params.set('status', filters.status);
  if (typeof filters.maxBudget === 'number') params.set('maxBudget', filters.maxBudget.toString());
  if (typeof filters.minBudget === 'number') params.set('minBudget', filters.minBudget.toString());
  if (filters.keyword) params.set('keyword', filters.keyword);
  if (filters.skills && filters.skills.length > 0) params.set('skills', filters.skills.join(','));
  if (filters.type) params.set('type', filters.type);
  if (filters.sortBy && filters.sortBy !== 'newest') params.set('sortBy', filters.sortBy);
  return params.toString();
}

/**
 * Главная страница — каталог открытых проектов.
 * Использует Abstract Factory (ThemeFactory) для создания тематизированных компонентов.
 * Использует Factory Method (ButtonFactory) для кнопок фильтрации.
 */
function HomePageContent() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [categories, setCategories] = useState<ProjectCategoryNode[]>([]);
  const [loading, setLoading] = useState(true);

  const filters = useMemo(() => parseFilters(searchParams), [searchParams]);
  const factory = useThemeFactory();

  useEffect(() => {
    loadProjects(filters);
  }, [filters]);

  async function loadProjects(activeFilters: ProjectSearchFilters) {
    setLoading(true);
    try {
      const [projectsData, categoriesData] = await Promise.all([
        apiFacade.getProjects(activeFilters),
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

  function handleFiltersChange(nextFilters: ProjectSearchFilters) {
    const queryString = toQueryString(nextFilters);
    router.push(queryString ? `${pathname}?${queryString}` : pathname);
  }

  return (
    <div>
      <CategoryTree nodes={categories} />

      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Открытые проекты
        </h1>
      </div>

      <ProjectFilters filters={filters} onChange={handleFiltersChange} />

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

export default function HomePage() {
  return (
    <Suspense fallback={<div className="py-12 text-center text-gray-500 dark:text-gray-400">Загрузка...</div>}>
      <HomePageContent />
    </Suspense>
  );
}
