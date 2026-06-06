/** Mirrors backend SurveyStatus enum. */
export type SurveyStatus = 'Draft' | 'Published' | 'Closed';

/** Flattened survey settings. */
export interface SurveySettings {
  startDate?: string;
  endDate?: string;
  allowAnonymous: boolean;
  responseLimit?: number;
  thankYouMessage?: string;
  showProgressBar: boolean;
  randomizeQuestions: boolean;
  isOpen: boolean;
}

/** Question option within a choice-based question. */
export interface QuestionOption {
  id: string;
  text: string;
  sortOrder: number;
  value?: string;
}

/** Question types supported by the survey builder. */
export type QuestionType =
  | 'SingleChoice'
  | 'MultipleChoice'
  | 'Rating'
  | 'Nps'
  | 'TextShort'
  | 'TextLong'
  | 'Date'
  | 'Dropdown'
  | 'Ranking'
  | 'FileUpload';

/** Question settings. */
export interface QuestionSettings {
  minRating: number;
  maxRating: number;
  minLabel?: string;
  maxLabel?: string;
  maxLength?: number;
  placeholder?: string;
  randomizeOptions: boolean;
  allowOther: boolean;
  allowedFileTypes?: string;
  maxFileSizeBytes?: number;
}

/** A question within a survey. */
export interface Question {
  id: string;
  text: string;
  description?: string;
  type: QuestionType;
  sortOrder: number;
  isRequired: boolean;
  settings: QuestionSettings;
  options: QuestionOption[];
}

/** Full survey object returned by the API. */
export interface Survey {
  id: string;
  title: string;
  description?: string;
  status: SurveyStatus;
  publicLinkId?: string;
  publicLinkUrl?: string;
  settings: SurveySettings;
  questions: Question[];
  responseCount: number;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
}

// --- Request types ---

export interface QuestionOptionRequest {
  text: string;
  sortOrder: number;
  value?: string;
}

export interface QuestionSettingsRequest {
  minRating?: number;
  maxRating?: number;
  minLabel?: string;
  maxLabel?: string;
  maxLength?: number;
  placeholder?: string;
  randomizeOptions?: boolean;
  allowOther?: boolean;
}

export interface QuestionCreateRequest {
  text: string;
  description?: string;
  type: string;
  isRequired: boolean;
  sortOrder: number;
  settings?: QuestionSettingsRequest;
  options: QuestionOptionRequest[];
}

export interface SurveySettingsRequest {
  startDate?: string;
  endDate?: string;
  allowAnonymous?: boolean;
  responseLimit?: number;
  thankYouMessage?: string;
  showProgressBar?: boolean;
  randomizeQuestions?: boolean;
}

export interface SurveyCreateRequest {
  title: string;
  description?: string;
  questions: QuestionCreateRequest[];
  settings?: SurveySettingsRequest;
}

export interface SurveyUpdateRequest {
  title: string;
  description?: string;
  questions: QuestionCreateRequest[];
  settings?: SurveySettingsRequest;
}
