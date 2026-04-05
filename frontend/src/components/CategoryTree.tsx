'use client';

import { ProjectCategoryNode } from '@/types';

function CategoryNodeView({ node, level = 0 }: { node: ProjectCategoryNode; level?: number }) {
  return (
    <li>
      <div className={`py-1 text-sm ${level === 0 ? 'font-semibold text-gray-900 dark:text-white' : 'text-gray-700 dark:text-gray-300'}`}>
        {node.name}
      </div>
      {node.children.length > 0 && (
        <ul className="pl-4 border-l border-gray-200 dark:border-gray-700 space-y-1">
          {node.children.map((child) => (
            <CategoryNodeView key={`${node.name}-${child.name}`} node={child} level={level + 1} />
          ))}
        </ul>
      )}
    </li>
  );
}

export function CategoryTree({ nodes }: { nodes: ProjectCategoryNode[] }) {
  if (nodes.length === 0) return null;

  return (
    <aside className="mb-8 p-4 rounded-xl bg-white/70 dark:bg-gray-900/60 border border-gray-200 dark:border-gray-700">
      <h2 className="text-lg font-semibold mb-3 text-gray-900 dark:text-white">Категории</h2>
      <ul className="space-y-2">
        {nodes.map((node) => (
          <CategoryNodeView key={node.name} node={node} />
        ))}
      </ul>
    </aside>
  );
}
