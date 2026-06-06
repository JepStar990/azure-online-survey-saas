export interface AnalyticsSummary {
  surveyId: string;
  surveyTitle: string;
  totalResponses: number;
  totalQuestions: number;
  completionRate: number;
  questionBreakdowns: QuestionAnalytics[];
}

export interface QuestionAnalytics {
  questionId: string;
  questionText: string;
  questionType: string;
  responseCount: number;
  optionCounts: OptionCount[];
  ratingSummary?: RatingSummary;
  textSamples: string[];
}

export interface OptionCount {
  optionId: string;
  optionText: string;
  count: number;
  percentage: number;
}

export interface RatingSummary {
  average: number;
  min: number;
  max: number;
  median: number;
  distribution: Record<number, number>;
}
