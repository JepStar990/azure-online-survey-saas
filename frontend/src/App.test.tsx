import { render, screen, waitFor } from '@testing-library/react'
import { vi } from 'vitest'
import App from './App'

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', async (input: any) => {
      if (typeof input === 'string' && input.endsWith('/api/health')) {
        return { ok: true, json: async () => ({ status: 'Healthy' }) } as any
      }
      if (typeof input === 'string' && input.endsWith('/api/surveys')) {
        return { ok: true, json: async () => ([{ id: 1, title: 'Customer Satisfaction' }]) } as any
      }
      return { ok: false } as any
    })
  })

  it('renders health and surveys', async () => {
    render(<App />)
    await waitFor(() => expect(screen.getByText(/Backend:/)).toBeInTheDocument())
    expect(screen.getByText(/Customer Satisfaction/)).toBeInTheDocument()
  })
})
