import { useEffect } from 'react'

export default function Toast({ message, kind = 'error', onDismiss }) {
  useEffect(() => {
    if (!message) return
    const timer = setTimeout(onDismiss, 4000)
    return () => clearTimeout(timer)
  }, [message, onDismiss])

  if (!message) return null

  return <div className={`toast ${kind === 'info' ? 'info' : ''}`}>{message}</div>
}
