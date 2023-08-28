import { useState, useEffect } from 'react';

export const useDebounce = (value, milliSeconds) => {
    const [debouncedValue, setDebouncedValue] = useState(value);
   
    useEffect(() => {
      const handler = setTimeout(() => {
        setDebouncedValue(value);
      }, milliSeconds);
   
      return () => {
        clearTimeout(handler);
      };
    }, [value, milliSeconds]);
   
    return debouncedValue;
   };