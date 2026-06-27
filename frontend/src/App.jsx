import { Routes, Route, Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from './AuthContext'
import MoviesPage from './pages/MoviesPage'
import MovieDetailPage from './pages/MovieDetailPage'
import SeatMapPage from './pages/SeatMapPage'
import MyBookingsPage from './pages/MyBookingsPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

function RequireAuth({ children }) {
  const { user, loading } = useAuth()
  if (loading) return null
  if (!user) return <Navigate to="/login" replace />
  return children
}

function Topbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  return (
    <header className="topbar">
      <Link to="/" className="brand">
        Cine<span className="brand-accent">Book</span>
      </Link>
      <nav className="nav-links">
        <Link className="nav-link" to="/">Now Showing</Link>
        {user && <Link className="nav-link" to="/my-bookings">My Bookings</Link>}
        {user ? (
          <div className="nav-user">
            <span>{user.name || user.email}</span>
            <button
              className="btn-ghost"
              onClick={() => {
                logout()
                navigate('/login')
              }}
            >
              Sign out
            </button>
          </div>
        ) : (
          <Link className="btn-ghost" to="/login">Sign in</Link>
        )}
      </nav>
    </header>
  )
}

export default function App() {
  return (
    <div className="app-shell">
      <Topbar />
      <main className="main">
        <Routes>
          <Route path="/" element={<MoviesPage />} />
          <Route path="/movies/:id" element={<MovieDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/screenings/:id/seats"
            element={
              <RequireAuth>
                <SeatMapPage />
              </RequireAuth>
            }
          />
          <Route
            path="/my-bookings"
            element={
              <RequireAuth>
                <MyBookingsPage />
              </RequireAuth>
            }
          />
        </Routes>
      </main>
    </div>
  )
}
