import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSurvey, useCreateSurvey, useUpdateSurvey, usePublishSurvey } from '../api/surveys';
import { QuestionEditor } from '../components/survey-builder/QuestionEditor';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { ErrorMessage } from '../components/shared/ErrorMessage';
import type { QuestionCreateRequest, SurveyCreateRequest, QuestionType } from '../types/survey';

/** Create or edit a survey. Mode determined by presence of :id param. */
const SurveyBuilderPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditing = !!id;

  const { data: existingSurvey, isLoading: loadingSurvey, error: loadError } = useSurvey(id);
  const createSurvey = useCreateSurvey();
  const updateSurvey = useUpdateSurvey();
  const publishSurvey = usePublishSurvey();

  // Form state
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [questions, setQuestions] = useState<QuestionCreateRequest[]>([]);

  // Populate form when editing an existing survey
  useEffect(() => {
    if (existingSurvey) {
      setTitle(existingSurvey.title);
      setDescription(existingSurvey.description ?? '');
      setQuestions(existingSurvey.questions.map((q, i) => ({
        text: q.text,
        description: q.description,
        type: q.type,
        isRequired: q.isRequired,
        sortOrder: i,
        settings: q.settings,
        options: q.options.map((o, oi) => ({
          text: o.text,
          sortOrder: oi,
          value: o.value,
        })),
      })));
    }
  }, [existingSurvey]);

  const handleAddQuestion = () => {
    setQuestions(prev => [...prev, {
      text: '',
      type: 'TextShort' as QuestionType,
      isRequired: true,
      sortOrder: prev.length,
      options: [],
    }]);
  };

  const handleQuestionChange = (index: number, updated: QuestionCreateRequest) => {
    setQuestions(prev => prev.map((q, i) => i === index ? updated : q));
  };

  const handleRemoveQuestion = (index: number) => {
    setQuestions(prev => prev.filter((_, i) => i !== index).map((q, i) => ({ ...q, sortOrder: i })));
  };

  const handleMoveQuestion = (index: number, direction: 'up' | 'down') => {
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= questions.length) return;
    setQuestions(prev => {
      const arr = [...prev];
      [arr[index], arr[newIndex]] = [arr[newIndex], arr[index]];
      return arr.map((q, i) => ({ ...q, sortOrder: i }));
    });
  };

  const handleSave = async () => {
    const payload: SurveyCreateRequest = {
      title,
      description: description || undefined,
      questions: questions.map((q, i) => ({ ...q, sortOrder: i })),
      settings: { allowAnonymous: true, showProgressBar: true },
    };

    if (isEditing && id) {
      updateSurvey.mutate({ id, ...payload }, {
        onSuccess: (data) => {
          alert('Survey updated!');
          navigate(`/surveys/${data.id}/edit`);
        },
        onError: (err: any) => alert('Error: ' + (err.response?.data?.error || err.message)),
      });
    } else {
      createSurvey.mutate(payload, {
        onSuccess: (data) => {
          alert('Survey created!');
          navigate(`/surveys/${data.id}/edit`);
        },
        onError: (err: any) => alert('Error: ' + (err.response?.data?.error || err.message)),
      });
    }
  };

  const handlePublish = async () => {
    if (!id) return;
    publishSurvey.mutate(id, {
      onSuccess: (data) => {
        alert(`Survey published! Share link: ${window.location.origin}/s/${data.publicLinkId}`);
        navigate(`/surveys/${data.id}/results`);
      },
      onError: (err: any) => alert('Error: ' + (err.response?.data?.error || err.message)),
    });
  };

  const isSaving = createSurvey.isPending || updateSurvey.isPending;
  const isPublishing = publishSurvey.isPending;

  if (isEditing && loadingSurvey) return <LoadingSpinner message="Loading survey..." />;
  if (isEditing && loadError) return <ErrorMessage message="Failed to load survey." />;

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <h1>{isEditing ? 'Edit Survey' : 'Create Survey'}</h1>
          <p>{isEditing ? 'Modify your survey questions and settings.' : 'Build a new survey from scratch.'}</p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button onClick={handleSave} disabled={isSaving} className="btn btn-primary">
            {isSaving ? 'Saving...' : isEditing ? 'Save Changes' : 'Save Draft'}
          </button>
          {isEditing && (
            <button onClick={handlePublish} disabled={isPublishing} className="btn btn-success">
              {isPublishing ? 'Publishing...' : 'Publish'}
            </button>
          )}
        </div>
      </div>

      {/* Survey metadata */}
      <div className="card">
        <div className="form-group">
          <label>Survey Title</label>
          <input type="text" className="form-input" value={title}
            onChange={e => setTitle(e.target.value)} placeholder="e.g., Customer Satisfaction Q1 2026" />
        </div>
        <div className="form-group">
          <label>Description (optional)</label>
          <textarea className="form-input" value={description}
            onChange={e => setDescription(e.target.value)} placeholder="Tell respondents what this survey is about..."
            rows={2} />
        </div>
      </div>

      {/* Questions */}
      <h2 style={{ fontSize: '1.1rem', margin: '1.5rem 0 0.75rem' }}>
        Questions ({questions.length})
      </h2>

      {questions.length === 0 && (
        <p style={{ color: '#64748b', textAlign: 'center', padding: '2rem' }}>
          No questions yet. Add at least one question to create your survey.
        </p>
      )}

      {questions.map((q, i) => (
        <QuestionEditor
          key={i}
          question={q}
          index={i}
          onChange={(updated) => handleQuestionChange(i, updated)}
          onRemove={() => handleRemoveQuestion(i)}
          onMoveUp={i > 0 ? () => handleMoveQuestion(i, 'up') : undefined}
          onMoveDown={i < questions.length - 1 ? () => handleMoveQuestion(i, 'down') : undefined}
        />
      ))}

      <button onClick={handleAddQuestion} className="btn btn-secondary" style={{ width: '100%', marginTop: '0.5rem', padding: '0.75rem' }}>
        + Add Question
      </button>
    </div>
  );
};

export default SurveyBuilderPage;
