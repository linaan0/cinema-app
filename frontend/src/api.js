// Base URLs for each service. When running behind the k8s Ingress (or an
// API gateway) in production, all of these collapse to the same origin
// with different path prefixes - see the VITE_API_BASE override below.
// For local docker-compose dev, each service is on its own port.
const GATEWAY = import.meta.env.VITE_API_BASE || ''

const BASES = GATEWAY
  ? { auth: GATEWAY, movies: GATEWAY, bookings: GATEWAY, pricing: GATEWAY, notifications: GATEWAY }
  : {
      auth: 'http://localhost:8081',
      movies: 'http://localhost:8082',
      bookings: 'http://localhost:8083',
      pricing: 'http://localhost:8084',
      notifications: 'http://localhost:8085',
    }

const TOKEN_KEY = 'cinebook_token'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

async function request(service, path, { method = 'GET', body, auth = false } = {}) {
  const headers = { 'Content-Type': 'application/json' }
  if (auth) {
    const token = getToken()
    if (token) headers['Authorization'] = `Bearer ${token}`
  }

  const res = await fetch(`${BASES[service]}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  // 204 No Content has no body to parse.
  const text = await res.text()
  const data = text ? JSON.parse(text) : null

  if (!res.ok) {
    const message = data?.message || data?.title || `Request failed (${res.status})`
    throw new ApiError(message, res.status)
  }

  return data
}

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.status = status
  }
}

export const authApi = {
  register: (email, password, name) =>
    request('auth', '/api/auth/register', { method: 'POST', body: { email, password, name } }),
  login: (email, password) =>
    request('auth', '/api/auth/login', { method: 'POST', body: { email, password } }),
  me: () => request('auth', '/api/auth/me', { auth: true }),
}

export const moviesApi = {
  list: (search) => request('movies', `/api/movies${search ? `?search=${encodeURIComponent(search)}` : ''}`),
  get: (id) => request('movies', `/api/movies/${id}`),
}

export const bookingsApi = {
  baseUrl: BASES.bookings,
  screenings: (movieId) =>
    request('bookings', `/api/screenings${movieId ? `?movieId=${movieId}` : ''}`),
  seatMap: (screeningId) =>
    request('bookings', `/api/screenings/${screeningId}/seats`, { auth: true }),
  lockSeat: (screeningId, seatId) =>
    request('bookings', `/api/bookings/screenings/${screeningId}/seats/${seatId}/lock`, {
      method: 'POST',
      auth: true,
    }),
  releaseSeat: (screeningId, seatId) =>
    request('bookings', `/api/bookings/screenings/${screeningId}/seats/${seatId}/release`, {
      method: 'POST',
      auth: true,
    }),
  confirmBooking: (screeningId, seatIds) =>
    request('bookings', '/api/bookings', {
      method: 'POST',
      auth: true,
      body: { screeningId, seatIds },
    }),
  myBookings: () => request('bookings', '/api/bookings/my', { auth: true }),
}

export const pricingApi = {
  calculate: (basePrice, screeningStartTime) =>
    request('pricing', '/api/pricing/calculate', {
      method: 'POST',
      body: { basePrice, screeningStartTime },
    }),
}

export const notificationsApi = {
  mine: () => request('notifications', '/api/notifications/my', { auth: true }),
}
