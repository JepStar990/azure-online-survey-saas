import { useQuery } from '@tanstack/react-query';
import { apiClient } from './client';
import type { AnalyticsSummary } from '../types/analytics';

/**
 * TanStack Query hooks for survey analytics.
 */

const analyticsKeys = {
  summary: (surveyId: string) => ['analytics', 'summary', surveyId] as const,
};

/** Fetch analytics summary for a survey. */
export function useAnalyticsSummary(surveyId: string | undefined) {
  return useQuery({
    queryKey: analyticsKeys.summary(surveyId ?? ''),
    queryFn: async () => {
      const { data } = await apiClient.get<AnalyticsSummary>(
        `/surveys/${surveyId}/analytics/summary`
      );
      return data;
    },
    enabled: !!surveyId,
  });
}
