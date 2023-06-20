'use client';

import { Sidebar } from 'flowbite-react';
import { useRouter } from 'next/navigation';

const CustomSidebar = () => {
  const router = useRouter();
  return (
    <Sidebar className="h-screen absolute z-50">
      <Sidebar.Items>
        <Sidebar.ItemGroup>
          <Sidebar.Item>
            <div
              className="cursor-pointer"
              onClick={(e) => {
                e.preventDefault();
                router.push('/');
              }}>
              Members
            </div>
          </Sidebar.Item>
          <Sidebar.Item>
            <div
              className="cursor-pointer"
              onClick={() => {
                router.push('accounts');
              }}>
              Accounts
            </div>
          </Sidebar.Item>
        </Sidebar.ItemGroup>
      </Sidebar.Items>
    </Sidebar>
  );
};

export default CustomSidebar;
