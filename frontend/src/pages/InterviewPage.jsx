import { useState, useRef, useEffect } from 'react';
import api from '../api';
import './InterviewPage.css';

function ScoreBar({ label, value, max = 10 }) {
  const pct = (value / max) * 100;
  const color = value >= 8 ? 'var(--accent-green)' : value >= 6 ? 'var(--accent-blue)' : value >= 4 ? 'var(--accent-amber)' : 'var(--accent-red)';
  return (
    <div className="score-bar-row">
      <span className="score-bar-label">{label}</span>
      <div className="score-bar-track">
        <div className="score-bar-fill" style={{ width: `${pct}%`, background: color }} />
      </div>
      <span className="score-bar-value" style={{ color }}>{value.toFixed(1)}</span>
    </div>
  );
}

export default function InterviewPage({ session, rounds, onRoundComplete, onComplete, onExit }) {
  const [answer, setAnswer] = useState('');
  const [loading, setLoading] = useState(false);
  const [hint, setHint] = useState('');
  const [hintLevel, setHintLevel] = useState(0);
  const [showHint, setShowHint] = useState(false);
  const [latestEval, setLatestEval] = useState(null);
  const chatEndRef = useRef(null);
  const textareaRef = useRef(null);

  const currentRound = rounds[rounds.length - 1];
  const isComplete = session.status === 'COMPLETED';
  const progress = (session.currentRound / session.maxRounds) * 100;

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [rounds, latestEval, loading]);

  const handleSubmit = async () => {
    if (!answer.trim() || loading) return;
    setLoading(true);
    setHint('');
    setShowHint(false);
    setHintLevel(0);

    try {
      const data = await api.submitAnswer(session.sessionId, answer);
      setLatestEval(data.evaluation);

      const roundData = {
        roundNumber: session.currentRound,
        answer: answer,
        evaluation: data.evaluation,
        nextQuestion: data.question,
        nextTopicArea: data.topicArea,
        sessionUpdate: {
          currentRound: data.currentRound,
          difficulty: data.difficulty,
          status: data.status,
          maxRounds: data.maxRounds,
        },
      };

      onRoundComplete(roundData);
      setAnswer('');

      if (data.status === 'COMPLETED' || data.isLastRound) {
        const result = await api.getResult(session.sessionId);
        onComplete(result);
      }
    } catch (e) {
      console.error('Submit failed:', e);
    } finally {
      setLoading(false);
    }
  };

  const handleHint = async () => {
    const nextLevel = Math.min(hintLevel + 1, 3);
    try {
      const data = await api.requestHint(session.sessionId, nextLevel);
      setHint(data.hint);
      setHintLevel(nextLevel);
      setShowHint(true);
    } catch (e) {
      console.error('Hint failed:', e);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) {
      handleSubmit();
    }
  };

  const getDifficultyBadge = (diff) => {
    const map = {
      BEGINNER: ['badge-green', '🟢'],
      EASY: ['badge-green', '🟡'],
      MEDIUM: ['badge-blue', '🔵'],
      HARD: ['badge-amber', '🟠'],
      EXPERT: ['badge-red', '🔴'],
    };
    const [cls, icon] = map[diff] || ['badge-blue', '🔵'];
    return <span className={`badge ${cls}`}>{icon} {diff}</span>;
  };

  return (
    <div className="interview-layout">
      {/* Sidebar */}
      <aside className="interview-sidebar">
        <div className="sidebar-header">
          <button className="btn btn-ghost" onClick={onExit}>← Exit</button>
          <h2 className="sidebar-topic">{session.topic}</h2>
          <span className="badge badge-purple">{session.companyMode}</span>
        </div>

        <div className="sidebar-progress">
          <div className="progress-label">
            <span>Round {session.currentRound} / {session.maxRounds}</span>
            {getDifficultyBadge(session.difficulty)}
          </div>
          <div className="progress-bar">
            <div className="progress-bar-fill" style={{ width: `${progress}%` }} />
          </div>
        </div>

        {/* Round History */}
        <div className="round-history">
          <h3 className="sidebar-section-title">Rounds</h3>
          {rounds.map((r, i) => (
            <div key={i} className={`round-item ${r.evaluation ? 'completed' : i === rounds.length - 1 ? 'active' : ''}`}>
              <div className="round-num">Q{r.roundNumber}</div>
              <div className="round-info">
                <span className="round-area">{r.topicArea || 'General'}</span>
                {r.evaluation && (
                  <span className={`round-score ${r.evaluation.score >= 7 ? 'score-good' : r.evaluation.score >= 5 ? 'score-average' : 'score-poor'}`}>
                    {r.evaluation.score.toFixed(1)}/10
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* Latest Evaluation */}
        {latestEval && (
          <div className="eval-panel glass-card animate-fade-in">
            <h3 className="eval-title">Latest Score</h3>
            <div className="eval-score-big">
              <span className={latestEval.score >= 7 ? 'score-good' : latestEval.score >= 5 ? 'score-average' : 'score-poor'}>
                {latestEval.score.toFixed(1)}
              </span>
              <span className="eval-max">/ 10</span>
            </div>
            {latestEval.rubricBreakdown && (
              <div className="eval-rubric">
                {Object.entries(latestEval.rubricBreakdown).map(([k, v]) => (
                  <ScoreBar key={k} label={k.replace(/_/g, ' ')} value={v} />
                ))}
              </div>
            )}
          </div>
        )}
      </aside>

      {/* Main Chat Area */}
      <main className="interview-main">
        <div className="chat-container">
          {rounds.map((round, i) => (
            <div key={i} className="round-block animate-fade-in">
              {/* Question */}
              <div className="message message-ai">
                <div className="message-avatar">🧠</div>
                <div className="message-content">
                  <div className="message-meta">
                    <span className="badge badge-blue">Q{round.roundNumber}</span>
                    {round.topicArea && <span className="badge badge-purple">{round.topicArea}</span>}
                  </div>
                  <p className="message-text">{round.question}</p>
                </div>
              </div>

              {/* Answer */}
              {round.answer && (
                <div className="message message-user">
                  <div className="message-content">
                    <p className="message-text">{round.answer}</p>
                  </div>
                  <div className="message-avatar">👤</div>
                </div>
              )}

              {/* Evaluation inline */}
              {round.evaluation && (
                <div className="message message-eval animate-slide-up">
                  <div className="message-avatar">📊</div>
                  <div className="message-content eval-inline">
                    <div className="eval-header">
                      <span className={`eval-score-inline ${round.evaluation.score >= 7 ? 'score-good' : round.evaluation.score >= 5 ? 'score-average' : 'score-poor'}`}>
                        Score: {round.evaluation.score.toFixed(1)}/10
                      </span>
                    </div>
                    {round.evaluation.strengths?.length > 0 && (
                      <div className="eval-section">
                        <span className="eval-label">✅ Strengths</span>
                        <ul>{round.evaluation.strengths.map((s, j) => <li key={j}>{s}</li>)}</ul>
                      </div>
                    )}
                    {round.evaluation.weaknesses?.length > 0 && (
                      <div className="eval-section">
                        <span className="eval-label">⚠️ Weaknesses</span>
                        <ul>{round.evaluation.weaknesses.map((w, j) => <li key={j}>{w}</li>)}</ul>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          ))}

          {/* Typing indicator */}
          {loading && (
            <div className="message message-ai animate-fade-in">
              <div className="message-avatar">🧠</div>
              <div className="message-content">
                <div className="typing-indicator">
                  <span /><span /><span />
                </div>
                <span className="typing-text">Evaluating your answer...</span>
              </div>
            </div>
          )}

          <div ref={chatEndRef} />
        </div>

        {/* Input Area */}
        {!isComplete && (
          <div className="input-area">
            {showHint && hint && (
              <div className="hint-panel glass-card animate-fade-in">
                <div className="hint-header">
                  <span>💡 Hint (Level {hintLevel}/3)</span>
                  <button className="btn btn-ghost" onClick={() => setShowHint(false)}>✕</button>
                </div>
                <p className="hint-text">{hint}</p>
              </div>
            )}
            <div className="input-row">
              <textarea
                ref={textareaRef}
                className="answer-input"
                value={answer}
                onChange={e => setAnswer(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Type your answer... (Cmd+Enter to submit)"
                disabled={loading}
              />
            </div>
            <div className="input-actions">
              <button className="btn btn-secondary" onClick={handleHint} disabled={loading || hintLevel >= 3}>
                💡 {hintLevel > 0 ? `Hint ${hintLevel}/3` : 'Get Hint'}
              </button>
              <div className="input-meta">
                <span className="char-count">{answer.length} chars</span>
              </div>
              <button className="btn btn-primary" onClick={handleSubmit} disabled={!answer.trim() || loading}>
                {loading ? <span className="spinner" /> : '📤'} Submit Answer
              </button>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
