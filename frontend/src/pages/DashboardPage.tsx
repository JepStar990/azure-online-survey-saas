import React from 'react';
import { Link } from 'react-router-dom';
import { useSurveys } from '../api/surveys';
import { useAuth } from '../auth/useAuth';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { ErrorMessage } from '../components/shared/ErrorMessage';

/** Dashboard: overview of surveys and quick actions. */
const DashboardPage: React.FC = () => {
  const { displayName } = useAuth();
  const { data, isLoading, error, refetch } = useSurveys(1, 5);

  const publishedCount = data?.items.filter(s => s.status === 'Published').length ?? 0;
  const draftCount = data?.items.filter(s => s.status === 'Draft').length ?? 0;

  return (
    <div>
      <div className="page-header">
        <h1>Welcome, {displayName}</h1>
        <p>Here's an overview of your surveys.</p>
      </div>

      {/* Summary cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#3b82f6' }}>{data?.totalCount ?? '-'}</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Total Surveys</div>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#16a34a' }}>{publishedCount}</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Published</div>
        </div>
        <div className="card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#f59e0b' }}>{draftCount}</div>
          <div style={{ color: '#64748b', fontSize: '0.875rem' }}>Drafts</div>
        </div>
      </div>

      {/* Recent surveys */}
      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h2 style={{ fontSize: '1.1rem' }}>Recent Surveys</h2>
          <Link to="/surveys/new" className="btn btn-primary">+ New Survey</Link>
        </div>

        {isLoading && <LoadingSpinner message="Loading surveys..." />}
        {error && <ErrorMessage message="Failed to load surveys." onRetry={() => refetch()} />}

        {data && data.items.length === 0 && (
          <p style={{ color: '#64748b', textAlign: 'center', padding: '2rem' }}>
            No surveys yet. Create your first survey to get started.
          </p>
        )}

        {data && data.items.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Responses</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map(s => (
                <tr key={s.id}>
                  <td>
                    <Link to={`/surveys/${s.id}/edit`} style={{ fontWeight: 500 }}>
                      {s.title}
                    </Link>
                  </td>
                  <td><span className={`badge badge-${s.status.toLowerCase()}`}>{s.status}</span></td>
                  <td>{s.responseCount}</td>
                  <td>{new Date(s.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {data && data.totalCount > 5 && (
          <Link to="/surveys" style={{ display: 'block', textAlign: 'center', marginTop: '0.75rem', fontSize: '0.875rem' }}>
            View all surveys →
          </Link>
        )}
      </div>
    </div>
  );
};

export default DashboardPage;
