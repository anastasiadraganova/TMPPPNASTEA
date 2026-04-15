'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { apiFacade } from '@/lib/apiFacade';
import { ProjectDto, ProposalDto, ProposalHistoryEntry } from '@/types';
import { Button } from '@/patterns/ButtonFactory';

type ProposalActionsProps = {
  project: ProjectDto;
  proposals: ProposalDto[];
  isOwner: boolean;
  onRefresh: () => Promise<void>;
};

const actionLabels: Record<string, string> = {
  AcceptProposal: 'Принят отклик',
  RejectProposal: 'Отклонен отклик',
};

export function ProposalActions({ project, proposals, isOwner, onRefresh }: ProposalActionsProps) {
  const [lastCommandId, setLastCommandId] = useState<string | null>(null);
  const [undoVisible, setUndoVisible] = useState(false);
  const [undoFading, setUndoFading] = useState(false);
  const [undoLoading, setUndoLoading] = useState(false);

  const [history, setHistory] = useState<ProposalHistoryEntry[]>([]);
  const [historyOpen, setHistoryOpen] = useState(false);
  const [historyLoading, setHistoryLoading] = useState(false);

  const hideTimerRef = useRef<number | null>(null);
  const fadeTimerRef = useRef<number | null>(null);

  const clearUndoTimers = () => {
    if (hideTimerRef.current) {
      window.clearTimeout(hideTimerRef.current);
      hideTimerRef.current = null;
    }

    if (fadeTimerRef.current) {
      window.clearTimeout(fadeTimerRef.current);
      fadeTimerRef.current = null;
    }
  };

  const loadHistory = useCallback(async () => {
    if (!isOwner) return;

    setHistoryLoading(true);
    try {
      const items = await apiFacade.getProposalHistory(project.id);
      setHistory(items);
    } catch (err) {
      console.error(err);
      setHistory([]);
    } finally {
      setHistoryLoading(false);
    }
  }, [isOwner, project.id]);

  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

  useEffect(() => {
    return () => clearUndoTimers();
  }, []);

  const showUndoWindow = (commandId: string) => {
    clearUndoTimers();
    setLastCommandId(commandId);
    setUndoFading(false);
    setUndoVisible(true);

    hideTimerRef.current = window.setTimeout(() => {
      setUndoFading(true);
      fadeTimerRef.current = window.setTimeout(() => {
        setUndoVisible(false);
        setUndoFading(false);
        setLastCommandId(null);
      }, 300);
    }, 60000);
  };

  const handleAccept = async (proposalId: string) => {
    try {
      const commandId = await apiFacade.acceptProposal(proposalId);
      showUndoWindow(commandId);
      await onRefresh();
      await loadHistory();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Не удалось принять отклик';
      alert(message);
    }
  };

  const handleReject = async (proposalId: string) => {
    try {
      const commandId = await apiFacade.rejectProposal(proposalId);
      showUndoWindow(commandId);
      await onRefresh();
      await loadHistory();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Не удалось отклонить отклик';
      alert(message);
    }
  };

  const handleUndo = async () => {
    if (!lastCommandId) return;

    setUndoLoading(true);
    try {
      await apiFacade.undoProposalCommand(lastCommandId);
      clearUndoTimers();
      setUndoVisible(false);
      setUndoFading(false);
      setLastCommandId(null);
      await onRefresh();
      await loadHistory();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Не удалось отменить действие';
      alert(message);
    } finally {
      setUndoLoading(false);
    }
  };

  if (!isOwner) {
    return null;
  }

  return (
    <section className="mt-8">
      <div
        className={`mb-3 overflow-hidden rounded-lg border border-blue-200 bg-blue-50 p-3 transition-all duration-300 dark:border-blue-900 dark:bg-blue-900/20 ${
          undoVisible && !undoFading ? 'max-h-24 opacity-100' : 'max-h-0 opacity-0 p-0 border-0'
        }`}
      >
        <div className="flex items-center justify-between gap-3 text-sm">
          <span className="text-blue-900 dark:text-blue-200">Последнее действие можно отменить в течение минуты</span>
          <Button variant="ghost" onClick={handleUndo} disabled={undoLoading || !lastCommandId}>
            {undoLoading ? 'Отмена...' : 'Undo'}
          </Button>
        </div>
      </div>

      <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">
        Отклики ({proposals.length})
      </h2>

      {proposals.length === 0 ? (
        <p className="text-sm text-gray-500 dark:text-gray-400">Пока нет откликов</p>
      ) : (
        <div className="space-y-4">
          {proposals.map((proposal) => (
            <div
              key={proposal.id}
              className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-gray-900"
            >
              <div className="flex justify-between items-start">
                <div>
                  <p className="font-medium text-gray-900 dark:text-white">{proposal.freelancerName}</p>
                  <p className="text-gray-600 dark:text-gray-300 whitespace-pre-wrap">{proposal.coverLetter}</p>
                  <p className="text-sm mt-2 text-gray-500">Ставка: ${proposal.bidAmount.toLocaleString()}</p>
                </div>

                {proposal.status === 'Pending' && project.status === 'Open' ? (
                  <div className="ml-4 flex gap-2">
                    <Button variant="primary" onClick={() => handleAccept(proposal.id)}>
                      Принять
                    </Button>
                    <Button variant="ghost" onClick={() => handleReject(proposal.id)}>
                      Отклонить
                    </Button>
                  </div>
                ) : (
                  <span
                    className={`text-sm font-medium ${
                      proposal.status === 'Accepted' ? 'text-green-600' : 'text-red-600'
                    }`}
                  >
                    {proposal.status === 'Accepted' ? 'Принят' : proposal.status === 'Rejected' ? 'Отклонен' : 'В ожидании'}
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mt-6 rounded-xl border border-gray-200 dark:border-gray-700">
        <button
          type="button"
          onClick={() => setHistoryOpen((prev) => !prev)}
          className="flex w-full items-center justify-between px-4 py-3 text-left text-sm font-medium text-gray-800 dark:text-gray-100"
        >
          <span>Активность</span>
          <span className="text-xs text-gray-500 dark:text-gray-400">{history.length}</span>
        </button>

        <div className={`transition-all duration-300 ${historyOpen ? 'max-h-80 overflow-auto p-4 pt-0' : 'max-h-0 overflow-hidden p-0'}`}>
          {historyLoading ? (
            <p className="text-sm text-gray-500 dark:text-gray-400">Загрузка...</p>
          ) : history.length === 0 ? (
            <p className="text-sm text-gray-500 dark:text-gray-400">История пока пуста</p>
          ) : (
            <ul className="space-y-2">
              {history.map((entry) => (
                <li key={entry.commandId} className="rounded-lg bg-gray-50 px-3 py-2 text-sm dark:bg-gray-800">
                  <p className="font-medium text-gray-900 dark:text-gray-100">
                    {actionLabels[entry.commandType] ?? entry.commandType}
                  </p>
                  <p className="text-gray-500 dark:text-gray-400">
                    {new Date(entry.executedAt).toLocaleString('ru-RU')}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
}
