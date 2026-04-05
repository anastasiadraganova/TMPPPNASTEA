'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import { ProjectDto, ProposalDto } from '@/types';
import { apiClient } from '@/lib/api';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import { Button } from '@/patterns/ButtonFactory';
import { ProjectCard } from '@/components/ProjectCard';
import Link from 'next/link';

export default function DashboardPage() {
  const { user, loadUser } = useAuthStore();
  const router = useRouter();
  const factory = useThemeFactory();
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [proposals, setProposals] = useState<ProposalDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) {
      loadUser().then(() => {
        if (!useAuthStore.getState().user) {
          router.push('/login');
        }
      });
    }
  }, []);

  useEffect(() => {
    if (user) loadData();
  }, [user]);

  async function loadData() {
    setLoading(true);
    try {
      if (user!.role === 'Customer') {
        const data = await apiClient.getProjects();
        setProjects(data.filter((p) => p.customerId === user!.id));
      } else {
        const data = await apiClient.getProjects();
        setProjects(data.filter((p) => p.freelancerId === user!.id));
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  if (!user) {
    return <div className="text-center py-12">{factory.createText({ children: 'Загрузка...' })}</div>;
  }

  const isCustomer = user.role === 'Customer';

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Личный кабинет
          </h1>
          {factory.createText({
            children: `${user.name} · ${isCustomer ? 'Заказчик' : 'Фрилансер'} · ${user.email}`,
          })}
        </div>
        {isCustomer && (
          <Link href="/projects/create">
            <Button variant="primary">Создать проект</Button>
          </Link>
        )}
      </div>

      {loading ? (
        <div className="text-center py-12">{factory.createText({ children: 'Загрузка...' })}</div>
      ) : (
        <>
          <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">
            {isCustomer ? 'Мои проекты' : 'Мои задачи'}
          </h2>
          {projects.length === 0 ? (
            factory.createCard({
              children: (
                <div className="text-center py-8">
                  {factory.createText({
                    children: isCustomer
                      ? 'У вас пока нет проектов. Создайте первый!'
                      : 'Вы пока не взяли ни одного проекта. Откликнитесь на открытый проект!',
                  })}
                  <div className="mt-4">
                    {isCustomer ? (
                      <Link href="/projects/create">
                        <Button variant="primary">Создать проект</Button>
                      </Link>
                    ) : (
                      <Link href="/">
                        <Button variant="primary">Смотреть проекты</Button>
                      </Link>
                    )}
                  </div>
                </div>
              ),
            })
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {projects.map((project) => (
                <ProjectCard key={project.id} project={project} />
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
