import { useState } from 'react';
import api from '../api';
import './LandingPage.css';

const TOPICS = [
  { id: 'url-shortener', name: 'URL Shortener', icon: '🔗', desc: 'Design bit.ly' },
  { id: 'twitter', name: 'Twitter / X', icon: '🐦', desc: 'Social media feed' },
  { id: 'netflix', name: 'Netflix', icon: '🎬', desc: 'Video streaming' },
  { id: 'uber', name: 'Uber', icon: '🚗', desc: 'Ride sharing' },
  { id: 'whatsapp', name: 'WhatsApp', icon: '💬', desc: 'Real-time messaging' },
  { id: 'instagram', name: 'Instagram', icon: '📷', desc: 'Photo sharing' },
  { id: 'rate-limiter', name: 'Rate Limiter', icon: '🚦', desc: 'Distributed throttling' },
  { id: 'notification', name: 'Notifications', icon: '🔔', desc: 'Scalable alerts' },
  { id: 'search-engine', name: 'Search Engine', icon: '🔍', desc: 'Web search' },
  { id: 'payment', name: 'Payment System', icon: '💳', desc: 'Payment processing' },
];

const COMPANY_MODES = [
  { id: 'GENERAL', name: 'General', icon: '🎯', desc: 'Balanced evaluation' },
  { id: 'GOOGLE', name: 'Google', icon: '🔵', desc: 'Scalability focus' },
  { id: 'AMAZON', name: 'Amazon', icon: '🟠', desc: 'Trade-offs + LP' },
];

export default function LandingPage({ onStart }) {
  const [selectedTopic, setSelectedTopic] = useState(null);
  const [companyMode, setCompanyMode] = useState('GENERAL');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleStart = async () => {
    if (!selectedTopic) return;
    setLoading(true);
    setError('');
    try {
      const topicName = TOPICS.find(t => t.id === selectedTopic)?.name || selectedTopic;
      const data = await api.startInterview(`Design ${topicName}`, companyMode);
      onStart(data);
    } catch (e) {
      setError('Failed to start interview. Make sure the backend is running.');
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="landing">
      {/* Hero */}
      <header className="landing-hero">
        <div className="hero-badge">
          <span>🧠</span> AI-Powered System Design Interviewer
        </div>
        <h1 className="hero-title">
          Master System Design
          <span className="hero-gradient"> Interviews</span>
        </h1>
        <p className="hero-subtitle">
          Practice with an AI interviewer that adapts to your level.
          Get structured feedback with rubric-based scoring.
        </p>

        <div className="hero-stats">
          <div className="stat">
            <span className="stat-value">10</span>
            <span className="stat-label">Topics</span>
          </div>
          <div className="stat-divider" />
          <div className="stat">
            <span className="stat-value">3</span>
            <span className="stat-label">Company Modes</span>
          </div>
          <div className="stat-divider" />
          <div className="stat">
            <span className="stat-value">6</span>
            <span className="stat-label">Rounds</span>
          </div>
        </div>
      </header>

      {/* Company Mode Selector */}
      <section className="section">
        <h2 className="section-title">Interview Style</h2>
        <div className="company-grid">
          {COMPANY_MODES.map(mode => (
            <button
              key={mode.id}
              className={`company-card glass-card ${companyMode === mode.id ? 'selected' : ''}`}
              onClick={() => setCompanyMode(mode.id)}
            >
              <span className="company-icon">{mode.icon}</span>
              <span className="company-name">{mode.name}</span>
              <span className="company-desc">{mode.desc}</span>
            </button>
          ))}
        </div>
      </section>

      {/* Topic Selector */}
      <section className="section">
        <h2 className="section-title">Choose a Topic</h2>
        <div className="topic-grid">
          {TOPICS.map(topic => (
            <button
              key={topic.id}
              className={`topic-card glass-card ${selectedTopic === topic.id ? 'selected' : ''}`}
              onClick={() => setSelectedTopic(topic.id)}
            >
              <span className="topic-icon">{topic.icon}</span>
              <div className="topic-info">
                <span className="topic-name">{topic.name}</span>
                <span className="topic-desc">{topic.desc}</span>
              </div>
            </button>
          ))}
        </div>
      </section>

      {/* Start Button */}
      <div className="start-section">
        {error && <div className="error-msg">{error}</div>}
        <button
          className="btn btn-primary btn-lg start-btn"
          onClick={handleStart}
          disabled={!selectedTopic || loading}
        >
          {loading ? (
            <>
              <span className="spinner" />
              Starting Interview...
            </>
          ) : (
            <>🚀 Start Interview</>
          )}
        </button>
      </div>
    </div>
  );
}
