// React Query global client. Provider _layout.tsx'te uygulamayı sarar.

import { QueryClient } from '@tanstack/react-query';

import { ApiError } from './client';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 dakika
      gcTime: 1000 * 60 * 10, // 10 dakika (garbage collection)
      retry: (failureCount, error) => {
        // 401, 403, 404 retry etme — anlamlı değil.
        if (error instanceof ApiError) {
          if ([401, 403, 404].includes(error.status)) {
            return false;
          }
        }
        return failureCount < 2; // En fazla 2 retry
      },
      refetchOnWindowFocus: false, // Mobile için gereksiz
      refetchOnReconnect: true, // Network geri gelince refetch
    },
    mutations: {
      retry: false, // Mutation'lar otomatik retry etmesin
    },
  },
});
