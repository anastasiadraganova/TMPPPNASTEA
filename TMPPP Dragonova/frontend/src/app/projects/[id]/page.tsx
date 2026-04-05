'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { ProjectDto, ProposalDto } from '@/types';
import { apiClient } from '@/lib/api';
import { useAuthStore } from '@/store/authStore';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import { Button } from '@/patterns/ButtonFactory';

const statusLabels: Record<string, string> = {
  Open: 'Открыт',
  InProgress: 'В работе',
  Completed: 'Завершён',
  Cancelled: 'Отменён',
};

export default function ProjectDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuthStore();
  const factory = useThemeFactory();
  const [project, setProject] = useState<ProjectDto | null>(null);
  const [proposals, setProposals] = useState<ProposalDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Отклик
  const [coverLetter, setCoverLetter] = useState('');
  const [bidAmount, setBidAmount] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const projectId = params.id as string;

  useEffect(() => {
    loadProject();
  }, [projectId]);

  async function loadProject() {
    setLoading(true);
    try {
      const [p, props] = await Promise.all([
        apiClient.getProject(projectId),
        user ? apiClient.getProjectProposals(projectId).catch(() => []) : Promise.resolve([]),
      ]);
      setProject(p);
      setProposals(props);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  async function handleProposal(e: React.FormEvent) {
    e.preventDefault();
    if (!user) return router.push('/login');
    setSubmitting(true);
    try {
      await apiClient.createProposal(projectId, {
        coverLetter,
        bidAmount: Number(bidAmount),
      });
      setCoverLetter('');
      setBidAmount('');
      loadProject();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  }

  async function handleAcceptProposal(proposalId: string) {
    try {
      await apiClient.acceptProposal(proposalId);
      loadProject();
    } catch (err: any) {
      alert(err.message);
    }
  }

  async function handleComplete() {
    try {
      await apiClient.completeProject(projectId);
      loadProject();
    } catch (err: any) {
      alert(err.message);
    }
  }

  if (loading) {
    return <div className="text-center py-12">{factory.createText({ children: 'Загрузка...' })}</div>;
  }

  if (!project) {
    return <div className="text-center py-12">{factory.createText({ children: 'Проект не найден' })}</div>;
  }

  const isOwner = user && user.id === project.customerId;
  const isFreelancer = user && user.role === 'Freelancer';

  return (
    <div className="max-w-4xl mx-auto">
      {/* Основная информация */}
      {factory.createCard({
        className: 'mb-6',
        children: (
          <>
            <div className="flex justify-between items-start mb-4">
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{project.title}</h1>
              <span className="px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                {statusLabels[project.status]}
              </span>
            </div>
            {factory.createText({ className: 'mb-4 whitespace-pre-wrap', children: project.description })}

            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Бюджет</p>
                <p className="text-lg font-semibold text-gray-900 dark:text-white">${project.budget.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Заказчик</p>
                <p className="text-lg font-semibold text-gray-900 dark:text-white">{project.customerName}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Откликов</p>
                <p className="text-lg font-semibold text-gray-900 dark:text-white">{project.proposalCount}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Дедлайн</p>
                <p className="text-lg font-semibold text-gray-900 dark:text-white">
                  {project.deadline ? new Date(project.deadline).toLocaleDateString('ru') : '—'}
                </p>
              </div>
            </div>

            <div className="flex flex-wrap gap-2 mb-4">
              {project.requiredSkills.map((skill) => factory.createBadge({ key: skill, children: skill }))}
            </div>

            {/* Кнопки владельца: Factory Method для кнопок */}
            {isOwner && project.status === 'InProgress' && (
              <Button variant="primary" onClick={handleComplete}>
                Завершить проект
              </Button>
            )}
            {isOwner && project.status === 'Open' && (
              <Button variant="danger" onClick={async () => {
                await apiClient.deleteProject(projectId);
                router.push('/dashboard');
              }}>
                Удалить проект
              </Button>
            )}
          </>
        ),
      })}

      {/* Форма отклика для фрилансера */}
      {isFreelancer && project.status === 'Open' && (
        <>
          {factory.createCard({
            className: 'mb-6',
            children: (
              <>
                <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">Откликнуться</h2>
                <form onSubmit={handleProposal} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                      Сопроводительное письмо
                    </label>
                    <textarea
                      value={coverLetter}
                      onChange={(e) => setCoverLetter(e.target.value)}
                      required
                      rows={4}
                      className="w-full px-3 py-2 border rounded-lg dark:bg-gray-800 dark:border-gray-600 dark:text-white"
                      placeholder="Расскажите, почему вы подходите для этого проекта"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                      Ваша ставка ($)
                    </label>
                    {factory.createInput({
                      type: 'number',
                      value: bidAmount,
                      onChange: (e) => setBidAmount(e.target.value),
                      required: true,
                      min: 1,
                      placeholder: 'Сумма',
                    })}
                  </div>
                  <Button variant="primary" type="submit" disabled={submitting}>
                    {submitting ? 'Отправка...' : 'Откликнуться'}
                  </Button>
                </form>
              </>
            ),
          })}
        </>
      )}

      {/* Список откликов (для владельца) */}
      {isOwner && proposals.length > 0 && (
        <>
          <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">
            Отклики ({proposals.length})
          </h2>
          <div className="space-y-4">
            {proposals.map((proposal) =>
              factory.createCard({
                key: proposal.id,
                className: 'mb-2',
                children: (
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="font-medium text-gray-900 dark:text-white">{proposal.freelancerName}</p>
                      {factory.createText({ children: proposal.coverLetter })}
                      <p className="text-sm mt-2 text-gray-500">Ставка: ${proposal.bidAmount.toLocaleString()}</p>
                    </div>
                    {proposal.status === 'Pending' && project.status === 'Open' && (
                      <div className="flex gap-2 ml-4">
                        <Button variant="primary" onClick={() => handleAcceptProposal(proposal.id)}>
                          Принять
                        </Button>
                        <Button variant="ghost" onClick={async () => {
                          await apiClient.rejectProposal(proposal.id);
                          loadProject();
                        }}>
                          Отклонить
                        </Button>
                      </div>
                    )}
                    {proposal.status !== 'Pending' && (
                      <span className={`text-sm font-medium ${
                        proposal.status === 'Accepted' ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {proposal.status === 'Accepted' ? 'Принят' : 'Отклонён'}
                      </span>
                    )}
                  </div>
                ),
              })
            )}
          </div>
        </>
      )}
    </div>
  );
}
