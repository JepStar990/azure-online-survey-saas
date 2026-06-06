import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type { Survey, SurveyCreateRequest, SurveyUpdateRequest } from '../types/survey';

/**
 * TanStack Query hooks for survey CRUD operations.
 */

// --- Query Keys ---
const surveyKeys = {
  all: ['surveys'] as const,
  list: (page: number, pageSize: number, status?: string) =>
    [...surveyKeys.all, 'list', { page, pageSize, status }] as const,
  detail: (id: string) => [...surveyKeys.all, 'detail', id] as const,
  public: (linkId: string) => [...surveyKeys.all, 'public', linkId] as const,
};

// --- Queries ---

/** Fetch a paginated list of surveys. */
export function useSurveys(page = 1, pageSize = 20, status?: string) {
  return useQuery({
    queryKey: surveyKeys.list(page, pageSize, status),
    queryFn: async () => {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (status) params.set('status', status);
      const { data } = await apiClient.get<PaginatedResult<Survey>>(`/surveys?${params}`);
      return data;
    },
  });
}

/** Fetch a single survey by ID. */
export function useSurvey(id: string | undefined) {
  return useQuery({
    queryKey: surveyKeys.detail(id ?? ''),
    queryFn: async () => {
      const { data } = await apiClient.get<Survey>(`/surveys/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

/** Fetch a published survey by its public link ID (for respondents). */
export function usePublicSurvey(linkId: string | undefined) {
  return useQuery({
    queryKey: surveyKeys.public(linkId ?? ''),
    queryFn: async () => {
      const { data } = await apiClient.get<Survey>(`/s/${linkId}`);
      // The public endpoint is /api/v1/s/{linkId} but stores responses at /api/v1/s/{linkId}/responses
      return data;
    },
    enabled: !!linkId,
  });
}

// --- Mutations ---

/** Create a new survey. */
export function useCreateSurvey() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (request: SurveyCreateRequest) => {
      const { data } = await apiClient.post<Survey>('/surveys', request);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: surveyKeys.all });
    },
  });
}

/** Update an existing survey. */
export function useUpdateSurvey() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...request }: SurveyUpdateRequest & { id: string }) => {
      const { data } = await apiClient.put<Survey>(`/surveys/${id}`, request);
      return data;
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: surveyKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: surveyKeys.all });
    },
  });
}

/** Delete a survey. */
export function useDeleteSurvey() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/surveys/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: surveyKeys.all });
    },
  });
}

/** Publish a survey. */
export function usePublishSurvey() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { data } = await apiClient.post<Survey>(`/surveys/${id}/publish`);
      return data;
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: surveyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: surveyKeys.all });
    },
  });
}

/** Close a survey. */
export function useCloseSurvey() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { data } = await apiClient.post<Survey>(`/surveys/${id}/close`);
      return data;
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: surveyKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: surveyKeys.all });
    },
  });
}

// --- Types ---

interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
