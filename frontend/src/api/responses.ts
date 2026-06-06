import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from './client';
import type { ResponseSubmitRequest, ResponseDto } from '../types/response';

/**
 * TanStack Query hooks for survey response operations.
 */

const responseKeys = {
  all: ['responses'] as const,
  list: (surveyId: string, page: number, pageSize: number) =>
    [...responseKeys.all, 'list', surveyId, { page, pageSize }] as const,
  detail: (surveyId: string, responseId: string) =>
    [...responseKeys.all, 'detail', surveyId, responseId] as const,
};

/** Fetch paginated responses for a survey. */
export function useResponses(surveyId: string | undefined, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: responseKeys.list(surveyId ?? '', page, pageSize),
    queryFn: async () => {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      const { data } = await apiClient.get(`/surveys/${surveyId}/responses?${params}`);
      return data as { items: ResponseDto[]; totalCount: number; page: number; pageSize: number };
    },
    enabled: !!surveyId,
  });
}

/** Fetch a single response by ID. */
export function useResponse(surveyId: string | undefined, responseId: string | undefined) {
  return useQuery({
    queryKey: responseKeys.detail(surveyId ?? '', responseId ?? ''),
    queryFn: async () => {
      const { data } = await apiClient.get<ResponseDto>(
        `/surveys/${surveyId}/responses/${responseId}`
      );
      return data;
    },
    enabled: !!surveyId && !!responseId,
  });
}

/** Submit a survey response (public endpoint). */
export function useSubmitResponse(publicLinkId: string) {
  return useMutation({
    mutationFn: async (request: ResponseSubmitRequest) => {
      const { data } = await apiClient.post<ResponseDto>(
        `/s/${publicLinkId}/responses`,
        request
      );
      return data;
    },
  });
}
