import { getToken } from './api'

const RECORD_SEPARATOR = '\u001e'

function encode(message) {
  return JSON.stringify(message) + RECORD_SEPARATOR
}

function decodeAll(raw) {
  return raw
      .split(RECORD_SEPARATOR)
      .filter(Boolean)
      .map((chunk) => JSON.parse(chunk))
}

export function connectSeatHub(baseUrl, { onSeatStatusChanged, onOpen, onClose } = {}) {
  const token = getToken()
  
  let wsBase
  if (!baseUrl) {
    const proto = window.location.protocol === 'https:' ? 'wss' : 'ws'
    wsBase = `${proto}://${window.location.host}`
  } else {
    wsBase = baseUrl.replace(/^http/, 'ws')
  }

  const url = `${wsBase}/hubs/seat-availability?access_token=${encodeURIComponent(token || '')}`

  let socket = null
  let handshakeDone = false
  let closedByUser = false

  function send(message) {
    if (socket && socket.readyState === WebSocket.OPEN) {
      socket.send(encode(message))
    }
  }

  function invoke(target, ...args) {
    send({ type: 1, target, arguments: args })
  }

  function open() {
    socket = new WebSocket(url)

    socket.onopen = () => {
      // SignalR handshake: must be sent before any other message.
      socket.send(encode({ protocol: 'json', version: 1 }))
    }

    socket.onmessage = (event) => {
      const messages = decodeAll(event.data)
      for (const msg of messages) {
        if (!handshakeDone) {
          handshakeDone = true
          onOpen?.()
          continue
        }
        if (msg.type === 1 && msg.target === 'SeatStatusChanged') {
          onSeatStatusChanged?.(msg.arguments[0])
        }
      }
    }

    socket.onclose = () => {
      handshakeDone = false
      onClose?.()
      if (!closedByUser) {
        setTimeout(open, 3000)
      }
    }

    socket.onerror = () => {
      socket?.close()
    }
  }

  open()

  return {
    joinScreening: (screeningId) => invoke('JoinScreening', screeningId),
    leaveScreening: (screeningId) => invoke('LeaveScreening', screeningId),
    close: () => {
      closedByUser = true
      socket?.close()
    },
  }
}
