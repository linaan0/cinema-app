import { createContext, useContext, useEffect, useState, useCallback } from 'react'
import { authApi, getToken, setToken } from './api'

const AuthContext = createContext(null)

function decodeJwtRole(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      role: payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
      name: payload.unique_name || payload.name,
      email: payload.email,
    }
  } catch {
    return {}
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = getToken()
    if (token) {
      const claims = decodeJwtRole(token)
      setUser(claims)
    }
    setLoading(false)
  }, [])

  const login = useCallback(async (email, password) => {
    const res = await authApi.login(email, password)
    setToken(res.token)
    setUser({ name: res.name, email: res.email, role: res.role })
    return res
  }, [])

  const register = useCallback(async (email, password, name) => {
    const res = await authApi.register(email, password, name)
    setToken(res.token)
    setUser({ name: res.name, email: res.email, role: res.role })
    return res
  }, [])

  const logout = useCallback(() => {
    setToken(null)
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
