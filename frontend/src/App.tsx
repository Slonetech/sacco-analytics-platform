import { useState } from 'react'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="container" style={{ padding: '40px 20px' }}>
      <div className="card">
        <h1 style={{ fontSize: '2rem', marginBottom: '16px', color: '#1e293b' }}>
          SACCO Analytics Platform
        </h1>
        <p style={{ marginBottom: '24px', color: '#64748b' }}>
          Multi-tenant SaaS platform for SACCO financial reporting and analytics
        </p>
        
        <div style={{ display: 'flex', gap: '12px', marginBottom: '24px' }}>
          <button className="btn-primary" onClick={() => setCount((count) => count + 1)}>
            Count is {count}
          </button>
          <button className="btn-secondary" onClick={() => setCount(0)}>
            Reset
          </button>
        </div>

        <div style={{ padding: '16px', backgroundColor: '#f8fafc', borderRadius: '8px' }}>
          <h3 style={{ marginBottom: '8px' }}>🎯 Development Status</h3>
          <ul style={{ paddingLeft: '20px' }}>
            <li>✅ Backend .NET 9 API</li>
            <li>✅ Frontend React + TypeScript</li>
            <li>✅ Docker PostgreSQL + Redis</li>
            <li>⏳ Authentication System (Next)</li>
            <li>⏳ Multi-tenant Database</li>
          </ul>
        </div>
      </div>
    </div>
  )
}

export default App
