const API_BASE = import.meta.env.VITE_API_BASE || ''

function url(path) {
    return `${API_BASE}${path}`
}

const TOKEN_KEY = 'cinebook_token'

export function getToken() {
    return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token) {
    if (token) localStorage.setItem(TOKEN_KEY, token)
    else localStorage.removeItem(TOKEN_KEY)
}

async function request(path, { method = 'GET', body, auth = false } = {}) {
    const headers = { 'Content-Type': 'application/json' }

    if (auth) {
        const token = getToken()
        if (token) headers['Authorization'] = `Bearer ${token}`
    }

    const res = await fetch(url(path), {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
    })

    const text = await res.text()
    const data = text ? JSON.parse(text) : null

    if (!res.ok) {
        const message = data?.message || `Request failed (${res.status})`
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
        request('/api/auth/register', { method: 'POST', body: { email, password, name } }),

    login: (email, password) =>
        request('/api/auth/login', { method: 'POST', body: { email, password } }),

    me: () => request('/api/auth/me', { auth: true }),
}

export const moviesApi = {
    list: (search) =>
        request(`/api/movies${search ? `?search=${encodeURIComponent(search)}` : ''}`),

    get: (id) =>
        request(`/api/movies/${id}`),
}

export const bookingsApi = {
    screenings: (movieId) =>
        request(`/api/screenings${movieId ? `?movieId=${movieId}` : ''}`),

    seatMap: (screeningId) =>
        request(`/api/screenings/${screeningId}/seats`, { auth: true }),

    lockSeat: (screeningId, seatId) =>
        request(`/api/bookings/screenings/${screeningId}/seats/${seatId}/lock`, { method: 'POST', auth: true }),

    releaseSeat: (screeningId, seatId) =>
        request(`/api/bookings/screenings/${screeningId}/seats/${seatId}/release`, { method: 'POST', auth: true }),

    confirmBooking: (screeningId, seatIds) =>
        request('/api/bookings', { method: 'POST', auth: true, body: { screeningId, seatIds } }),

    myBookings: () =>
        request('/api/bookings/my', { auth: true }),
}

export const pricingApi = {
    calculate: (basePrice, screeningStartTime) =>
        request('/api/pricing/calculate', {
            method: 'POST',
            body: { basePrice, screeningStartTime },
        }),
}

export const notificationsApi = {
    mine: () =>
        request('/api/notifications/my', { auth: true }),
}