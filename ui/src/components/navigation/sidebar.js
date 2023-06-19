'use client';

import { Sidebar } from 'flowbite-react';

const CustomSidebar = () => {
  return (
    <Sidebar className="h-screen absolute z-50">
      <Sidebar.Items>
        <Sidebar.ItemGroup>
          <Sidebar.Item>
            <p>Members</p>
          </Sidebar.Item>
        </Sidebar.ItemGroup>
      </Sidebar.Items>
    </Sidebar>
  );
};

export default CustomSidebar;
