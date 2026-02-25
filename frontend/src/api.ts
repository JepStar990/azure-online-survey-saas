export async function getHealth(){
  try{
    const res = await fetch('/api/health')
    return await res.json()
  }catch(e){
    return { status: 'offline' }
  }
}

export async function getSurveys(){
  try{
    const res = await fetch('/api/surveys')
    if(!res.ok) return []
    return await res.json()
  }catch(e){
    return []
  }
}
