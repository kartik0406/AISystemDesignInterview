import { useState } from 'react';
import LandingPage from './pages/LandingPage';
import InterviewPage from './pages/InterviewPage';
import ResultsPage from './pages/ResultsPage';
import './App.css';

function App() {
  const [page, setPage] = useState('landing'); // landing | interview | results
  const [session, setSession] = useState(null);
  const [rounds, setRounds] = useState([]);

  const handleStartInterview = (sessionData) => {
    setSession(sessionData);
    setRounds([{
      roundNumber: 1,
      question: sessionData.question,
      topicArea: sessionData.topicArea,
    }]);
    setPage('interview');
  };

  const handleRoundComplete = (roundData) => {
    setRounds(prev => {
      const updated = [...prev];
      const idx = updated.findIndex(r => r.roundNumber === roundData.roundNumber);
      if (idx >= 0) {
        updated[idx] = { ...updated[idx], ...roundData };
      }
      // Add next round if present
      if (roundData.nextQuestion) {
        updated.push({
          roundNumber: roundData.roundNumber + 1,
          question: roundData.nextQuestion,
          topicArea: roundData.nextTopicArea || '',
        });
      }
      return updated;
    });
    setSession(prev => ({ ...prev, ...roundData.sessionUpdate }));
  };

  const handleInterviewComplete = (resultData) => {
    setSession(prev => ({ ...prev, result: resultData }));
    setPage('results');
  };

  const handleNewInterview = () => {
    setSession(null);
    setRounds([]);
    setPage('landing');
  };

  return (
    <div className="app">
      {page === 'landing' && (
        <LandingPage onStart={handleStartInterview} />
      )}
      {page === 'interview' && session && (
        <InterviewPage
          session={session}
          rounds={rounds}
          onRoundComplete={handleRoundComplete}
          onComplete={handleInterviewComplete}
          onExit={handleNewInterview}
        />
      )}
      {page === 'results' && session && (
        <ResultsPage
          session={session}
          onNewInterview={handleNewInterview}
        />
      )}
    </div>
  );
}

export default App;
