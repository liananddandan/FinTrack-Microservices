import WelcomeCard from '../components/WelcomeCard'

export default function Home() {
  return (
    <div>
      <h1>Home Page</h1>
      <WelcomeCard
        title="First component"
        message="This is a typed functional component."
      />
    </div>
  )
}