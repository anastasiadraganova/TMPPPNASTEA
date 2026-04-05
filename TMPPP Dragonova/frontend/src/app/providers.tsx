'use client';

import { ThemeProvider } from 'next-themes';
import { ThemeFactoryProvider } from '@/patterns/ThemeAbstractFactory';

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <ThemeProvider attribute="class" defaultTheme="light" enableSystem>
      <ThemeFactoryProvider>
        {children}
      </ThemeFactoryProvider>
    </ThemeProvider>
  );
}
