import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { usePublicSurvey } from '../api/surveys';
import { useSubmitResponse } from '../api/responses';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { ErrorMessage } from '../components/shared/ErrorMessage';
import type { AnswerRequest } from '../types/response';

/** Public survey-taking page. Loads survey by publicLinkId and renders questions for response. */
const SurveyTakePage: React.FC = () => {
  const { publicLinkId } = useParams<{ publicLinkId: string }>();
  const navigate = useNavigate();
  const { data: survey, isLoading, error } = usePublicSurvey(publicLinkId);
  const submitResponse = useSubmitResponse(publicLinkId ?? '');

  // Answer state: questionId -> answer data
  const [answers, setAnswers] = useState<Map<string, AnswerRequest>>(new Map());

  const handleTextChange = (questionId: string, value: string) => {
    setAnswers(prev => {
      const next = new Map(prev);
      next.set(questionId, { questionId, value });
      return next;
    });
  };

  const handleRatingChange = (questionId: string, rating: number) => {
    setAnswers(prev => {
      const next = new Map(prev);
      next.set(questionId, { questionId, ratingValue: rating });
      return next;
    });
  };

  const handleChoiceChange = (questionId: string, optionId: string, multi: boolean) => {
    setAnswers(prev => {
      const next = new Map(prev);
      const existing = next.get(questionId);
      if (multi) {
        const currentIds = existing?.selectedOptionIds ?? [];
        const newIds = currentIds.includes(optionId)
          ? currentIds.filter(id => id !== optionId)
          : [...currentIds, optionId];
        next.set(questionId, { questionId, selectedOptionIds: newIds });
      } else {
        next.set(questionId, { questionId, selectedOptionIds: [optionId] });
      }
      return next;
    });
  };

  const handleSubmit = async () => {
    const answerList = Array.from(answers.values());
    submitResponse.mutate({ answers: answerList }, {
      onSuccess: () => navigate(`/s/${publicLinkId}/thanks`),
      onError: (err: any) => alert('Error submitting: ' + (err.response?.data?.error || err.message)),
    });
  };

  if (isLoading) return <LoadingSpinner message="Loading survey..." />;
  if (error || !survey) return <ErrorMessage message="Survey not found or no longer available." />;
  if (!survey.settings.isOpen) return <ErrorMessage message="This survey is no longer accepting responses." />;

  const progress = survey.questions.length > 0
    ? Math.round((answers.size / survey.questions.length) * 100)
    : 0;

  return (
    <div style={{ maxWidth: 720, margin: '0 auto', padding: '2rem 1rem' }}>
      <h1 style={{ fontSize: '1.5rem', marginBottom: '0.25rem' }}>{survey.title}</h1>
      {survey.description && <p style={{ color: '#64748b', marginBottom: '1.5rem' }}>{survey.description}</p>}

      {/* Progress bar */}
      {survey.settings.showProgressBar && (
        <div style={{ marginBottom: '1.5rem' }}>
          <div style={{ height: 6, background: '#e2e8f0', borderRadius: 3, overflow: 'hidden' }}>
            <div style={{ height: '100%', width: `${progress}%`, background: '#3b82f6', borderRadius: 3, transition: 'width 0.3s' }} />
          </div>
          <div style={{ textAlign: 'right', fontSize: '0.8rem', color: '#64748b', marginTop: '0.25rem' }}>{progress}% complete</div>
        </div>
      )}

      {/* Questions */}
      {survey.questions.map(q => (
        <div key={q.id} className="card" style={{ marginBottom: '1rem' }}>
          <p style={{ fontWeight: 500, marginBottom: '0.5rem' }}>
            {q.text}
            {q.isRequired && <span style={{ color: '#dc2626', marginLeft: 4 }}>*</span>}
          </p>
          {q.description && <p style={{ color: '#64748b', fontSize: '0.85rem', marginBottom: '0.75rem' }}>{q.description}</p>}

          {/* Render by question type */}
          {(q.type === 'SingleChoice' || q.type === 'Dropdown') && q.options.map(opt => (
            <label key={opt.id} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', padding: '0.4rem 0', cursor: 'pointer' }}>
              <input type="radio" name={q.id}
                checked={(answers.get(q.id)?.selectedOptionIds ?? []).includes(opt.id)}
                onChange={() => handleChoiceChange(q.id, opt.id, false)} />
              {opt.text}
            </label>
          ))}
          {q.type === 'MultipleChoice' && q.options.map(opt => (
            <label key={opt.id} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', padding: '0.4rem 0', cursor: 'pointer' }}>
              <input type="checkbox"
                checked={(answers.get(q.id)?.selectedOptionIds ?? []).includes(opt.id)}
                onChange={() => handleChoiceChange(q.id, opt.id, true)} />
              {opt.text}
            </label>
          ))}
          {(q.type === 'TextShort' || q.type === 'TextLong') && (
            q.type === 'TextLong' ? (
              <textarea className="form-input" rows={4}
                value={answers.get(q.id)?.value ?? ''}
                onChange={e => handleTextChange(q.id, e.target.value)}
                placeholder="Enter your answer..." />
            ) : (
              <input type="text" className="form-input"
                value={answers.get(q.id)?.value ?? ''}
                onChange={e => handleTextChange(q.id, e.target.value)}
                placeholder="Enter your answer..." />
            )
          )}
          {(q.type === 'Rating' || q.type === 'Nps') && (
            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              {Array.from({ length: (q.settings.maxRating || (q.type === 'Nps' ? 10 : 5)) - (q.settings.minRating || 1) + 1 }, (_, i) => i + (q.settings.minRating || 1)).map(n => (
                <button key={n} onClick={() => handleRatingChange(q.id, n)}
                  style={{
                    width: 40, height: 40, borderRadius: 6, cursor: 'pointer', fontWeight: 600,
                    background: answers.get(q.id)?.ratingValue === n ? '#3b82f6' : '#f1f5f9',
                    color: answers.get(q.id)?.ratingValue === n ? '#fff' : '#475569',
                    border: answers.get(q.id)?.ratingValue === n ? '2px solid #2563eb' : '2px solid #e2e8f0',
                  }}>
                  {n}
                </button>
              ))}
            </div>
          )}
          {q.type === 'Date' && (
            <input type="date" className="form-input"
              value={answers.get(q.id)?.value ?? ''}
              onChange={e => handleTextChange(q.id, e.target.value)} />
          )}
        </div>
      ))}

      <button onClick={handleSubmit} disabled={submitResponse.isPending}
        className="btn btn-primary" style={{ width: '100%', padding: '0.75rem', fontSize: '1rem' }}>
        {submitResponse.isPending ? 'Submitting...' : 'Submit Survey'}
      </button>
    </div>
  );
};

export default SurveyTakePage;
