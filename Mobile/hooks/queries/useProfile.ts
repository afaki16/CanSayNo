// Hesap bilgileri: sunucudaki güncel kullanıcı (GET /api/Auth/me) ve profil
// güncelleme (PUT /api/Auth/profile). Başarıda saklı kullanıcı (ad/soyad) da
// güncellenir; Anasayfa selamlaması ve Profil kartı hemen yenilenir.

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { authService, toStoredUser } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import type { ApiUser, UpdateProfileRequest } from '@/types/api/auth';

export const profileKeys = {
  all: ['profile'] as const,
  me: () => [...profileKeys.all, 'me'] as const,
};

export function useMe() {
  return useQuery<ApiUser, ApiError>({
    queryKey: profileKeys.me(),
    queryFn: () => authService.me(),
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();
  const updateUser = useAuthStore((state) => state.updateUser);

  return useMutation<ApiUser, ApiError, UpdateProfileRequest>({
    mutationFn: async (body) => {
      await authService.updateProfile(body);
      // Sunucunun normalize ettiği değerleri (trim vb.) almak için tazele.
      return authService.me();
    },
    onSuccess: async (user) => {
      queryClient.setQueryData(profileKeys.me(), user);
      await updateUser(toStoredUser(user));
    },
  });
}
