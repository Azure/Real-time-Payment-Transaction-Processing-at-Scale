import '../globals.css';
import CustomSidebar from '~/components/navigation/sidebar';

export const metadata = {
  title: 'Accounts',
  description: 'Accounts section'
};

export default function RootLayout({ children }) {
  return (
    <section>
      <div>{children}</div>
    </section>
  );
}
