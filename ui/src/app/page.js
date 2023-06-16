'use client';

import { useState } from 'react';
import TransactionsSection from '~/components/sections/transactions-section';

export default function Home() {
  const [account, setAccount] = useState('0909090907');
  return (
    <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden">
      <TransactionsSection account={account} />
    </main>
  );
}
