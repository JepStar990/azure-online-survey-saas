import React from 'react';
import { useParams, Link } from 'react-router-dom';

/** Thank-you page shown after survey submission. */
const SurveyThankYouPage: React.FC = () => {
  const { publicLinkId } = useParams<{ publicLinkId: string }>();

  return (
    <div style={{
      display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
      minHeight: '100vh', textAlign: 'center', padding: '2rem',
    }}>
      <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🎉</div>
      <h1 style={{ fontSize: '1.75rem', color: '#0f172a', marginBottom: '0.5rem' }}>
        Thank You!
      </h1>
      <p style={{ color: '#64748b', maxWidth: 400, marginBottom: '1.5rem' }}>
        Your response has been recorded. We appreciate your time and feedback.
      </p>
      {publicLinkId && (
        <Link to={`/s/${publicLinkId}`} className="btn btn-primary">
          Submit Another Response
        </Link>
      )}
    </div>
  );
};

export default SurveyThankYouPage;
