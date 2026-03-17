import './App.css'
import { useState } from 'react'
import WelcomeCard from './components/WelcomeCard'

function App() {
  const [count, setCount] = useState<number>(0)
  return (
    <div>
      <h1>Hello React 19 + TypeScript</h1>
      <p>Project is running!</p>
      <WelcomeCard title="First component" message="This is a typed functional component." />
      <div>
        <p>Count: {count}</p>
        <button onClick={() => setCount(count + 1)}>Increment</button>
      </div>
    </div>
  )
}

export default App
