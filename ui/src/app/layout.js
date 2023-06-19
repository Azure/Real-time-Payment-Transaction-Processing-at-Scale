import './globals.css';
import { Inter } from 'next/font/google';
import Sidebar from '~/components/navigation/sidebar';

const inter = Inter({ subsets: ['latin'] });

export const metadata = {
  title: 'Members',
  description: 'Members section'
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Sidebar />
        <div className="sidebar-margin">{children}</div>
      </body>
    </html>
  );
}
