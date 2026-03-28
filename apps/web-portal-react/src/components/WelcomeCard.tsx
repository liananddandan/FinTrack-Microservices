type WelcomCardProps = {
    title: string
    message: string
}

export default function WelcomeCard({ title, message }: WelcomCardProps) {
    return (
        <section>
            <h2>{title}</h2>
            <p>{message}</p>
        </section>
    )
}