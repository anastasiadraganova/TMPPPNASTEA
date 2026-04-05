'use client';

import React from 'react';

/**
 * Паттерн Factory Method — фабрика UI-кнопок.
 *
 * По типу ("primary", "secondary", "danger", "outline", "ghost") возвращает
 * готовый React-компонент кнопки с соответствующими стилями.
 *
 * Это позволяет централизовать создание кнопок и легко менять стили
 * во всём приложении, изменив лишь фабрику.
 */

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'outline' | 'ghost';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  children: React.ReactNode;
}

const baseClasses =
  'inline-flex items-center justify-center px-4 py-2 rounded-lg font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed text-sm';

// ─── Конкретные кнопки (продукты) ───

function PrimaryButton({ children, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`${baseClasses} bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

function SecondaryButton({ children, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`${baseClasses} bg-gray-200 text-gray-800 hover:bg-gray-300 focus:ring-gray-400 dark:bg-gray-700 dark:text-gray-200 dark:hover:bg-gray-600 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

function DangerButton({ children, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`${baseClasses} bg-red-600 text-white hover:bg-red-700 focus:ring-red-500 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

function OutlineButton({ children, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`${baseClasses} border-2 border-blue-600 text-blue-600 hover:bg-blue-50 focus:ring-blue-500 dark:border-blue-400 dark:text-blue-400 dark:hover:bg-blue-900/20 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

function GhostButton({ children, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`${baseClasses} text-gray-600 hover:bg-gray-100 focus:ring-gray-400 dark:text-gray-300 dark:hover:bg-gray-800 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

// ─── Factory Method ───

/**
 * Фабричный метод createButton — возвращает компонент кнопки по типу.
 */
export function createButton(variant: ButtonVariant): React.FC<ButtonProps> {
  switch (variant) {
    case 'primary':
      return PrimaryButton;
    case 'secondary':
      return SecondaryButton;
    case 'danger':
      return DangerButton;
    case 'outline':
      return OutlineButton;
    case 'ghost':
      return GhostButton;
    default:
      return PrimaryButton;
  }
}

/**
 * Удобный компонент-обёртка, использующий фабрику внутри.
 */
export function Button({
  variant = 'primary',
  children,
  ...props
}: ButtonProps & { variant?: ButtonVariant }) {
  const Component = createButton(variant);
  return <Component {...props}>{children}</Component>;
}
