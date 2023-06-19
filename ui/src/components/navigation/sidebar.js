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
            <p className="cursor-pointer" onClick={() => router.replace('/')}>
              Members
            </p>
          </Sidebar.Item>
        </Sidebar.ItemGroup>
      </Sidebar.Items>
    </Sidebar>
  );
};

export default CustomSidebar;
