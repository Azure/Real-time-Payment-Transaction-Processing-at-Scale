import '../globals.css';

export const metadata = {
  title: 'Azure Cosmos DB Payments Demo App',
  description: 'Accounts section'
};

export default function RootLayout({ children }) {
  return (
    <section>
      <div>{children}</div>
    </section>
  );
}
