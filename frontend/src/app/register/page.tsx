'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';
import { Button } from '@/patterns/ButtonFactory';
import { useThemeFactory } from '@/patterns/ThemeAbstractFactory';
import Link from 'next/link';

export default function RegisterPage() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState<'Customer' | 'Freelancer'>('Freelancer');
  const [error, setError] = useState('');
  const { register, isLoading } = useAuthStore();
  const router = useRouter();
  const factory = useThemeFactory();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await register(email, password, name, role);
      router.push('/dashboard');
    } catch (err: any) {
      setError(err.message || 'Ошибка регистрации');
    }
  }

  return (
    <div className="max-w-md mx-auto mt-16">
      {factory.createCard({
        children: (
          <>
            <h1 className="text-2xl font-bold mb-6 text-center text-gray-900 dark:text-white">
              Регистрация
            </h1>
            {error && (
              <div className="mb-4 p-3 bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 rounded-lg text-sm">
                {error}
              </div>
            )}
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                  Имя
                </label>
                {factory.createInput({
                  type: 'text',
                  value: name,
                  onChange: (e) => setName(e.target.value),
                  required: true,
                  placeholder: 'Ваше имя',
                })}
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                  Email
                </label>
                {factory.createInput({
                  type: 'email',
                  value: email,
                  onChange: (e) => setEmail(e.target.value),
                  required: true,
                  placeholder: 'your@email.com',
                })}
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                  Пароль
                </label>
                {factory.createInput({
                  type: 'password',
                  value: password,
                  onChange: (e) => setPassword(e.target.value),
                  required: true,
                  minLength: 6,
                  placeholder: 'Минимум 6 символов',
                })}
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">
                  Роль
                </label>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="role"
                      value="Freelancer"
                      checked={role === 'Freelancer'}
                      onChange={() => setRole('Freelancer')}
                      className="text-blue-600"
                    />
                    <span className="text-gray-700 dark:text-gray-300">Фрилансер</span>
                  </label>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="role"
                      value="Customer"
                      checked={role === 'Customer'}
                      onChange={() => setRole('Customer')}
                      className="text-blue-600"
                    />
                    <span className="text-gray-700 dark:text-gray-300">Заказчик</span>
                  </label>
                </div>
              </div>
              <Button variant="primary" type="submit" disabled={isLoading} className="w-full">
                {isLoading ? 'Регистрация...' : 'Зарегистрироваться'}
              </Button>
            </form>
            <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
              Уже есть аккаунт?{' '}
              <Link href="/login" className="text-blue-600 dark:text-blue-400 hover:underline">
                Войти
              </Link>
            </p>
          </>
        ),
      })}
    </div>
  );
}
