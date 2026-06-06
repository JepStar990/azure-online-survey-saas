/** Request to submit survey answers. */
export interface ResponseSubmitRequest {
  answers: AnswerRequest[];
}

export interface AnswerRequest {
  questionId: string;
  value?: string;
  selectedOptionIds?: string[];
  ratingValue?: number;
}

/** Response returned by the API. */
export interface ResponseDto {
  id: string;
  surveyId: string;
  respondentId?: string;
  status: 'InProgress' | 'Submitted';
  startedAt: string;
  completedAt?: string;
  answers: ResponseAnswerDto[];
}

export interface ResponseAnswerDto {
  questionId: string;
  value?: string;
  selectedOptionIds?: string[];
  ratingValue?: number;
  fileUrl?: string;
  answeredAt: string;
}
