'use client';

import { useEffect, useState } from 'react';
import { mutate } from 'swr';

import ManageAccountsSection from '~/components/sections/accounts/manage-accounts';
import TransactionsSection from '~/components/sections/accounts/transactions';

export default function Home() {
  const [accountId, setAccountId] = useState();

  useEffect(() => {
    mutate('transactions');
  }, [accountId]);

  return (
    <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden p-6">
      <ManageAccountsSection setAccountId={setAccountId} />
      {accountId && <TransactionsSection accountId={accountId} />}
    </main>
  );
}
