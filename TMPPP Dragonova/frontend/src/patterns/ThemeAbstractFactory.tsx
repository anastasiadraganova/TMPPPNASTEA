'use client';

import React, { createContext, useContext } from 'react';
import { useTheme } from 'next-themes';

/**
 * Паттерн Abstract Factory — создание семейства UI-компонентов для разных тем.
 *
 * ThemeFactory — абстрактная фабрика, определяющая методы создания
 * тематизированных компонентов: кнопки, карточки, текста.
 *
 * LightThemeFactory и DarkThemeFactory — конкретные фабрики,
 * возвращающие компоненты со светлой и тёмной цветовой схемой соответственно.
 *
 * Переключение темы в UI автоматически переключает фабрику.
 */

// ─── Интерфейсы продуктов ───

interface ThemedComponentProps {
  children: React.ReactNode;
  className?: string;
}

// ─── Абстрактная фабрика ───

export interface ThemeFactory {
  createButton: (props: ThemedComponentProps & React.ButtonHTMLAttributes<HTMLButtonElement>) => React.ReactElement;
  createCard: (props: ThemedComponentProps) => React.ReactElement;
  createText: (props: ThemedComponentProps) => React.ReactElement;
  createBadge: (props: ThemedComponentProps) => React.ReactElement;
  createInput: (props: React.InputHTMLAttributes<HTMLInputElement>) => React.ReactElement;
}

// ─── Светлая тема (конкретная фабрика) ───

const LightThemeFactory: ThemeFactory = {
  createButton: ({ children, className = '', ...props }) => (
    <button
      className={`px-4 py-2 bg-white text-gray-900 border border-gray-300 rounded-lg hover:bg-gray-50 shadow-sm transition-all ${className}`}
      {...props}
    >
      {children}
    </button>
  ),

  createCard: ({ children, className = '' }) => (
    <div className={`bg-white border border-gray-200 rounded-xl shadow-sm p-6 ${className}`}>
      {children}
    </div>
  ),

  createText: ({ children, className = '' }) => (
    <p className={`text-gray-700 ${className}`}>{children}</p>
  ),

  createBadge: ({ children, className = '' }) => (
    <span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 ${className}`}>
      {children}
    </span>
  ),

  createInput: ({ className = '', ...props }) => (
    <input
      className={`w-full px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${className}`}
      {...props}
    />
  ),
};

// ─── Тёмная тема (конкретная фабрика) ───

const DarkThemeFactory: ThemeFactory = {
  createButton: ({ children, className = '', ...props }) => (
    <button
      className={`px-4 py-2 bg-gray-800 text-gray-100 border border-gray-600 rounded-lg hover:bg-gray-700 shadow-sm transition-all ${className}`}
      {...props}
    >
      {children}
    </button>
  ),

  createCard: ({ children, className = '' }) => (
    <div className={`bg-gray-800 border border-gray-700 rounded-xl shadow-lg p-6 ${className}`}>
      {children}
    </div>
  ),

  createText: ({ children, className = '' }) => (
    <p className={`text-gray-300 ${className}`}>{children}</p>
  ),

  createBadge: ({ children, className = '' }) => (
    <span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-700 text-gray-200 ${className}`}>
      {children}
    </span>
  ),

  createInput: ({ className = '', ...props }) => (
    <input
      className={`w-full px-3 py-2 border border-gray-600 rounded-lg bg-gray-800 text-gray-100 focus:ring-2 focus:ring-blue-400 focus:border-blue-400 ${className}`}
      {...props}
    />
  ),
};

// ─── Context для доступа к текущей фабрике ───

const ThemeFactoryContext = createContext<ThemeFactory>(LightThemeFactory);

export function useThemeFactory(): ThemeFactory {
  return useContext(ThemeFactoryContext);
}

/**
 * Provider, который автоматически выбирает фабрику (Light/Dark)
 * в зависимости от текущей темы (next-themes).
 */
export function ThemeFactoryProvider({ children }: { children: React.ReactNode }) {
  const { resolvedTheme } = useTheme();
  const factory = resolvedTheme === 'dark' ? DarkThemeFactory : LightThemeFactory;

  return (
    <ThemeFactoryContext.Provider value={factory}>
      {children}
    </ThemeFactoryContext.Provider>
  );
}
