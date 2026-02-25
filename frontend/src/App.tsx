import React, {useEffect, useState} from 'react'
import { getHealth, getSurveys } from './api'

export default function App(){
  const [health, setHealth] = useState<string>('...')
  const [surveys, setSurveys] = useState<any[]>([])

  useEffect(()=>{
    getHealth().then(h=>setHealth(h.status))
    getSurveys().then(s=>setSurveys(s))
  },[])

  return (
    <div style={{fontFamily:'Segoe UI, Roboto, sans-serif', padding:20}}>
      <h1>Azure Online Survey (Demo)</h1>
      <p><strong>Backend:</strong> {health}</p>
      <h2>Surveys</h2>
      {surveys.length===0 ? <p>No surveys (unauthenticated demo)</p> : (
        <ul>
          {surveys.map(s=> <li key={s.id}>{s.title}</li>)}
        </ul>
      )}
      <p>Auth integration with Azure AD is a placeholder in the backend.</p>
    </div>
  )
}
