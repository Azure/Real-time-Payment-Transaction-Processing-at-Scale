import React, { useState, useCallback } from 'react';
import { Button, Label, Spinner, TextInput } from 'flowbite-react';

export default function SearchBar({ setQuery }) {
  const [isLoading, setIsLoading] = useState(false);
  const [form, setForm] = useState({
    query: 'How many transactions are there?'
  });

  const onChangeQuery = (e) => setForm({ ...form, query: e.target.value });

  
}