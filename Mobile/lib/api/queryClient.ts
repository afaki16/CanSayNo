// React Query global client. Provider _layout.tsx'te uygulamayı sarar.

import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 1000 * 60 * 5, // 5 dakika
      refetchOnWindowFocus: false, // React Native'de gereksiz
    },
    mutations: {
      retry: 0,
    },
  },
});
