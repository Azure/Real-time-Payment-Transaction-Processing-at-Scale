import '../globals.css';
import { Inter } from 'next/font/google';
import CustomSidebar from '~/components/navigation/sidebar';

const inter = Inter({ subsets: ['latin'] });

export const metadata = {
  title: 'Accounts',
  description: 'Accounts section'
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <CustomSidebar />
        <div className="sidebar-margin">{children}</div>
      </body>
    </html>
  );
}
