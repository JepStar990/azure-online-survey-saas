import React from 'react';

/** Placeholder shown when a list or data view has no items. */
export const EmptyState: React.FC<{
  title: string;
  description?: string;
  action?: React.ReactNode;
}> = ({ title, description, action }) => (
  <div style={{
    textAlign: 'center',
    padding: '3rem 1.5rem',
    color: '#64748b',
  }}>
    <h3 style={{ margin: '0 0 0.5rem', color: '#475569' }}>{title}</h3>
    {description && <p style={{ margin: '0 0 1rem' }}>{description}</p>}
    {action}
  </div>
);
