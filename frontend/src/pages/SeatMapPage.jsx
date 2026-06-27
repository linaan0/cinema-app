import { useCallback, useEffect, useRef, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { bookingsApi, ApiError } from '../api'
import { connectSeatHub } from '../signalr'
import Toast from '../components/Toast'

// Groups a flat seat list into rows by their letter prefix ("A1" -> row "A"),
// preserving the order seats arrived in within each row.
function groupByRow(seats) {
  const rows = new Map()
  for (const seat of seats) {
    if (!rows.has(seat.row)) rows.set(seat.row, [])
    rows.get(seat.row).push(seat)
  }
  return Array.from(rows.entries())
}

function seatClass(seat, mySelections) {
  if (seat.status === 'Booked') return 'seat booked'
  if (mySelections.has(seat.seatId)) return 'seat locked-by-me'
  if (seat.status === 'Locked') return 'seat locked-by-other'
  return 'seat available'
}

export default function SeatMapPage() {
  const { id: screeningId } = useParams()
  const navigate = useNavigate()
  const [seats, setSeats] = useState([])
  const [mySelections, setMySelections] = useState(new Set())
  const [loading, setLoading] = useState(true)
  const [confirming, setConfirming] = useState(false)
  const [toast, setToast] = useState(null)
  const [liveConnected, setLiveConnected] = useState(false)
  const hubRef = useRef(null)

  const loadSeatMap = useCallback(() => {
    bookingsApi
      .seatMap(screeningId)
      .then((map) => setSeats(map.seats))
      .catch(() => setToast({ message: 'Could not load the seat map.', kind: 'error' }))
      .finally(() => setLoading(false))
  }, [screeningId])

  useEffect(() => {
    loadSeatMap()

    const hub = connectSeatHub(bookingsApi.baseUrl, {
      onOpen: () => {
        setLiveConnected(true)
        hub.joinScreening(screeningId)
      },
      onClose: () => setLiveConnected(false),
      onSeatStatusChanged: ({ seatId, status }) => {
        setSeats((prev) =>
          prev.map((s) => (s.seatId === seatId ? { ...s, status, lockedByMe: s.lockedByMe } : s))
        )
      },
    })
    hubRef.current = hub

    return () => {
      hub.leaveScreening(screeningId)
      hub.close()
    }
  }, [screeningId, loadSeatMap])

  async function toggleSeat(seat) {
    if (seat.status === 'Booked') return
    if (seat.status === 'Locked' && !mySelections.has(seat.seatId)) return

    if (mySelections.has(seat.seatId)) {
      // release
      try {
        await bookingsApi.releaseSeat(screeningId, seat.seatId)
        setMySelections((prev) => {
          const next = new Set(prev)
          next.delete(seat.seatId)
          return next
        })
        setSeats((prev) =>
          prev.map((s) => (s.seatId === seat.seatId ? { ...s, status: 'Available' } : s))
        )
      } catch (err) {
        setToast({ message: 'Could not release that seat.', kind: 'error' })
      }
      return
    }

    try {
      await bookingsApi.lockSeat(screeningId, seat.seatId)
      setMySelections((prev) => new Set(prev).add(seat.seatId))
      setSeats((prev) =>
        prev.map((s) => (s.seatId === seat.seatId ? { ...s, status: 'Locked', lockedByMe: true } : s))
      )
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setToast({ message: 'Someone just grabbed that seat. Try another one.', kind: 'error' })
        loadSeatMap()
      } else {
        setToast({ message: 'Could not select that seat.', kind: 'error' })
      }
    }
  }

  async function handleConfirm() {
    setConfirming(true)
    try {
      const booking = await bookingsApi.confirmBooking(screeningId, Array.from(mySelections))
      setToast({ message: `Booked! Confirmation total: ${booking.totalPrice.toFixed(2)}`, kind: 'info' })
      setTimeout(() => navigate('/my-bookings'), 1200)
    } catch (err) {
      const message =
        err instanceof ApiError
          ? err.message
          : 'Could not confirm the booking. Some seats may no longer be available.'
      setToast({ message, kind: 'error' })
      loadSeatMap()
      setMySelections(new Set())
    } finally {
      setConfirming(false)
    }
  }

  if (loading) return <p className="page-subtitle">Loading seat map...</p>

  const rows = groupByRow(seats)

  return (
    <div>
      <p className="eyebrow">Pick your seats</p>
      <h1 className="page-title">Seat Map</h1>
      <p className="page-subtitle">
        Selected seats are held for 5 minutes.{' '}
        <span className="live-pill">
          <span className="live-dot" />
          {liveConnected ? 'Live updates connected' : 'Connecting...'}
        </span>
      </p>

      <div className="seat-map-wrap">
        <div className="screen-glow" />
        <div className="screen-label">Screen</div>

        <div className="seat-rows">
          {rows.map(([row, rowSeats]) => (
            <div className="seat-row" key={row}>
              <span className="row-label">{row}</span>
              {rowSeats.map((seat) => (
                <button
                  key={seat.seatId}
                  className={seatClass(seat, mySelections)}
                  disabled={seat.status === 'Booked' || (seat.status === 'Locked' && !mySelections.has(seat.seatId))}
                  onClick={() => toggleSeat(seat)}
                  title={`${seat.seatId} - ${seat.status}`}
                />
              ))}
            </div>
          ))}
        </div>

        <div className="seat-legend">
          <span className="legend-item">
            <span className="legend-swatch" style={{ background: 'var(--teal-dim)', border: '1px solid var(--teal)' }} />
            Available
          </span>
          <span className="legend-item">
            <span className="legend-swatch" style={{ background: 'var(--amber)' }} />
            Your selection
          </span>
          <span className="legend-item">
            <span className="legend-swatch" style={{ background: 'var(--ink-line)', border: '1px solid var(--cream-dim)' }} />
            Locked by someone else
          </span>
          <span className="legend-item">
            <span className="legend-swatch" style={{ background: 'var(--burgundy-dim)', border: '1px solid var(--burgundy)' }} />
            Booked
          </span>
        </div>

        <div className="booking-bar">
          <div className="booking-summary">
            {mySelections.size === 0 ? (
              'No seats selected'
            ) : (
              <>
                <strong>{mySelections.size}</strong> seat{mySelections.size > 1 ? 's' : ''} selected:{' '}
                {Array.from(mySelections).join(', ')}
              </>
            )}
          </div>
          <button
            className="btn-primary"
            disabled={mySelections.size === 0 || confirming}
            onClick={handleConfirm}
          >
            {confirming ? 'Confirming...' : 'Confirm booking'}
          </button>
        </div>
      </div>

      <Toast message={toast?.message} kind={toast?.kind} onDismiss={() => setToast(null)} />
    </div>
  )
}
