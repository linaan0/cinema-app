import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { moviesApi, bookingsApi } from '../api'

export default function MovieDetailPage() {
  const { id } = useParams()
  const [movie, setMovie] = useState(null)
  const [screenings, setScreenings] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    Promise.all([
      moviesApi.get(id).catch(() => null),
      bookingsApi.screenings(id).catch(() => []),
    ])
      .then(([movieData, screeningData]) => {
        if (!active) return
        setMovie(movieData)
        setScreenings(screeningData)
      })
      .catch(() => active && setError('Could not load this movie.'))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [id])

  if (loading) return <p className="page-subtitle">Loading...</p>

  return (
    <div>
      <Link to="/" className="nav-link" style={{ fontSize: 13 }}>← Back to Now Showing</Link>

      {movie ? (
        <>
          <p className="eyebrow" style={{ marginTop: 24 }}>
            {movie.genres?.join(' · ') || 'Feature film'}
          </p>
          <h1 className="page-title">{movie.title}</h1>
          <p className="page-subtitle" style={{ maxWidth: 640 }}>{movie.description}</p>
        </>
      ) : (
        <div className="form-error" style={{ marginTop: 24 }}>
          This movie's catalog entry couldn't be loaded (it may not exist in the Movies service),
          but its showtimes below still come from the Bookings service.
        </div>
      )}

      <h2 className="eyebrow" style={{ marginTop: 16 }}>Showtimes</h2>

      {error && <div className="form-error">{error}</div>}

      {screenings.length === 0 ? (
        <div className="empty-state">No showtimes scheduled for this film yet.</div>
      ) : (
        <div className="screening-list">
          {screenings.map((s) => {
            const low = s.availableSeats <= s.totalSeats * 0.2
            return (
              <Link key={s.id} to={`/screenings/${s.id}/seats`} className="screening-row">
                <div>
                  <div className="screening-time">
                    {new Date(s.startTime).toLocaleString(undefined, {
                      weekday: 'short',
                      hour: '2-digit',
                      minute: '2-digit',
                      month: 'short',
                      day: 'numeric',
                    })}
                  </div>
                  <div className="screening-hall">{s.hallId} · {s.basePrice.toFixed(2)} base price</div>
                </div>
                <div className={`screening-availability ${low ? 'low' : ''}`}>
                  {s.availableSeats} / {s.totalSeats} seats left
                </div>
              </Link>
            )
          })}
        </div>
      )}
    </div>
  )
}
