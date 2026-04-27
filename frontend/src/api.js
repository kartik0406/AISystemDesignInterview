const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:8080';

const api = {
  async startInterview(topic, companyMode = 'GENERAL') {
    const res = await fetch(`${API_BASE}/api/v1/interview/start`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ topic, companyMode }),
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },

  async submitAnswer(sessionId, answer) {
    const res = await fetch(`${API_BASE}/api/v1/interview/answer`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sessionId, answer }),
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },

  async getSession(sessionId) {
    const res = await fetch(`${API_BASE}/api/v1/interview/session/${sessionId}`);
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },

  async getResult(sessionId) {
    const res = await fetch(`${API_BASE}/api/v1/interview/result/${sessionId}`);
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },

  async requestHint(sessionId, hintLevel = 1) {
    const res = await fetch(`${API_BASE}/api/v1/interview/hint`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sessionId, hintLevel }),
    });
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },

  async getTopics() {
    const res = await fetch(`${API_BASE}/api/v1/interview/topics`);
    if (!res.ok) throw new Error(await res.text());
    return res.json();
  },
};

export default api;
