import React from 'react';

/** A simple confirmation dialog for destructive actions. */
export const ConfirmDialog: React.FC<{
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
  loading?: boolean;
}> = ({ open, title, message, confirmLabel = 'Confirm', cancelLabel = 'Cancel', onConfirm, onCancel, loading }) => {
  if (!open) return null;

  return (
    <div style={{
      position: 'fixed', inset: 0,
      background: 'rgba(0,0,0,0.5)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      zIndex: 1000,
    }}>
      <div style={{
        background: '#fff', borderRadius: 8, padding: '1.5rem',
        maxWidth: 420, width: '90%',
        boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
      }}>
        <h3 style={{ margin: '0 0 0.5rem' }}>{title}</h3>
        <p style={{ color: '#64748b', margin: '0 0 1.5rem' }}>{message}</p>
        <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
          <button onClick={onCancel} disabled={loading} style={{
            padding: '0.5rem 1rem', border: '1px solid #d1d5db', borderRadius: 6,
            background: '#fff', cursor: 'pointer',
          }}>
            {cancelLabel}
          </button>
          <button onClick={onConfirm} disabled={loading} style={{
            padding: '0.5rem 1rem', border: 'none', borderRadius: 6,
            background: '#dc2626', color: '#fff', cursor: 'pointer',
            opacity: loading ? 0.7 : 1,
          }}>
            {loading ? 'Processing...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
};
