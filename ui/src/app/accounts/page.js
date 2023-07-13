'use client';

import { useState } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';

import ManageAccountsSection from '~/components/sections/accounts/manage-accounts';
import TransactionsSection from '~/components/sections/accounts/transactions';

const client = new QueryClient();

export default function Home() {
  const [accountId, setAccountId] = useState();

  return (
    <QueryClientProvider client={client}>
      <main className="flex min-h-screen flex-col items-center justify-between p-24 overflow-hidden p-6">
        <ManageAccountsSection setAccountId={setAccountId} />
        {accountId && <TransactionsSection accountId={accountId} />}
      </main>
    </QueryClientProvider>
  );
}
