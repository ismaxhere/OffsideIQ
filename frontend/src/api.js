const BASE = '/api';

function getToken() {
  return localStorage.getItem('offsideiq_token');
}

async function request(path, options = {}) {
  const token = getToken();
  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (res.status === 204) return null;
  const data = await res.json();
  if (!res.ok) throw new Error(data.message || `HTTP ${res.status}`);
  return data;
}

export const api = {
  post: (path, body) => request(path, { method: 'POST', body: JSON.stringify(body) }),
  get: (path) => request(path),
  put: (path, body) => request(path, { method: 'PUT', body: JSON.stringify(body) }),
  del: (path) => request(path, { method: 'DELETE' }),
};

export const authApi = {
  login: (body) => api.post('/auth/login', body),
  register: (body) => api.post('/auth/register', body),
};

export const dashboardApi = {
  get: () => api.get('/dashboard'),
};

export const teamsApi = {
  list: () => api.get('/teams'),
  get: (id) => api.get(`/teams/${id}`),
  form: (id) => api.get(`/teams/${id}/form`),
  create: (body) => api.post('/teams', body),
  update: (id, body) => api.put(`/teams/${id}`, body),
  delete: (id) => api.del(`/teams/${id}`),
};

export const matchesApi = {
  list: (page = 1, pageSize = 20) => api.get(`/matches?page=${page}&pageSize=${pageSize}`),
  recent: (take = 10) => api.get(`/matches/recent?take=${take}`),
  get: (id) => api.get(`/matches/${id}`),
  insights: (id) => api.get(`/matches/${id}/insights`),
  create: (body) => api.post('/matches', body),
  update: (id, body) => api.put(`/matches/${id}`, body),
  delete: (id) => api.del(`/matches/${id}`),
  upsertStats: (id, body) => api.put(`/matches/${id}/stats`, body),
  notes: (id) => api.get(`/matches/${id}/notes`),
  addNote: (id, body) => api.post(`/matches/${id}/notes`, body),
};

export const insightsApi = {
  global: () => api.get('/insights'),
  forTeam: (id) => api.get(`/insights/teams/${id}`),
  predict: (homeId, awayId) => api.get(`/insights/predict?homeTeamId=${homeId}&awayTeamId=${awayId}`),
};

export const h2hApi = {
  get: (aId, bId) => api.get(`/h2h/${aId}/${bId}`),
};

export const playersApi = {
  byTeam: (teamId) => api.get(`/players/team/${teamId}`),
  get: (id) => api.get(`/players/${id}`),
  create: (body) => api.post('/players', body),
};
