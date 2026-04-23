import { useState, useEffect, useCallback } from 'react';
import { BrowserRouter, Routes, Route, Link, useNavigate, useParams, Navigate } from 'react-router-dom';
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer,
  RadarChart, PolarGrid, PolarAngleAxis, Radar, LineChart, Line, CartesianGrid
} from 'recharts';
import { useAuthStore } from './store';
import { authApi, dashboardApi, teamsApi, matchesApi, insightsApi, h2hApi } from './api';

// ─────────────────────────────────────────────────────────────────────────────
// DESIGN TOKENS
// ─────────────────────────────────────────────────────────────────────────────
const css = `
  @import url('https://fonts.googleapis.com/css2?family=Barlow+Condensed:ital,wght@0,400;0,600;0,700;0,800;1,700&family=DM+Sans:wght@400;500;600&display=swap');

  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

  :root {
    --bg:        #0a0c10;
    --surface:   #111418;
    --border:    #1e2228;
    --border2:   #2a2f38;
    --text:      #e8eaf0;
    --muted:     #6b7280;
    --accent:    #00e676;
    --accent2:   #00bcd4;
    --danger:    #ff5252;
    --warn:      #ffb300;
    --positive:  #69f0ae;
    --font-head: 'Barlow Condensed', sans-serif;
    --font-body: 'DM Sans', sans-serif;
    --r:         8px;
  }

  body {
    background: var(--bg);
    color: var(--text);
    font-family: var(--font-body);
    font-size: 14px;
    line-height: 1.6;
    min-height: 100vh;
  }

  a { color: inherit; text-decoration: none; }
  button { cursor: pointer; font-family: inherit; }
  input, select, textarea {
    font-family: inherit;
    background: var(--surface);
    border: 1px solid var(--border2);
    color: var(--text);
    border-radius: var(--r);
    padding: 9px 12px;
    font-size: 14px;
    width: 100%;
    transition: border-color .15s;
    outline: none;
  }
  input:focus, select:focus, textarea:focus { border-color: var(--accent); }

  .layout { display: flex; min-height: 100vh; }

  /* Sidebar */
  .sidebar {
    width: 220px;
    min-height: 100vh;
    background: var(--surface);
    border-right: 1px solid var(--border);
    display: flex;
    flex-direction: column;
    padding: 0;
    position: sticky;
    top: 0;
    height: 100vh;
    overflow-y: auto;
    flex-shrink: 0;
  }
  .sidebar-logo {
    padding: 24px 20px 16px;
    border-bottom: 1px solid var(--border);
  }
  .sidebar-logo span {
    font-family: var(--font-head);
    font-size: 22px;
    font-weight: 800;
    letter-spacing: .5px;
    color: var(--accent);
  }
  .sidebar-logo small { display: block; color: var(--muted); font-size: 11px; margin-top: 1px; }
  .sidebar nav { padding: 12px 0; flex: 1; }
  .nav-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 20px;
    color: var(--muted);
    font-size: 13px;
    font-weight: 500;
    border-left: 2px solid transparent;
    transition: all .15s;
  }
  .nav-item:hover { color: var(--text); background: rgba(255,255,255,.03); }
  .nav-item.active { color: var(--accent); border-left-color: var(--accent); background: rgba(0,230,118,.05); }
  .nav-icon { font-size: 16px; width: 20px; text-align: center; }
  .sidebar-user {
    padding: 14px 20px;
    border-top: 1px solid var(--border);
    display: flex;
    align-items: center;
    gap: 10px;
  }
  .avatar {
    width: 32px; height: 32px;
    border-radius: 50%;
    background: linear-gradient(135deg, var(--accent), var(--accent2));
    display: flex; align-items: center; justify-content: center;
    font-family: var(--font-head);
    font-weight: 700;
    font-size: 13px;
    color: #000;
    flex-shrink: 0;
  }
  .sidebar-user-info { flex: 1; overflow: hidden; }
  .sidebar-user-info strong { display: block; font-size: 13px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .sidebar-user-info span { color: var(--muted); font-size: 11px; }
  .btn-logout {
    background: none; border: none;
    color: var(--muted); font-size: 16px;
    padding: 4px; border-radius: 4px;
    transition: color .15s;
  }
  .btn-logout:hover { color: var(--danger); }

  /* Main content */
  .main { flex: 1; overflow-x: hidden; }
  .page { padding: 32px; max-width: 1200px; }
  .page-header {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    margin-bottom: 28px;
    gap: 16px;
  }
  .page-title {
    font-family: var(--font-head);
    font-size: 32px;
    font-weight: 800;
    letter-spacing: .3px;
    line-height: 1;
  }
  .page-subtitle { color: var(--muted); font-size: 13px; margin-top: 4px; }

  /* Cards */
  .card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 20px;
  }
  .card-title {
    font-family: var(--font-head);
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 1px;
    text-transform: uppercase;
    color: var(--muted);
    margin-bottom: 16px;
  }

  /* Grid */
  .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
  .grid-3 { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
  .grid-4 { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
  .gap-16 { display: flex; flex-direction: column; gap: 16px; }

  /* Stat cards */
  .stat-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 20px;
  }
  .stat-label { color: var(--muted); font-size: 11px; font-weight: 600; letter-spacing: .8px; text-transform: uppercase; }
  .stat-value {
    font-family: var(--font-head);
    font-size: 38px;
    font-weight: 800;
    line-height: 1.1;
    margin-top: 4px;
    color: var(--accent);
  }
  .stat-sub { color: var(--muted); font-size: 12px; margin-top: 2px; }

  /* Match card */
  .match-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 16px 20px;
    display: flex;
    align-items: center;
    gap: 16px;
    transition: border-color .15s;
    cursor: pointer;
  }
  .match-card:hover { border-color: var(--border2); }
  .match-teams { display: flex; align-items: center; gap: 12px; flex: 1; min-width: 0; }
  .team-name { font-weight: 600; font-size: 15px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .team-name.home { text-align: right; flex: 1; }
  .team-name.away { flex: 1; }
  .score-block {
    font-family: var(--font-head);
    font-size: 24px;
    font-weight: 800;
    letter-spacing: 2px;
    min-width: 72px;
    text-align: center;
    color: var(--text);
  }
  .score-block.live { color: var(--accent); }
  .match-meta { color: var(--muted); font-size: 12px; }
  .match-badge {
    padding: 3px 8px;
    border-radius: 4px;
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: .5px;
  }
  .badge-completed { background: rgba(105,240,174,.1); color: var(--positive); }
  .badge-live { background: rgba(255,82,82,.15); color: var(--danger); animation: pulse 1.5s infinite; }
  .badge-scheduled { background: rgba(107,114,128,.1); color: var(--muted); }
  @keyframes pulse { 0%,100% { opacity: 1; } 50% { opacity: .6; } }

  /* Form dots */
  .form-dots { display: flex; gap: 4px; }
  .form-dot {
    width: 22px; height: 22px;
    border-radius: 50%;
    font-size: 10px;
    font-weight: 700;
    display: flex; align-items: center; justify-content: center;
    color: #000;
  }
  .form-W { background: var(--positive); }
  .form-D { background: var(--warn); }
  .form-L { background: var(--danger); }

  /* Insight cards */
  .insight-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-left: 3px solid;
    border-radius: var(--r);
    padding: 14px 16px;
    display: flex;
    gap: 12px;
    align-items: flex-start;
  }
  .insight-positive { border-left-color: var(--positive); }
  .insight-negative { border-left-color: var(--danger); }
  .insight-warning  { border-left-color: var(--warn); }
  .insight-info     { border-left-color: var(--accent2); }
  .insight-icon { font-size: 18px; margin-top: 1px; }
  .insight-title { font-weight: 600; font-size: 13px; margin-bottom: 2px; }
  .insight-message { color: var(--muted); font-size: 13px; line-height: 1.5; }

  /* Buttons */
  .btn {
    display: inline-flex; align-items: center; gap: 6px;
    padding: 9px 18px;
    border-radius: var(--r);
    font-size: 13px;
    font-weight: 600;
    border: none;
    transition: all .15s;
    white-space: nowrap;
  }
  .btn-primary { background: var(--accent); color: #000; }
  .btn-primary:hover { background: #00ff8a; }
  .btn-ghost { background: transparent; border: 1px solid var(--border2); color: var(--text); }
  .btn-ghost:hover { border-color: var(--text); }
  .btn-danger { background: rgba(255,82,82,.1); color: var(--danger); border: 1px solid rgba(255,82,82,.2); }
  .btn-danger:hover { background: rgba(255,82,82,.2); }
  .btn-sm { padding: 6px 12px; font-size: 12px; }
  .btn:disabled { opacity: .4; cursor: not-allowed; }

  /* Forms */
  .form-group { display: flex; flex-direction: column; gap: 6px; }
  .label { font-size: 12px; font-weight: 600; color: var(--muted); letter-spacing: .4px; text-transform: uppercase; }
  .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
  .form-row-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; }

  /* Table */
  .table-wrap { overflow-x: auto; }
  table { width: 100%; border-collapse: collapse; }
  th { text-align: left; font-size: 11px; font-weight: 700; letter-spacing: .8px; text-transform: uppercase; color: var(--muted); padding: 8px 12px; border-bottom: 1px solid var(--border); }
  td { padding: 12px 12px; border-bottom: 1px solid var(--border); font-size: 13px; }
  tr:last-child td { border-bottom: none; }
  tr:hover td { background: rgba(255,255,255,.02); }

  /* Auth page */
  .auth-wrap {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--bg);
    padding: 24px;
  }
  .auth-card {
    width: 100%;
    max-width: 400px;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 14px;
    padding: 40px;
  }
  .auth-logo { font-family: var(--font-head); font-size: 28px; font-weight: 800; color: var(--accent); margin-bottom: 4px; }
  .auth-tagline { color: var(--muted); font-size: 13px; margin-bottom: 32px; }

  /* Error */
  .error-msg { color: var(--danger); font-size: 13px; margin-top: 8px; }

  /* Modal */
  .modal-overlay {
    position: fixed; inset: 0;
    background: rgba(0,0,0,.7);
    display: flex; align-items: center; justify-content: center;
    z-index: 100; padding: 24px;
  }
  .modal {
    background: var(--surface);
    border: 1px solid var(--border2);
    border-radius: 12px;
    padding: 28px;
    width: 100%;
    max-width: 540px;
    max-height: 90vh;
    overflow-y: auto;
  }
  .modal-title { font-family: var(--font-head); font-size: 22px; font-weight: 800; margin-bottom: 20px; }
  .modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 24px; }

  /* Tabs */
  .tabs { display: flex; gap: 4px; border-bottom: 1px solid var(--border); margin-bottom: 20px; }
  .tab {
    padding: 10px 16px;
    font-size: 13px;
    font-weight: 500;
    color: var(--muted);
    background: none;
    border: none;
    border-bottom: 2px solid transparent;
    margin-bottom: -1px;
    transition: all .15s;
  }
  .tab:hover { color: var(--text); }
  .tab.active { color: var(--accent); border-bottom-color: var(--accent); }

  /* Divider */
  .divider { height: 1px; background: var(--border); margin: 20px 0; }

  /* Loading */
  .loading { display: flex; align-items: center; justify-content: center; padding: 60px; color: var(--muted); gap: 10px; }
  .spinner {
    width: 20px; height: 20px;
    border: 2px solid var(--border2);
    border-top-color: var(--accent);
    border-radius: 50%;
    animation: spin .7s linear infinite;
  }
  @keyframes spin { to { transform: rotate(360deg); } }

  /* Empty state */
  .empty { padding: 48px; text-align: center; color: var(--muted); }
  .empty-icon { font-size: 36px; margin-bottom: 12px; }
  .empty-title { font-family: var(--font-head); font-size: 18px; font-weight: 700; color: var(--text); margin-bottom: 6px; }

  /* Stats bar */
  .stat-bar-wrap { display: flex; flex-direction: column; gap: 10px; }
  .stat-row { display: grid; grid-template-columns: 1fr auto 1fr; gap: 8px; align-items: center; font-size: 13px; }
  .stat-row-label { font-size: 11px; text-align: center; color: var(--muted); font-weight: 600; }
  .bar-outer { height: 5px; background: var(--border2); border-radius: 3px; overflow: hidden; }
  .bar-inner { height: 100%; border-radius: 3px; background: var(--accent); transition: width .4s; }
  .bar-inner.away { background: var(--accent2); }
  .val-home { text-align: right; font-weight: 600; }
  .val-away { text-align: left; font-weight: 600; }

  /* Probability bar */
  .prob-bar { display: flex; height: 28px; border-radius: 6px; overflow: hidden; gap: 2px; }
  .prob-seg { display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #000; transition: flex .5s; }
  .prob-home { background: var(--accent); }
  .prob-draw { background: var(--warn); }
  .prob-away { background: var(--accent2); }
`;

// ─────────────────────────────────────────────────────────────────────────────
// HELPERS
// ─────────────────────────────────────────────────────────────────────────────
function fmtDate(d) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}
function fmtScore(m) { return `${m.homeScore} — ${m.awayScore}`; }
function statusBadge(s) {
  const map = { 2: ['badge-completed', 'FT'], 1: ['badge-live', 'LIVE'], 0: ['badge-scheduled', 'SCH'] };
  const [cls, label] = map[s] || ['badge-scheduled', '—'];
  return <span className={`match-badge ${cls}`}>{label}</span>;
}
function insightIcon(level) {
  return { positive: '✅', negative: '⚠️', warning: '🔶', info: 'ℹ️' }[level] || 'ℹ️';
}

function FormDots({ form }) {
  if (!form) return null;
  return (
    <div className="form-dots">
      {form.split('').map((c, i) => (
        <div key={i} className={`form-dot form-${c}`}>{c}</div>
      ))}
    </div>
  );
}

function InsightCard({ insight }) {
  return (
    <div className={`insight-card insight-${insight.level}`}>
      <span className="insight-icon">{insightIcon(insight.level)}</span>
      <div>
        <div className="insight-title">{insight.title}</div>
        <div className="insight-message">{insight.message}</div>
      </div>
    </div>
  );
}

function StatBar({ label, home, away, max }) {
  const hPct = max ? (home / max) * 100 : 50;
  const aPct = max ? (away / max) * 100 : 50;
  return (
    <div>
      <div className="stat-row">
        <div style={{ textAlign: 'right' }} className="val-home">{home}</div>
        <div className="stat-row-label">{label}</div>
        <div className="val-away">{away}</div>
      </div>
      <div className="stat-row" style={{ gap: '8px' }}>
        <div className="bar-outer" style={{ transform: 'scaleX(-1)' }}>
          <div className="bar-inner" style={{ width: `${hPct}%` }} />
        </div>
        <div style={{ width: 24 }} />
        <div className="bar-outer">
          <div className="bar-inner away" style={{ width: `${aPct}%` }} />
        </div>
      </div>
    </div>
  );
}

function Loading() {
  return <div className="loading"><div className="spinner" /><span>Loading…</span></div>;
}

function useAsync(fn, deps = []) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { setData(await fn()); }
    catch (e) { setError(e.message); }
    finally { setLoading(false); }
  }, deps);
  useEffect(() => { load(); }, [load]);
  return { data, loading, error, reload: load };
}

// ─────────────────────────────────────────────────────────────────────────────
// AUTH PAGES
// ─────────────────────────────────────────────────────────────────────────────
function AuthPage() {
  const [mode, setMode] = useState('login');
  const [form, setForm] = useState({ email: '', password: '', displayName: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true); setError('');
    try {
      const res = mode === 'login'
        ? await authApi.login({ email: form.email, password: form.password })
        : await authApi.register(form);
      setAuth(res.token, res.user);
      navigate('/');
    } catch (e) { setError(e.message); }
    finally { setLoading(false); }
  };

  return (
    <div className="auth-wrap">
      <div className="auth-card">
        <div className="auth-logo">⚽ Offside IQ</div>
        <div className="auth-tagline">Football analytics platform</div>
        <div className="tabs" style={{ marginBottom: 24 }}>
          <button className={`tab ${mode === 'login' ? 'active' : ''}`} onClick={() => setMode('login')}>Sign In</button>
          <button className={`tab ${mode === 'register' ? 'active' : ''}`} onClick={() => setMode('register')}>Register</button>
        </div>
        <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {mode === 'register' && (
            <div className="form-group">
              <label className="label">Display Name</label>
              <input placeholder="Your name" value={form.displayName} onChange={e => setForm(f => ({ ...f, displayName: e.target.value }))} required />
            </div>
          )}
          <div className="form-group">
            <label className="label">Email</label>
            <input type="email" placeholder="you@example.com" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} required />
          </div>
          <div className="form-group">
            <label className="label">Password</label>
            <input type="password" placeholder="••••••••" value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} required />
          </div>
          {error && <div className="error-msg">⚠ {error}</div>}
          <button className="btn btn-primary" disabled={loading} style={{ marginTop: 4 }}>
            {loading ? 'Loading…' : mode === 'login' ? 'Sign In' : 'Create Account'}
          </button>
        </form>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// SIDEBAR
// ─────────────────────────────────────────────────────────────────────────────
const NAV = [
  { path: '/', label: 'Dashboard', icon: '▦' },
  { path: '/matches', label: 'Matches', icon: '⚽' },
  { path: '/teams', label: 'Teams', icon: '🛡' },
  { path: '/insights', label: 'Insights', icon: '💡' },
];

function Sidebar() {
  const { user, logout } = useAuthStore();
  const [activePath, setActivePath] = useState(window.location.pathname);

  useEffect(() => {
    const handler = () => setActivePath(window.location.pathname);
    window.addEventListener('popstate', handler);
    return () => window.removeEventListener('popstate', handler);
  }, []);

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <span>OFFSIDE IQ</span>
        <small>Analytics Platform</small>
      </div>
      <nav>
        {NAV.map(n => (
          <Link key={n.path} to={n.path} className={`nav-item ${activePath === n.path ? 'active' : ''}`} onClick={() => setActivePath(n.path)}>
            <span className="nav-icon">{n.icon}</span>
            {n.label}
          </Link>
        ))}
      </nav>
      <div className="sidebar-user">
        <div className="avatar">{user?.displayName?.[0]?.toUpperCase() || 'U'}</div>
        <div className="sidebar-user-info">
          <strong>{user?.displayName}</strong>
          <span>{user?.role}</span>
        </div>
        <button className="btn-logout" onClick={logout} title="Sign out">⏻</button>
      </div>
    </aside>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// DASHBOARD
// ─────────────────────────────────────────────────────────────────────────────
function Dashboard() {
  const { data, loading } = useAsync(() => dashboardApi.get());

  if (loading) return <Loading />;
  if (!data) return <div className="page"><div className="empty"><div className="empty-icon">📭</div><div className="empty-title">No data yet</div><p>Add some teams and matches to get started.</p></div></div>;

  const { recentMatches = [], insights = [], teamForms = [], stats } = data;

  const chartData = recentMatches.slice(0, 8).reverse().map(m => ({
    name: `${m.homeTeam?.shortCode} v ${m.awayTeam?.shortCode}`,
    goals: m.homeScore + m.awayScore,
    home: m.homeScore,
    away: m.awayScore,
  }));

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-title">Dashboard</div>
          <div className="page-subtitle">Your football analytics overview</div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid-4" style={{ marginBottom: 24 }}>
        {[
          { label: 'Total Matches', value: stats?.totalMatches ?? 0, sub: 'all time' },
          { label: 'Teams Tracked', value: stats?.totalTeams ?? 0, sub: 'registered' },
          { label: 'Avg Goals', value: (stats?.avgGoalsPerMatch ?? 0).toFixed(1), sub: 'per match' },
          { label: 'This Month', value: stats?.matchesThisMonth ?? 0, sub: 'matches' },
        ].map(s => (
          <div key={s.label} className="stat-card">
            <div className="stat-label">{s.label}</div>
            <div className="stat-value">{s.value}</div>
            <div className="stat-sub">{s.sub}</div>
          </div>
        ))}
      </div>

      <div className="grid-2" style={{ marginBottom: 24 }}>
        {/* Goals chart */}
        <div className="card">
          <div className="card-title">Goals — Recent Matches</div>
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={180}>
              <BarChart data={chartData} barSize={14}>
                <XAxis dataKey="name" tick={{ fill: '#6b7280', fontSize: 10 }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: '#6b7280', fontSize: 10 }} axisLine={false} tickLine={false} />
                <Tooltip contentStyle={{ background: '#111418', border: '1px solid #1e2228', borderRadius: 8, fontSize: 12 }} />
                <Bar dataKey="home" name="Home" fill="#00e676" radius={[3, 3, 0, 0]} />
                <Bar dataKey="away" name="Away" fill="#00bcd4" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : <div className="empty" style={{ padding: 40 }}><p>No match data yet</p></div>}
        </div>

        {/* Team forms */}
        <div className="card">
          <div className="card-title">Team Form (Last 5)</div>
          {teamForms.length > 0 ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              {teamForms.map(f => (
                <div key={f.teamId} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
                  <div style={{ fontWeight: 600, fontSize: 14, flex: 1 }}>{f.teamName}</div>
                  <FormDots form={f.formString} />
                  <div style={{ color: '#6b7280', fontSize: 12, minWidth: 50, textAlign: 'right' }}>{f.wins}W {f.draws}D {f.losses}L</div>
                </div>
              ))}
            </div>
          ) : <div className="empty" style={{ padding: 30 }}><p>No teams tracked yet</p></div>}
        </div>
      </div>

      {/* Insights */}
      {insights.length > 0 && (
        <div className="card" style={{ marginBottom: 24 }}>
          <div className="card-title">Latest Insights</div>
          <div className="gap-16">
            {insights.map((ins, i) => <InsightCard key={i} insight={ins} />)}
          </div>
        </div>
      )}

      {/* Recent matches */}
      <div className="card">
        <div className="card-title" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          Recent Matches
          <Link to="/matches" style={{ color: 'var(--accent)', fontSize: 12, textTransform: 'none', letterSpacing: 0 }}>View all →</Link>
        </div>
        <div className="gap-16">
          {recentMatches.length === 0
            ? <div className="empty" style={{ padding: 24 }}><p>No matches yet. <Link to="/matches" style={{ color: 'var(--accent)' }}>Add your first match →</Link></p></div>
            : recentMatches.slice(0, 5).map(m => <MatchRow key={m.id} match={m} />)}
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// MATCHES
// ─────────────────────────────────────────────────────────────────────────────
function MatchRow({ match: m }) {
  const navigate = useNavigate();
  return (
    <div className="match-card" onClick={() => navigate(`/matches/${m.id}`)}>
      <div className="match-teams">
        <div className="team-name home">{m.homeTeam?.name}</div>
        <div className={`score-block ${m.status === 1 ? 'live' : ''}`}>{fmtScore(m)}</div>
        <div className="team-name away">{m.awayTeam?.name}</div>
      </div>
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4 }}>
        {statusBadge(m.status)}
        <div className="match-meta">{fmtDate(m.matchDate)}</div>
        {m.competition && <div className="match-meta">{m.competition}</div>}
      </div>
    </div>
  );
}

function CreateMatchModal({ teams, onClose, onCreated }) {
  const [form, setForm] = useState({
    homeTeamId: '', awayTeamId: '', homeScore: 0, awayScore: 0,
    matchDate: new Date().toISOString().slice(0, 16),
    competition: '', venue: '', status: 2,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true); setError('');
    try {
      await matchesApi.create({ ...form, homeScore: +form.homeScore, awayScore: +form.awayScore, status: +form.status });
      onCreated();
    } catch (e) { setError(e.message); }
    finally { setLoading(false); }
  };

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-title">New Match</div>
        <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="form-row">
            <div className="form-group">
              <label className="label">Home Team</label>
              <select value={form.homeTeamId} onChange={e => set('homeTeamId', e.target.value)} required>
                <option value="">Select team…</option>
                {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="label">Away Team</label>
              <select value={form.awayTeamId} onChange={e => set('awayTeamId', e.target.value)} required>
                <option value="">Select team…</option>
                {teams.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="label">Home Score</label>
              <input type="number" min={0} value={form.homeScore} onChange={e => set('homeScore', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="label">Away Score</label>
              <input type="number" min={0} value={form.awayScore} onChange={e => set('awayScore', e.target.value)} />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="label">Date & Time</label>
              <input type="datetime-local" value={form.matchDate} onChange={e => set('matchDate', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="label">Status</label>
              <select value={form.status} onChange={e => set('status', e.target.value)}>
                <option value={0}>Scheduled</option>
                <option value={1}>Live</option>
                <option value={2}>Completed</option>
                <option value={3}>Postponed</option>
              </select>
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="label">Competition</label>
              <input placeholder="e.g. Premier League" value={form.competition} onChange={e => set('competition', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="label">Venue</label>
              <input placeholder="e.g. Anfield" value={form.venue} onChange={e => set('venue', e.target.value)} />
            </div>
          </div>
          {error && <div className="error-msg">{error}</div>}
          <div className="modal-actions">
            <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
            <button className="btn btn-primary" disabled={loading}>{loading ? '…' : 'Create Match'}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

function MatchesPage() {
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const { data: teamsData } = useAsync(() => teamsApi.list());
  const { data, loading, reload } = useAsync(() => matchesApi.list(page, 15), [page]);
  const teams = teamsData || [];
  const { items = [], totalPages = 1 } = data || {};

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-title">Matches</div>
          <div className="page-subtitle">Track and analyze all matches</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ New Match</button>
      </div>

      {loading ? <Loading /> : (
        <div className="gap-16">
          {items.length === 0
            ? <div className="empty"><div className="empty-icon">⚽</div><div className="empty-title">No matches yet</div><p>Create your first match to get started.</p></div>
            : items.map(m => <MatchRow key={m.id} match={m} />)}
        </div>
      )}

      {totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginTop: 24 }}>
          <button className="btn btn-ghost btn-sm" disabled={page === 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
          <span style={{ padding: '6px 12px', color: 'var(--muted)' }}>Page {page} / {totalPages}</span>
          <button className="btn btn-ghost btn-sm" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>Next →</button>
        </div>
      )}

      {showModal && (
        <CreateMatchModal
          teams={teams}
          onClose={() => setShowModal(false)}
          onCreated={() => { setShowModal(false); reload(); }}
        />
      )}
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// MATCH DETAIL
// ─────────────────────────────────────────────────────────────────────────────
function MatchDetail() {
  const { id } = useParams();
  const [tab, setTab] = useState('stats');
  const { data: match, loading } = useAsync(() => matchesApi.get(id), [id]);
  const { data: insights } = useAsync(() => matchesApi.insights(id), [id]);
  const { data: notes, reload: reloadNotes } = useAsync(() => matchesApi.notes(id), [id]);
  const [note, setNote] = useState('');

  const addNote = async () => {
    if (!note.trim()) return;
    try {
      await matchesApi.addNote(id, { content: note, isPublic: false });
      setNote('');
      reloadNotes();
    } catch {}
  };

  if (loading) return <Loading />;
  if (!match) return <div className="page"><p style={{ color: 'var(--muted)' }}>Match not found</p></div>;

  const s = match.stats;

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <Link to="/matches" style={{ color: 'var(--muted)', fontSize: 13 }}>← Matches</Link>
          <div className="page-title" style={{ marginTop: 4 }}>
            {match.homeTeam?.name} vs {match.awayTeam?.name}
          </div>
          <div className="page-subtitle">{match.competition} · {fmtDate(match.matchDate)}</div>
        </div>
        {statusBadge(match.status)}
      </div>

      {/* Score hero */}
      <div className="card" style={{ marginBottom: 24, textAlign: 'center', padding: '32px 20px' }}>
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 32 }}>
          <div style={{ flex: 1, textAlign: 'right' }}>
            <div style={{ fontFamily: 'var(--font-head)', fontSize: 22, fontWeight: 800 }}>{match.homeTeam?.name}</div>
            <div style={{ color: 'var(--muted)', fontSize: 12 }}>HOME</div>
          </div>
          <div style={{ fontFamily: 'var(--font-head)', fontSize: 56, fontWeight: 800, letterSpacing: 4, minWidth: 160, textAlign: 'center', color: 'var(--accent)' }}>
            {match.homeScore} — {match.awayScore}
          </div>
          <div style={{ flex: 1, textAlign: 'left' }}>
            <div style={{ fontFamily: 'var(--font-head)', fontSize: 22, fontWeight: 800 }}>{match.awayTeam?.name}</div>
            <div style={{ color: 'var(--muted)', fontSize: 12 }}>AWAY</div>
          </div>
        </div>
        {match.venue && <div style={{ color: 'var(--muted)', fontSize: 13, marginTop: 12 }}>📍 {match.venue}</div>}
      </div>

      <div className="tabs">
        {['stats', 'insights', 'notes'].map(t => (
          <button key={t} className={`tab ${tab === t ? 'active' : ''}`} onClick={() => setTab(t)}>
            {t.charAt(0).toUpperCase() + t.slice(1)}
          </button>
        ))}
      </div>

      {tab === 'stats' && (
        s ? (
          <div className="grid-2">
            <div className="card">
              <div className="card-title">Match Statistics</div>
              <div className="stat-bar-wrap">
                <StatBar label="Possession %" home={s.homePossession} away={s.awayPossession} max={100} />
                <StatBar label="Shots" home={s.homeShotsTotal} away={s.awayShotsTotal} max={Math.max(s.homeShotsTotal, s.awayShotsTotal, 1)} />
                <StatBar label="On Target" home={s.homeShotsOnTarget} away={s.awayShotsOnTarget} max={Math.max(s.homeShotsOnTarget, s.awayShotsOnTarget, 1)} />
                <StatBar label="Passes" home={s.homePasses} away={s.awayPasses} max={Math.max(s.homePasses, s.awayPasses, 1)} />
                <StatBar label="Pass Accuracy %" home={s.homePassAccuracy} away={s.awayPassAccuracy} max={100} />
                <StatBar label="Corners" home={s.homeCorners} away={s.awayCorners} max={Math.max(s.homeCorners, s.awayCorners, 1)} />
                <StatBar label="Fouls" home={s.homeFouls} away={s.awayFouls} max={Math.max(s.homeFouls, s.awayFouls, 1)} />
                <StatBar label="Yellow Cards" home={s.homeYellowCards} away={s.awayYellowCards} max={Math.max(s.homeYellowCards, s.awayYellowCards, 1)} />
              </div>
            </div>
            <div className="card">
              <div className="card-title">xG & Cards</div>
              {(s.homeXg || s.awayXg) && (
                <div style={{ marginBottom: 20 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
                    <span style={{ color: 'var(--accent)', fontWeight: 700 }}>{s.homeXg?.toFixed(2) ?? '—'} xG</span>
                    <span style={{ color: 'var(--muted)', fontSize: 12 }}>Expected Goals</span>
                    <span style={{ color: 'var(--accent2)', fontWeight: 700 }}>{s.awayXg?.toFixed(2) ?? '—'} xG</span>
                  </div>
                </div>
              )}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                {[
                  ['🟡 Home Yellows', s.homeYellowCards],
                  ['🟡 Away Yellows', s.awayYellowCards],
                  ['🔴 Home Reds', s.homeRedCards],
                  ['🔴 Away Reds', s.awayRedCards],
                ].map(([l, v]) => (
                  <div key={l} style={{ background: 'rgba(255,255,255,.03)', borderRadius: 8, padding: '12px 16px' }}>
                    <div style={{ color: 'var(--muted)', fontSize: 11 }}>{l}</div>
                    <div style={{ fontFamily: 'var(--font-head)', fontSize: 28, fontWeight: 800 }}>{v}</div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : (
          <div className="empty"><div className="empty-icon">📊</div><div className="empty-title">No stats recorded</div><p>Add match statistics to see the breakdown.</p></div>
        )
      )}

      {tab === 'insights' && (
        <div className="gap-16">
          {(insights?.length > 0)
            ? insights.map((ins, i) => <InsightCard key={i} insight={ins} />)
            : <div className="empty"><div className="empty-icon">💡</div><div className="empty-title">No insights yet</div><p>Insights generate from completed matches with recorded stats.</p></div>}
        </div>
      )}

      {tab === 'notes' && (
        <div>
          <div className="card" style={{ marginBottom: 16 }}>
            <div className="card-title">Add Note</div>
            <div style={{ display: 'flex', gap: 10 }}>
              <textarea rows={2} placeholder="Add your match observations…" value={note} onChange={e => setNote(e.target.value)} style={{ resize: 'vertical' }} />
              <button className="btn btn-primary btn-sm" onClick={addNote} style={{ alignSelf: 'flex-end', whiteSpace: 'nowrap' }}>Add</button>
            </div>
          </div>
          <div className="gap-16">
            {(notes || []).map(n => (
              <div key={n.id} className="card" style={{ borderLeft: '2px solid var(--border2)' }}>
                <div style={{ color: 'var(--muted)', fontSize: 11, marginBottom: 6 }}>{n.authorName} · {fmtDate(n.createdAt)}</div>
                <p style={{ fontSize: 14 }}>{n.content}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// TEAMS
// ─────────────────────────────────────────────────────────────────────────────
function CreateTeamModal({ onClose, onCreated }) {
  const [form, setForm] = useState({ name: '', shortCode: '', league: '', country: '', stadium: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true); setError('');
    try { await teamsApi.create(form); onCreated(); }
    catch (e) { setError(e.message); }
    finally { setLoading(false); }
  };

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal-title">Create Team</div>
        <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="form-row">
            <div className="form-group">
              <label className="label">Team Name</label>
              <input placeholder="Arsenal FC" value={form.name} onChange={e => set('name', e.target.value)} required />
            </div>
            <div className="form-group">
              <label className="label">Short Code</label>
              <input placeholder="ARS" maxLength={5} value={form.shortCode} onChange={e => set('shortCode', e.target.value.toUpperCase())} required />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="label">League</label>
              <input placeholder="Premier League" value={form.league} onChange={e => set('league', e.target.value)} />
            </div>
            <div className="form-group">
              <label className="label">Country</label>
              <input placeholder="England" value={form.country} onChange={e => set('country', e.target.value)} />
            </div>
          </div>
          <div className="form-group">
            <label className="label">Stadium</label>
            <input placeholder="Emirates Stadium" value={form.stadium} onChange={e => set('stadium', e.target.value)} />
          </div>
          {error && <div className="error-msg">{error}</div>}
          <div className="modal-actions">
            <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
            <button className="btn btn-primary" disabled={loading}>{loading ? '…' : 'Create Team'}</button>
          </div>
        </form>
      </div>
    </div>
  );
}

function TeamsPage() {
  const [showModal, setShowModal] = useState(false);
  const [h2hA, setH2hA] = useState('');
  const [h2hB, setH2hB] = useState('');
  const [h2hData, setH2hData] = useState(null);
  const { data: teams, loading, reload } = useAsync(() => teamsApi.list());

  const fetchH2H = async () => {
    if (!h2hA || !h2hB) return;
    try { setH2hData(await h2hApi.get(h2hA, h2hB)); }
    catch (e) { alert(e.message); }
  };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-title">Teams</div>
          <div className="page-subtitle">Manage squads and view form</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ New Team</button>
      </div>

      {loading ? <Loading /> : (
        <>
          {/* Team cards grid */}
          {(teams || []).length === 0
            ? <div className="empty"><div className="empty-icon">🛡</div><div className="empty-title">No teams yet</div><p>Create a team to start tracking performance.</p></div>
            : (
              <div className="grid-3" style={{ marginBottom: 28 }}>
                {(teams || []).map(t => <TeamCard key={t.id} team={t} />)}
              </div>
            )}

          {/* Head to head */}
          {(teams || []).length >= 2 && (
            <div className="card">
              <div className="card-title">Head to Head</div>
              <div style={{ display: 'flex', gap: 12, alignItems: 'flex-end', flexWrap: 'wrap' }}>
                <div className="form-group" style={{ flex: 1, minWidth: 180 }}>
                  <label className="label">Team A</label>
                  <select value={h2hA} onChange={e => setH2hA(e.target.value)}>
                    <option value="">Select…</option>
                    {(teams || []).map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                  </select>
                </div>
                <div className="form-group" style={{ flex: 1, minWidth: 180 }}>
                  <label className="label">Team B</label>
                  <select value={h2hB} onChange={e => setH2hB(e.target.value)}>
                    <option value="">Select…</option>
                    {(teams || []).map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                  </select>
                </div>
                <button className="btn btn-primary" onClick={fetchH2H}>Compare →</button>
              </div>

              {h2hData && <H2HResult data={h2hData} />}
            </div>
          )}
        </>
      )}

      {showModal && <CreateTeamModal onClose={() => setShowModal(false)} onCreated={() => { setShowModal(false); reload(); }} />}
    </div>
  );
}

function TeamCard({ team }) {
  const navigate = useNavigate();
  const { data: form } = useAsync(() => teamsApi.form(team.id), [team.id]);

  return (
    <div className="card" style={{ cursor: 'pointer', transition: 'border-color .15s' }}
      onMouseEnter={e => e.currentTarget.style.borderColor = 'var(--border2)'}
      onMouseLeave={e => e.currentTarget.style.borderColor = 'var(--border)'}
      onClick={() => navigate(`/teams/${team.id}`)}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 14 }}>
        <div>
          <div style={{ fontFamily: 'var(--font-head)', fontSize: 18, fontWeight: 800 }}>{team.name}</div>
          <div style={{ color: 'var(--muted)', fontSize: 12 }}>{team.league} · {team.country}</div>
        </div>
        <div style={{
          background: 'rgba(0,230,118,.1)', color: 'var(--accent)',
          fontFamily: 'var(--font-head)', fontWeight: 800, fontSize: 14,
          padding: '4px 8px', borderRadius: 6
        }}>{team.shortCode}</div>
      </div>
      {team.stadium && <div style={{ color: 'var(--muted)', fontSize: 12, marginBottom: 12 }}>📍 {team.stadium}</div>}
      {form && (
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <FormDots form={form.formString} />
          <div style={{ color: 'var(--muted)', fontSize: 11 }}>{form.wins}W {form.draws}D {form.losses}L</div>
        </div>
      )}
    </div>
  );
}

function H2HResult({ data: d }) {
  const total = d.totalMatches;
  if (total === 0) return <div className="empty" style={{ padding: 24 }}><p>No head-to-head matches found.</p></div>;

  return (
    <div style={{ marginTop: 24 }}>
      <div className="divider" />
      <div style={{ textAlign: 'center', marginBottom: 20 }}>
        <div style={{ fontFamily: 'var(--font-head)', fontSize: 13, color: 'var(--muted)', letterSpacing: 1, textTransform: 'uppercase', marginBottom: 12 }}>
          {total} matches played
        </div>
        <div style={{ display: 'flex', justifyContent: 'center', gap: 32 }}>
          {[
            [d.teamA?.name, d.teamAWins, 'var(--accent)'],
            ['Draws', d.draws, 'var(--warn)'],
            [d.teamB?.name, d.teamBWins, 'var(--accent2)'],
          ].map(([label, val, color]) => (
            <div key={label} style={{ textAlign: 'center' }}>
              <div style={{ fontFamily: 'var(--font-head)', fontSize: 40, fontWeight: 800, color }}>{val}</div>
              <div style={{ color: 'var(--muted)', fontSize: 12 }}>{label}</div>
            </div>
          ))}
        </div>
      </div>
      <div className="prob-bar" style={{ marginBottom: 8 }}>
        <div className="prob-seg prob-home" style={{ flex: d.teamAWins || 0.1 }}>{d.teamAWins > 0 ? `${Math.round(d.teamAWins / total * 100)}%` : ''}</div>
        <div className="prob-seg prob-draw" style={{ flex: d.draws || 0.1 }}>{d.draws > 0 ? `${Math.round(d.draws / total * 100)}%` : ''}</div>
        <div className="prob-seg prob-away" style={{ flex: d.teamBWins || 0.1 }}>{d.teamBWins > 0 ? `${Math.round(d.teamBWins / total * 100)}%` : ''}</div>
      </div>
      {d.recentMatches?.length > 0 && (
        <div style={{ marginTop: 16 }}>
          <div className="card-title">Recent Meetings</div>
          <div className="gap-16">
            {d.recentMatches.map(m => <MatchRow key={m.id} match={m} />)}
          </div>
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// TEAM DETAIL
// ─────────────────────────────────────────────────────────────────────────────
function TeamDetail() {
  const { id } = useParams();
  const { data: team } = useAsync(() => teamsApi.get(id), [id]);
  const { data: form } = useAsync(() => teamsApi.form(id), [id]);
  const { data: insights } = useAsync(() => insightsApi.forTeam(id), [id]);

  if (!team) return <Loading />;

  const chartData = (form?.last5 || []).map((r, i) => ({
    match: `M${i + 1}`, gf: r.goalsFor, ga: r.goalsAgainst,
  })).reverse();

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <Link to="/teams" style={{ color: 'var(--muted)', fontSize: 13 }}>← Teams</Link>
          <div className="page-title" style={{ marginTop: 4 }}>{team.name}</div>
          <div className="page-subtitle">{team.league} · {team.country}</div>
        </div>
        <div style={{
          background: 'rgba(0,230,118,.1)', color: 'var(--accent)',
          fontFamily: 'var(--font-head)', fontWeight: 800, fontSize: 20,
          padding: '8px 16px', borderRadius: 8
        }}>{team.shortCode}</div>
      </div>

      <div className="grid-2" style={{ marginBottom: 24 }}>
        <div className="card">
          <div className="card-title">Current Form</div>
          {form ? (
            <>
              <div style={{ display: 'flex', gap: 16, alignItems: 'center', marginBottom: 16 }}>
                <FormDots form={form.formString} />
                <div style={{ color: 'var(--muted)', fontSize: 13 }}>
                  {form.wins}W · {form.draws}D · {form.losses}L
                </div>
              </div>
              <div className="grid-2" style={{ gap: 10 }}>
                {[
                  ['Avg Goals Scored', form.avgGoalsScored?.toFixed(1)],
                  ['Avg Goals Conceded', form.avgGoalsConceded?.toFixed(1)],
                  ['Win Rate', `${form.winRate?.toFixed(0)}%`],
                ].map(([l, v]) => (
                  <div key={l} style={{ background: 'rgba(255,255,255,.03)', borderRadius: 8, padding: '10px 14px' }}>
                    <div style={{ color: 'var(--muted)', fontSize: 11 }}>{l}</div>
                    <div style={{ fontFamily: 'var(--font-head)', fontSize: 24, fontWeight: 800, color: 'var(--accent)' }}>{v}</div>
                  </div>
                ))}
              </div>
            </>
          ) : <div className="empty" style={{ padding: 24 }}><p>No match data yet</p></div>}
        </div>

        <div className="card">
          <div className="card-title">Goals — Last 5</div>
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height={160}>
              <LineChart data={chartData}>
                <CartesianGrid stroke="#1e2228" vertical={false} />
                <XAxis dataKey="match" tick={{ fill: '#6b7280', fontSize: 11 }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: '#6b7280', fontSize: 11 }} axisLine={false} tickLine={false} />
                <Tooltip contentStyle={{ background: '#111418', border: '1px solid #1e2228', borderRadius: 8, fontSize: 12 }} />
                <Line type="monotone" dataKey="gf" name="Scored" stroke="#00e676" strokeWidth={2} dot={{ r: 4 }} />
                <Line type="monotone" dataKey="ga" name="Conceded" stroke="#ff5252" strokeWidth={2} dot={{ r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          ) : <div className="empty" style={{ padding: 40 }}><p>Play some matches first</p></div>}
        </div>
      </div>

      {insights?.length > 0 && (
        <div className="card">
          <div className="card-title">Team Insights</div>
          <div className="gap-16">
            {insights.map((ins, i) => <InsightCard key={i} insight={ins} />)}
          </div>
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// INSIGHTS
// ─────────────────────────────────────────────────────────────────────────────
function InsightsPage() {
  const { data: teams } = useAsync(() => teamsApi.list());
  const { data: global, loading } = useAsync(() => insightsApi.global());
  const [predHome, setPredHome] = useState('');
  const [predAway, setPredAway] = useState('');
  const [prediction, setPrediction] = useState(null);
  const [predLoading, setPredLoading] = useState(false);

  const runPrediction = async () => {
    if (!predHome || !predAway) return;
    setPredLoading(true);
    try { setPrediction(await insightsApi.predict(predHome, predAway)); }
    catch (e) { alert(e.message); }
    finally { setPredLoading(false); }
  };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-title">Insights</div>
          <div className="page-subtitle">AI-powered analytics and predictions</div>
        </div>
      </div>

      {/* Global insights */}
      <div className="card" style={{ marginBottom: 24 }}>
        <div className="card-title">Global Insights</div>
        {loading ? <Loading /> : (
          (global || []).length > 0
            ? <div className="gap-16">{global.map((ins, i) => <InsightCard key={i} insight={ins} />)}</div>
            : <div className="empty" style={{ padding: 24 }}><p>Add completed matches to generate insights.</p></div>
        )}
      </div>

      {/* Match predictor */}
      <div className="card">
        <div className="card-title">Match Predictor</div>
        <div style={{ display: 'flex', gap: 12, alignItems: 'flex-end', flexWrap: 'wrap', marginBottom: 16 }}>
          <div className="form-group" style={{ flex: 1, minWidth: 180 }}>
            <label className="label">Home Team</label>
            <select value={predHome} onChange={e => setPredHome(e.target.value)}>
              <option value="">Select…</option>
              {(teams || []).map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          <div style={{ padding: '9px 0', color: 'var(--muted)', fontWeight: 700 }}>vs</div>
          <div className="form-group" style={{ flex: 1, minWidth: 180 }}>
            <label className="label">Away Team</label>
            <select value={predAway} onChange={e => setPredAway(e.target.value)}>
              <option value="">Select…</option>
              {(teams || []).map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          <button className="btn btn-primary" onClick={runPrediction} disabled={predLoading || !predHome || !predAway}>
            {predLoading ? 'Predicting…' : 'Predict'}
          </button>
        </div>

        {prediction && (
          <div style={{ marginTop: 8 }}>
            <div className="divider" />
            <div style={{ textAlign: 'center', marginBottom: 16 }}>
              <div style={{ fontFamily: 'var(--font-head)', fontSize: 13, color: 'var(--muted)', letterSpacing: 1, textTransform: 'uppercase', marginBottom: 8 }}>Predicted Outcome</div>
              <div style={{ fontFamily: 'var(--font-head)', fontSize: 28, fontWeight: 800, color: 'var(--accent)' }}>{prediction.predictedOutcome}</div>
            </div>
            <div className="prob-bar" style={{ marginBottom: 10 }}>
              <div className="prob-seg prob-home" style={{ flex: prediction.homeWinProbability }}>{prediction.homeWinProbability}%</div>
              <div className="prob-seg prob-draw" style={{ flex: prediction.drawProbability }}>{prediction.drawProbability}%</div>
              <div className="prob-seg prob-away" style={{ flex: prediction.awayWinProbability }}>{prediction.awayWinProbability}%</div>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--muted)', fontSize: 12, marginBottom: 16 }}>
              <span>{prediction.homeTeamName}</span>
              <span>Draw</span>
              <span>{prediction.awayTeamName}</span>
            </div>
            <div className="card-title" style={{ marginBottom: 8 }}>Factors</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
              {prediction.factors.map((f, i) => (
                <div key={i} style={{ display: 'flex', gap: 8, color: 'var(--muted)', fontSize: 13 }}>
                  <span style={{ color: 'var(--accent)' }}>→</span> {f}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// APP SHELL
// ─────────────────────────────────────────────────────────────────────────────
function ProtectedLayout() {
  const { token } = useAuthStore();
  if (!token) return <Navigate to="/auth" replace />;
  return (
    <div className="layout">
      <Sidebar />
      <main className="main">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/matches" element={<MatchesPage />} />
          <Route path="/matches/:id" element={<MatchDetail />} />
          <Route path="/teams" element={<TeamsPage />} />
          <Route path="/teams/:id" element={<TeamDetail />} />
          <Route path="/insights" element={<InsightsPage />} />
        </Routes>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <>
      <style>{css}</style>
      <BrowserRouter>
        <Routes>
          <Route path="/auth" element={<AuthPage />} />
          <Route path="/*" element={<ProtectedLayout />} />
        </Routes>
      </BrowserRouter>
    </>
  );
}
