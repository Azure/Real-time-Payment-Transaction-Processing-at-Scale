'use client';

import { useState } from 'react';
import ManageAccountsSection from '~/components/sections/manage-accounts';
import TransactionsSection from '~/components/sections/transactions';

export default function Home() {
  const [account, setAccount] = useState('0909090907');
  return (
    <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden p-6">
      <ManageAccountsSection />
      <TransactionsSection account={account} />
    </main>
  );
}
