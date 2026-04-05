'use client';

import Link from 'next/link';
import { useAuthStore } from '@/store/authStore';
import { ThemeToggle } from './ThemeToggle';
import { Button } from '@/patterns/ButtonFactory';

export function Navbar() {
  const { user, logout } = useAuthStore();

  return (
    <nav className="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16 items-center">
          <div className="flex items-center space-x-8">
            <Link href="/" className="text-xl font-bold text-blue-600 dark:text-blue-400">
              FreelanceMarket
            </Link>
            <Link href="/" className="text-gray-600 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400">
              Проекты
            </Link>
          </div>

          <div className="flex items-center space-x-4">
            <ThemeToggle />
            {user ? (
              <>
                <Link href="/dashboard">
                  <Button variant="ghost">{user.name} ({user.role === 'Customer' ? 'Заказчик' : 'Фрилансер'})</Button>
                </Link>
                <Button variant="outline" onClick={logout}>
                  Выйти
                </Button>
              </>
            ) : (
              <>
                <Link href="/login">
                  <Button variant="outline">Войти</Button>
                </Link>
                <Link href="/register">
                  <Button variant="primary">Регистрация</Button>
                </Link>
              </>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
