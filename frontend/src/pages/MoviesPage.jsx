import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { moviesApi } from '../api'

export default function MoviesPage() {
  const [movies, setMovies] = useState([])
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    setLoading(true)
    moviesApi
      .list(search || undefined)
      .then((data) => {
        if (active) setMovies(data)
      })
      .catch(() => {
        if (active) setError('Could not load movies. Is the movies-service running?')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [search])

  return (
    <div>
      <p className="eyebrow">This week</p>
      <h1 className="page-title">Now Showing</h1>
      <p className="page-subtitle">Pick a film, then a showtime, then your seat.</p>

      <div className="field" style={{ maxWidth: 320, marginBottom: 32 }}>
        <input
          placeholder="Search by title..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {loading && <p className="page-subtitle">Loading...</p>}
      {error && <div className="form-error">{error}</div>}

      {!loading && !error && movies.length === 0 && (
        <div className="empty-state">
          No movies yet. Add one via the Movies API (admin role required) to get started.
        </div>
      )}

      <div className="movie-grid">
        {movies.map((movie) => (
          <Link key={movie.id} to={`/movies/${movie.id}`} className="movie-card">
            <div className="movie-poster">
              {movie.posterUrl ? (
                <img src={movie.posterUrl} alt={movie.title} />
              ) : (
                movie.title
              )}
            </div>
            <div className="movie-info">
              <h2 className="movie-title">{movie.title}</h2>
              <p className="movie-meta">
                {movie.runtimeMinutes ? `${movie.runtimeMinutes} min` : ''}
                {movie.genres?.length ? ` · ${movie.genres.join(', ')}` : ''}
              </p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  )
}
