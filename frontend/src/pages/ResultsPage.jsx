import { useState, useEffect } from 'react';
import './ResultsPage.css';

function AnimatedScore({ value, max = 10, delay = 0 }) {
  const [display, setDisplay] = useState(0);
  useEffect(() => {
    const timer = setTimeout(() => {
      const duration = 1200;
      const start = performance.now();
      const animate = (now) => {
        const progress = Math.min((now - start) / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
        setDisplay(value * eased);
        if (progress < 1) requestAnimationFrame(animate);
      };
      requestAnimationFrame(animate);
    }, delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  const color = display >= 7 ? 'var(--accent-green)' : display >= 5 ? 'var(--accent-blue)' : display >= 3 ? 'var(--accent-amber)' : 'var(--accent-red)';
  return <span style={{ color, fontFamily: 'var(--font-mono)', fontWeight: 800 }}>{display.toFixed(1)}</span>;
}

function RadialChart({ value, max = 10, size = 120, label }) {
  const pct = (value / max) * 100;
  const radius = (size - 12) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (pct / 100) * circumference;
  const color = value >= 7 ? 'var(--accent-green)' : value >= 5 ? 'var(--accent-blue)' : value >= 3 ? 'var(--accent-amber)' : 'var(--accent-red)';

  return (
    <div className="radial-chart">
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
        <circle cx={size / 2} cy={size / 2} r={radius} fill="none" stroke="var(--bg-tertiary)" strokeWidth="6" />
        <circle
          cx={size / 2} cy={size / 2} r={radius}
          fill="none" stroke={color} strokeWidth="6"
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          transform={`rotate(-90 ${size / 2} ${size / 2})`}
          style={{ transition: 'stroke-dashoffset 1.5s ease-out' }}
        />
      </svg>
      <div className="radial-label">
        <span className="radial-value" style={{ color }}>{value.toFixed(1)}</span>
        <span className="radial-max">/{max}</span>
      </div>
      {label && <span className="radial-name">{label}</span>}
    </div>
  );
}

export default function ResultsPage({ session, onNewInterview }) {
  const result = session.result;
  if (!result) return <div className="results-loading">Generating report...</div>;

  return (
    <div className="results">
      {/* Header */}
      <header className="results-header animate-fade-in">
        <div className="results-badge">📊 Interview Complete</div>
        <h1 className="results-title">{result.topic}</h1>
        <div className="results-meta">
          <span className="badge badge-purple">{result.companyMode}</span>
          <span className="badge badge-blue">{result.totalRounds} Rounds</span>
        </div>
      </header>

      {/* Overall Score */}
      <section className="score-hero glass-card animate-slide-up">
        <RadialChart value={result.overallScore} size={160} />
        <div className="score-hero-info">
          <h2>Overall Score</h2>
          <p className="score-verdict">
            {result.overallScore >= 8 ? '🏆 Excellent! You demonstrated strong system design skills.' :
             result.overallScore >= 6 ? '👍 Good performance. Some areas for improvement.' :
             result.overallScore >= 4 ? '📚 Decent attempt. Focus on the suggested areas.' :
             '💪 Keep practicing! Review the fundamentals.'}
          </p>
        </div>
      </section>

      {/* Rubric Breakdown */}
      {result.aggregatedRubric && Object.keys(result.aggregatedRubric).length > 0 && (
        <section className="rubric-section animate-slide-up">
          <h2 className="section-heading">Rubric Breakdown</h2>
          <div className="rubric-grid">
            {Object.entries(result.aggregatedRubric).map(([key, value], i) => (
              <div key={key} className="rubric-card glass-card">
                <RadialChart value={value} size={80} />
                <span className="rubric-label">{key.replace(/_/g, ' ')}</span>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* Strengths & Weaknesses */}
      <div className="feedback-grid">
        {result.overallStrengths?.length > 0 && (
          <section className="feedback-card glass-card animate-slide-up">
            <h3 className="feedback-title strengths-title">✅ Strengths</h3>
            <ul className="feedback-list">
              {result.overallStrengths.map((s, i) => <li key={i}>{s}</li>)}
            </ul>
          </section>
        )}
        {result.overallWeaknesses?.length > 0 && (
          <section className="feedback-card glass-card animate-slide-up">
            <h3 className="feedback-title weaknesses-title">⚠️ Areas for Improvement</h3>
            <ul className="feedback-list">
              {result.overallWeaknesses.map((w, i) => <li key={i}>{w}</li>)}
            </ul>
          </section>
        )}
      </div>

      {/* Suggestions */}
      {result.improvementSuggestions?.length > 0 && (
        <section className="suggestions-section glass-card animate-slide-up">
          <h3 className="feedback-title">📚 Study Recommendations</h3>
          <ul className="suggestions-list">
            {result.improvementSuggestions.map((s, i) => (
              <li key={i}>{s}</li>
            ))}
          </ul>
        </section>
      )}

      {/* Round-by-Round */}
      <section className="rounds-section animate-slide-up">
        <h2 className="section-heading">Round-by-Round Review</h2>
        {result.rounds?.map((round, i) => (
          <div key={i} className="round-review glass-card">
            <div className="round-review-header">
              <div className="round-review-info">
                <span className="badge badge-blue">Q{round.roundNumber}</span>
                <span className="badge badge-purple">{round.difficulty}</span>
              </div>
              <span className={`round-review-score ${round.score >= 7 ? 'score-good' : round.score >= 5 ? 'score-average' : 'score-poor'}`}>
                <AnimatedScore value={round.score} delay={i * 200} />/10
              </span>
            </div>
            <div className="round-review-qa">
              <div className="qa-block">
                <span className="qa-label">Question</span>
                <p>{round.question}</p>
              </div>
              {round.answer && (
                <div className="qa-block">
                  <span className="qa-label">Your Answer</span>
                  <p>{round.answer}</p>
                </div>
              )}
            </div>
            {(round.strengths?.length > 0 || round.weaknesses?.length > 0) && (
              <div className="round-review-feedback">
                {round.strengths?.length > 0 && (
                  <div className="rf-col">
                    <span className="rf-label">✅ Strengths</span>
                    {round.strengths.map((s, j) => <span key={j} className="rf-item">{s}</span>)}
                  </div>
                )}
                {round.weaknesses?.length > 0 && (
                  <div className="rf-col">
                    <span className="rf-label">⚠️ Weaknesses</span>
                    {round.weaknesses.map((w, j) => <span key={j} className="rf-item">{w}</span>)}
                  </div>
                )}
              </div>
            )}
          </div>
        ))}
      </section>

      {/* Architecture Diagram */}
      {result.architectureDiagram && (
        <section className="diagram-section glass-card animate-slide-up">
          <h3 className="feedback-title">🧱 Architecture Diagram</h3>
          <pre className="diagram-code"><code>{result.architectureDiagram}</code></pre>
        </section>
      )}

      {/* Actions */}
      <div className="results-actions">
        <button className="btn btn-primary btn-lg" onClick={onNewInterview}>
          🚀 Start New Interview
        </button>
      </div>
    </div>
  );
}
