import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAnalyticsSummary } from '../api/analytics';
import { useResponses } from '../api/responses';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { ErrorMessage } from '../components/shared/ErrorMessage';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#f97316'];

/** Analytics results page for a survey. */
const ResultsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data: analytics, isLoading, error, refetch } = useAnalyticsSummary(id);
  const { data: responses } = useResponses(id, 1, 20);

  if (isLoading) return <LoadingSpinner message="Loading analytics..." />;
  if (error || !analytics) return <ErrorMessage message="Failed to load analytics." onRetry={() => refetch()} />;

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <h1>Results: {analytics.surveyTitle}</h1>
          <p>Survey analytics and response data.</p>
        </div>
        <Link to={`/surveys/${id}/edit`} className="btn btn-secondary">← Back to Survey</Link>
      </div>

      {/* Summary cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#3b82f6' }}>{analytics.totalResponses}</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Total Responses</div>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#10b981' }}>{analytics.completionRate.toFixed(0)}%</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Completion Rate</div>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#8b5cf6' }}>{analytics.totalQuestions}</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Questions</div>
        </div>
      </div>

      {/* Per-question analytics */}
      {analytics.questionBreakdowns.map((q, qi) => (
        <div key={q.questionId} className="card" style={{ marginBottom: '1rem' }}>
          <h3 style={{ fontSize: '1rem', marginBottom: '0.5rem' }}>
            {qi + 1}. {q.questionText}
            <span style={{ color: '#64748b', fontSize: '0.8rem', marginLeft: '0.5rem' }}>({q.responseCount} responses)</span>
          </h3>

          {/* Choice-based: bar chart */}
          {q.optionCounts.length > 0 && (
            <div style={{ height: 250 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={q.optionCounts} margin={{ top: 10, right: 10, left: 10, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="optionText" tick={{ fontSize: 12 }} />
                  <YAxis tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Bar dataKey="count" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}

          {/* Rating summary */}
          {q.ratingSummary && (
            <div>
              <div style={{ display: 'flex', gap: '1.5rem', marginBottom: '0.75rem' }}>
                <div><strong>Average:</strong> {q.ratingSummary.average}</div>
                <div><strong>Median:</strong> {q.ratingSummary.median}</div>
                <div><strong>Range:</strong> {q.ratingSummary.min}–{q.ratingSummary.max}</div>
              </div>
              {Object.keys(q.ratingSummary.distribution).length > 0 && (
                <div style={{ height: 200 }}>
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={Object.entries(q.ratingSummary.distribution).map(([k, v]) => ({ name: `Rating ${k}`, value: v }))}
                        dataKey="value" nameKey="name"
                        cx="50%" cy="50%" outerRadius={80} label={({ name, value }) => `${name}: ${value}`}>
                        {Object.keys(q.ratingSummary.distribution).map((_, i) => (
                          <Cell key={i} fill={COLORS[i % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              )}
            </div>
          )}

          {/* Text samples */}
          {q.textSamples.length > 0 && (
            <div>
              <p style={{ fontWeight: 500, fontSize: '0.85rem', marginBottom: '0.5rem' }}>Recent responses:</p>
              {q.textSamples.map((sample, i) => (
                <div key={i} style={{ background: '#f8fafc', padding: '0.5rem 0.75rem', borderRadius: 6, marginBottom: '0.35rem', fontSize: '0.875rem', color: '#475569' }}>
                  "{sample}"
                </div>
              ))}
            </div>
          )}
        </div>
      ))}

      {/* Individual responses */}
      {responses && responses.items.length > 0 && (
        <div className="card">
          <h3 style={{ fontSize: '1rem', marginBottom: '0.75rem' }}>Recent Responses</h3>
          <table className="table">
            <thead>
              <tr>
                <th>Response ID</th>
                <th>Status</th>
                <th>Answers</th>
                <th>Submitted</th>
              </tr>
            </thead>
            <tbody>
              {responses.items.map(r => (
                <tr key={r.id}>
                  <td style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>{r.id.substring(0, 8)}...</td>
                  <td><span className={`badge badge-${r.status === 'Submitted' ? 'published' : 'draft'}`}>{r.status}</span></td>
                  <td>{r.answers.length}</td>
                  <td>{r.completedAt ? new Date(r.completedAt).toLocaleString() : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ResultsPage;
