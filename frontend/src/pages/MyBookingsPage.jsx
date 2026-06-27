import { useEffect, useState } from 'react'
import { bookingsApi } from '../api'

export default function MyBookingsPage() {
  const [bookings, setBookings] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    bookingsApi
      .myBookings()
      .then(setBookings)
      .catch(() => setError('Could not load your bookings.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <div>
      <p className="eyebrow">Your tickets</p>
      <h1 className="page-title">My Bookings</h1>
      <p className="page-subtitle">Every confirmed booking, most recent first.</p>

      {loading && <p className="page-subtitle">Loading...</p>}
      {error && <div className="form-error">{error}</div>}

      {!loading && !error && bookings.length === 0 && (
        <div className="empty-state">No bookings yet. Pick a film and grab a seat.</div>
      )}

      {bookings
        .slice()
        .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        .map((booking) => (
          <div className="booking-card" key={booking.id}>
            <div>
              <div className="booking-card-id">Booking #{booking.id.slice(-6).toUpperCase()}</div>
              <div className="booking-card-seats">Seats {booking.seatIds.join(', ')}</div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <div className={`status-badge ${booking.status.toLowerCase()}`}>{booking.status}</div>
              <div style={{ marginTop: 8, fontFamily: 'var(--display)', fontSize: 18 }}>
                {booking.totalPrice.toFixed(2)}
              </div>
            </div>
          </div>
        ))}
    </div>
  )
}
