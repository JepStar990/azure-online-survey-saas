import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useSurveys, useDeleteSurvey } from '../api/surveys';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';
import { ErrorMessage } from '../components/shared/ErrorMessage';
import { EmptyState } from '../components/shared/EmptyState';
import { ConfirmDialog } from '../components/shared/ConfirmDialog';

/** List of all surveys with filtering, search, and actions. */
const SurveyListPage: React.FC = () => {
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const { data, isLoading, error, refetch } = useSurveys(page, 20, statusFilter);
  const deleteSurvey = useDeleteSurvey();
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null);

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div>
          <h1>Surveys</h1>
          <p>Manage your surveys and view response data.</p>
        </div>
        <Link to="/surveys/new" className="btn btn-primary">+ New Survey</Link>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        {[undefined, 'Draft', 'Published', 'Closed'].map(s => (
          <button key={s ?? 'all'} onClick={() => { setStatusFilter(s); setPage(1); }}
            className={`btn ${statusFilter === s ? 'btn-primary' : 'btn-secondary'}`}
            style={{ fontSize: '0.8rem' }}>
            {s ?? 'All'}
          </button>
        ))}
      </div>

      {isLoading && <LoadingSpinner />}
      {error && <ErrorMessage message="Failed to load surveys." onRetry={() => refetch()} />}

      {data && data.items.length === 0 && (
        <EmptyState
          title="No surveys found"
          description={statusFilter ? `No surveys with status "${statusFilter}".` : 'Create your first survey to get started.'}
          action={<Link to="/surveys/new" className="btn btn-primary">Create Survey</Link>}
        />
      )}

      {data && data.items.length > 0 && (
        <>
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Questions</th>
                <th>Responses</th>
                <th>Created</th>
                <th>Actions</th>
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
                  <td>{s.questions.length}</td>
                  <td>{s.responseCount > 0 ? (
                    <Link to={`/surveys/${s.id}/results`}>{s.responseCount}</Link>
                  ) : '—'}</td>
                  <td>{new Date(s.createdAt).toLocaleDateString()}</td>
                  <td>
                    <div style={{ display: 'flex', gap: '0.35rem' }}>
                      <Link to={`/surveys/${s.id}/edit`} className="btn btn-secondary" style={{ fontSize: '0.75rem', padding: '0.25rem 0.5rem' }}>Edit</Link>
                      {s.status === 'Draft' && (
                        <button onClick={() => setDeleteTarget(s.id)}
                          className="btn btn-danger" style={{ fontSize: '0.75rem', padding: '0.25rem 0.5rem' }}>
                          Delete
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          {data.totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1rem' }}>
              <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={!data.hasPreviousPage} className="btn btn-secondary">
                Previous
              </button>
              <span style={{ padding: '0.5rem', color: '#64748b', fontSize: '0.875rem' }}>
                Page {data.page} of {data.totalPages}
              </span>
              <button onClick={() => setPage(p => p + 1)} disabled={!data.hasNextPage} className="btn btn-secondary">
                Next
              </button>
            </div>
          )}
        </>
      )}

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Survey"
        message="Are you sure you want to delete this survey? This action cannot be undone."
        confirmLabel="Delete"
        onConfirm={() => {
          if (deleteTarget) {
            deleteSurvey.mutate(deleteTarget, {
              onSuccess: () => setDeleteTarget(null),
            });
          }
        }}
        onCancel={() => setDeleteTarget(null)}
        loading={deleteSurvey.isPending}
      />
    </div>
  );
};

export default SurveyListPage;
