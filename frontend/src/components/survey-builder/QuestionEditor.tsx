import React from 'react';
import type { QuestionCreateRequest, QuestionType } from '../../types/survey';

/** Available question types with labels. */
const QUESTION_TYPES: { value: QuestionType; label: string; description: string }[] = [
  { value: 'TextShort', label: 'Short Text', description: 'Single-line text input' },
  { value: 'TextLong', label: 'Long Text', description: 'Multi-line text area' },
  { value: 'SingleChoice', label: 'Single Choice', description: 'Pick one option' },
  { value: 'MultipleChoice', label: 'Multiple Choice', description: 'Pick multiple options' },
  { value: 'Rating', label: 'Rating', description: 'Star or numeric scale' },
  { value: 'Nps', label: 'NPS', description: 'Net Promoter Score 0-10' },
  { value: 'Dropdown', label: 'Dropdown', description: 'Select from dropdown' },
  { value: 'Date', label: 'Date', description: 'Date picker' },
];

interface Props {
  question: QuestionCreateRequest;
  index: number;
  onChange: (updated: QuestionCreateRequest) => void;
  onRemove: () => void;
  onMoveUp?: () => void;
  onMoveDown?: () => void;
}

/** Editor for a single question: type selector, text, options, settings. */
export const QuestionEditor: React.FC<Props> = ({ question, index, onChange, onRemove, onMoveUp, onMoveDown }) => {
  const isChoiceType = ['SingleChoice', 'MultipleChoice', 'Dropdown', 'Ranking'].includes(question.type);
  const isRatingType = ['Rating', 'Nps'].includes(question.type);
  const isTextType = ['TextShort', 'TextLong'].includes(question.type);

  const handleAddOption = () => {
    const newOption = {
      text: '',
      sortOrder: question.options.length,
    };
    onChange({ ...question, options: [...question.options, newOption] });
  };

  const handleOptionChange = (idx: number, text: string) => {
    const options = question.options.map((o, i) => i === idx ? { ...o, text } : o);
    onChange({ ...question, options });
  };

  const handleRemoveOption = (idx: number) => {
    onChange({ ...question, options: question.options.filter((_, i) => i !== idx) });
  };

  return (
    <div className="card" style={{ borderLeft: '3px solid #3b82f6' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
        <strong style={{ fontSize: '0.9rem' }}>Question {index + 1}</strong>
        <div style={{ display: 'flex', gap: '0.35rem' }}>
          {onMoveUp && <button onClick={onMoveUp} className="btn btn-secondary" style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}>↑</button>}
          {onMoveDown && <button onClick={onMoveDown} className="btn btn-secondary" style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}>↓</button>}
          <button onClick={onRemove} className="btn btn-danger" style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}>✕</button>
        </div>
      </div>

      {/* Question text */}
      <div className="form-group">
        <label>Question Text</label>
        <input
          type="text" className="form-input"
          value={question.text}
          onChange={e => onChange({ ...question, text: e.target.value })}
          placeholder="Enter your question..."
        />
      </div>

      {/* Question type */}
      <div className="form-group">
        <label>Question Type</label>
        <select
          className="form-input"
          value={question.type}
          onChange={e => onChange({
            ...question,
            type: e.target.value as QuestionType,
            options: isChoiceType ? question.options : [],
            settings: isRatingType ? { minRating: 1, maxRating: e.target.value === 'Nps' ? 10 : 5 } : question.settings,
          })}
        >
          {QUESTION_TYPES.map(t => (
            <option key={t.value} value={t.value}>{t.label} — {t.description}</option>
          ))}
        </select>
      </div>

      {/* Required toggle */}
      <div className="form-group">
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
          <input
            type="checkbox"
            checked={question.isRequired}
            onChange={e => onChange({ ...question, isRequired: e.target.checked })}
          />
          Required question
        </label>
      </div>

      {/* Options for choice types */}
      {isChoiceType && (
        <div className="form-group">
          <label>Options</label>
          {question.options.map((opt, i) => (
            <div key={i} style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.35rem' }}>
              <input
                type="text" className="form-input"
                value={opt.text}
                onChange={e => handleOptionChange(i, e.target.value)}
                placeholder={`Option ${i + 1}`}
              />
              <button onClick={() => handleRemoveOption(i)} className="btn btn-secondary"
                style={{ padding: '0.35rem 0.5rem', fontSize: '0.8rem' }}>✕</button>
            </div>
          ))}
          <button onClick={handleAddOption} className="btn btn-secondary" style={{ fontSize: '0.8rem', marginTop: '0.25rem' }}>
            + Add Option
          </button>
        </div>
      )}

      {/* Rating settings */}
      {isRatingType && (
        <div style={{ display: 'flex', gap: '1rem' }}>
          <div className="form-group">
            <label>Min Value</label>
            <input type="number" className="form-input" style={{ width: 80 }}
              value={question.settings?.minRating ?? 1}
              onChange={e => onChange({ ...question, settings: { ...question.settings, minRating: parseInt(e.target.value) || 1 } })} />
          </div>
          <div className="form-group">
            <label>Max Value</label>
            <input type="number" className="form-input" style={{ width: 80 }}
              value={question.settings?.maxRating ?? 5}
              onChange={e => onChange({ ...question, settings: { ...question.settings, maxRating: parseInt(e.target.value) || 5 } })} />
          </div>
        </div>
      )}

      {/* Text settings */}
      {isTextType && (
        <div className="form-group">
          <label>Max Length (characters)</label>
          <input type="number" className="form-input" style={{ width: 120 }}
            value={question.settings?.maxLength ?? 500}
            onChange={e => onChange({ ...question, settings: { ...question.settings, maxLength: parseInt(e.target.value) || undefined } })} />
        </div>
      )}

      {/* Description (optional) */}
      <div className="form-group">
        <label>Description (optional)</label>
        <input
          type="text" className="form-input"
          value={question.description ?? ''}
          onChange={e => onChange({ ...question, description: e.target.value || undefined })}
          placeholder="Additional context or instructions..."
        />
      </div>
    </div>
  );
};
