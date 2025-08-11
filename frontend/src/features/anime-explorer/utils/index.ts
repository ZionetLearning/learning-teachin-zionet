import { useEffect, useState } from "react";

export const useDebounceValue = <T>(value: T, delay = 300): T => {
  const [debounced, setDebounced] = useState(value);

  useEffect(
    function debounceValue() {
      const handler = setTimeout(() => {
        setDebounced(value);
      }, delay);

      return () => {
        clearTimeout(handler);
      };
    },
    [value, delay],
  );

  return debounced;
};
